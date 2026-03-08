using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Data;
using SocialNetworkApi.Models;

namespace SocialNetworkApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]

public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;

    public AdminController(UserManager<ApplicationUser> userManager, AppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    [HttpGet("users")]
    public IActionResult Users() =>
        Ok(_userManager.Users.Select(u => new { u.Id, u.Email, u.FullName }).ToList());

    [HttpGet("reports")]
    public async Task<IActionResult> Reports()
    {
        var reports = await _db.Reports
            .Include(r => r.Reporter)
            .Include(r => r.Post)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(reports.Select(r => new
        {
            r.Id,
            Reporter = r.Reporter!.Email,
            r.PostId,
            r.Reason,
            r.Status,
            r.CreatedAt
        }));
    }

    [HttpDelete("posts/{postId}")]
    public async Task<IActionResult> DeletePost(int postId)
    {
        var post = await _db.Posts.FindAsync(postId);
        if (post == null) return NotFound();
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Post removed" });
    }
}