using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Data;
using SocialNetworkApi.Dtos;
using SocialNetworkApi.Models;
using System.Security.Claims;

namespace SocialNetworkApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public FriendsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private string MeId()
    {
        // Works if your JWT includes NameIdentifier claim.
        // If not, use JwtRegisteredClaimNames.Sub claim.
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? throw new Exception("No user id claim found in token.");
    }

    // --------- SEND REQUEST (by email) ----------
    [HttpPost("request")]
    public async Task<IActionResult> SendRequest(SendFriendRequestDto dto)
    {
        var me = MeId();
        var other = await _userManager.FindByEmailAsync(dto.Email);
        if (other == null) return NotFound("User not found.");
        if (other.Id == me) return BadRequest("Cannot send request to yourself.");

        // blocked checks
        var blocked = await _db.UserBlocks.AnyAsync(b =>
            (b.BlockerId == me && b.BlockedId == other.Id) ||
            (b.BlockerId == other.Id && b.BlockedId == me));
        if (blocked) return BadRequest("You cannot friend this user (blocked).");

        // already friends?
        var alreadyFriend = await _db.Friendships.AnyAsync(f => f.UserId == me && f.FriendId == other.Id);
        if (alreadyFriend) return BadRequest("Already friends.");

        // existing pending request either direction?
        var exists = await _db.FriendRequests.AnyAsync(r =>
            ((r.SenderId == me && r.ReceiverId == other.Id) ||
             (r.SenderId == other.Id && r.ReceiverId == me)) &&
            r.Status == FriendRequestStatus.Pending);

        if (exists) return BadRequest("Request already pending.");

        var req = new FriendRequest
        {
            SenderId = me,
            ReceiverId = other.Id,
            Status = FriendRequestStatus.Pending
        };

        _db.FriendRequests.Add(req);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Friend request sent." });
    }

    // --------- INCOMING REQUESTS ----------
    [HttpGet("requests/incoming")]
    public async Task<IActionResult> Incoming()
    {
        var me = MeId();
        var list = await _db.FriendRequests
            .Include(r => r.Sender)
            .Where(r => r.ReceiverId == me && r.Status == FriendRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.CreatedAt,
                sender = new { r.Sender.Id, r.Sender.Email, r.Sender.FullName }
            })
            .ToListAsync();

        return Ok(list);
    }

    // --------- OUTGOING REQUESTS ----------
    [HttpGet("requests/outgoing")]
    public async Task<IActionResult> Outgoing()
    {
        var me = MeId();
        var list = await _db.FriendRequests
            .Include(r => r.Receiver)
            .Where(r => r.SenderId == me && r.Status == FriendRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.CreatedAt,
                receiver = new { r.Receiver.Id, r.Receiver.Email, r.Receiver.FullName }
            })
            .ToListAsync();

        return Ok(list);
    }

    // --------- ACCEPT ----------
    [HttpPost("accept")]
    public async Task<IActionResult> Accept(RespondFriendRequestDto dto)
    {
        var me = MeId();

        var req = await _db.FriendRequests.FirstOrDefaultAsync(r => r.Id == dto.RequestId);
        if (req == null) return NotFound("Request not found.");
        if (req.ReceiverId != me) return Forbid();
        if (req.Status != FriendRequestStatus.Pending) return BadRequest("Request already handled.");

        // block check
        var blocked = await _db.UserBlocks.AnyAsync(b =>
            (b.BlockerId == me && b.BlockedId == req.SenderId) ||
            (b.BlockerId == req.SenderId && b.BlockedId == me));
        if (blocked) return BadRequest("Cannot accept due to block.");

        req.Status = FriendRequestStatus.Accepted;

        // Create friendship in BOTH directions
        _db.Friendships.Add(new Friendship { UserId = req.SenderId, FriendId = req.ReceiverId });
        _db.Friendships.Add(new Friendship { UserId = req.ReceiverId, FriendId = req.SenderId });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Friend request accepted." });
    }

    // --------- REJECT ----------
    [HttpPost("reject")]
    public async Task<IActionResult> Reject(RespondFriendRequestDto dto)
    {
        var me = MeId();

        var req = await _db.FriendRequests.FirstOrDefaultAsync(r => r.Id == dto.RequestId);
        if (req == null) return NotFound("Request not found.");
        if (req.ReceiverId != me) return Forbid();
        if (req.Status != FriendRequestStatus.Pending) return BadRequest("Request already handled.");

        req.Status = FriendRequestStatus.Rejected;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Friend request rejected." });
    }

    // --------- FRIEND LIST ----------
    [HttpGet("list")]
    public async Task<IActionResult> FriendsList()
    {
        var me = MeId();
        var friends = await _db.Friendships
            .Include(f => f.Friend)
            .Where(f => f.UserId == me)
            .OrderBy(f => f.Friend.FullName)
            .Select(f => new { f.Friend.Id, f.Friend.Email, f.Friend.FullName })
            .ToListAsync();

        return Ok(friends);
    }

    // --------- UNFRIEND ----------
    [HttpDelete("unfriend/{friendId}")]
    public async Task<IActionResult> Unfriend(string friendId)
    {
        var me = MeId();

        var a = await _db.Friendships.FirstOrDefaultAsync(x => x.UserId == me && x.FriendId == friendId);
        var b = await _db.Friendships.FirstOrDefaultAsync(x => x.UserId == friendId && x.FriendId == me);

        if (a != null) _db.Friendships.Remove(a);
        if (b != null) _db.Friendships.Remove(b);

        await _db.SaveChangesAsync();
        return Ok(new { message = "Unfriended." });
    }

    // --------- BLOCK ----------
    [HttpPost("block/{userId}")]
    public async Task<IActionResult> Block(string userId)
    {
        var me = MeId();
        if (me == userId) return BadRequest("Cannot block yourself.");

        // remove friendship if exists
        var a = await _db.Friendships.FirstOrDefaultAsync(x => x.UserId == me && x.FriendId == userId);
        var b = await _db.Friendships.FirstOrDefaultAsync(x => x.UserId == userId && x.FriendId == me);
        if (a != null) _db.Friendships.Remove(a);
        if (b != null) _db.Friendships.Remove(b);

        // mark pending requests as rejected
        var pending = await _db.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Pending &&
                        ((r.SenderId == me && r.ReceiverId == userId) ||
                         (r.SenderId == userId && r.ReceiverId == me)))
            .ToListAsync();
        foreach (var r in pending) r.Status = FriendRequestStatus.Rejected;

        // add block
        var exists = await _db.UserBlocks.AnyAsync(x => x.BlockerId == me && x.BlockedId == userId);
        if (!exists)
        {
            _db.UserBlocks.Add(new UserBlock { BlockerId = me, BlockedId = userId });
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "User blocked." });
    }

    // --------- UNBLOCK ----------
    [HttpPost("unblock/{userId}")]
    public async Task<IActionResult> Unblock(string userId)
    {
        var me = MeId();
        var block = await _db.UserBlocks.FirstOrDefaultAsync(x => x.BlockerId == me && x.BlockedId == userId);
        if (block == null) return NotFound("Not blocked.");

        _db.UserBlocks.Remove(block);
        await _db.SaveChangesAsync();
        return Ok(new { message = "User unblocked." });
    }

    // --------- MY BLOCKED LIST ----------
    [HttpGet("blocked")]
    public async Task<IActionResult> Blocked()
    {
        var me = MeId();
        var list = await _db.UserBlocks
            .Include(b => b.Blocked)
            .Where(b => b.BlockerId == me)
            .Select(b => new { b.Blocked.Id, b.Blocked.Email, b.Blocked.FullName })
            .ToListAsync();

        return Ok(list);
    }
}