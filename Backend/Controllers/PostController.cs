using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Data;
using SocialNetworkApi.Dtos;
using SocialNetworkApi.Models;
using System.Security.Claims;

namespace SocialNetworkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PostsController(AppDbContext db) { _db = db; }

        private string? Me => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePostDto dto)
        {
            var post = new Post { UserId = Me!, Content = dto.Content, ImageUrl = dto.ImageUrl };
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();
            return Ok(post);
        }

        // News Feed: my posts + friends' posts (accepted)
        [Authorize]
        [HttpGet("feed")]
        public async Task<IActionResult> Feed()
        {
            var myId = Me!;
            var friendIds = await _db.FriendRequests
                .Where(fr => fr.Status == FriendRequestStatus.Accepted &&
                            (fr.SenderId == myId || fr.ReceiverId == myId))
                .Select(fr => fr.SenderId == myId ? fr.ReceiverId : fr.SenderId)
                .ToListAsync();

            friendIds.Add(myId);

            var posts = await _db.Posts
                .Where(p => friendIds.Contains(p.UserId))
                .Include(p => p.User)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .Select(p => new
                {
                    p.Id,
                    p.Content,
                    p.ImageUrl,
                    p.CreatedAt,
                    User = new { p.UserId, p.User!.FullName, p.User.ProfileImageUrl },
                    LikeCount = p.Likes.Count,
                    Comments = p.Comments.OrderByDescending(c => c.CreatedAt).Take(5).Select(c => new
                    {
                        c.Id,
                        c.Text,
                        c.CreatedAt,
                        User = new { c.UserId, c.User!.FullName }
                    })
                })
                .ToListAsync();

            return Ok(posts);
        }

        [Authorize]
        [HttpPost("{postId}/like")]
        public async Task<IActionResult> Like(int postId)
        {
            var myId = Me!;
            var exists = await _db.PostLikes.AnyAsync(x => x.PostId == postId && x.UserId == myId);
            if (exists) return Ok(new { message = "Already liked" });

            _db.PostLikes.Add(new PostLike { PostId = postId, UserId = myId });
            await _db.SaveChangesAsync();
            return Ok(new { message = "Liked" });
        }

        [Authorize]
        [HttpPost("comment")]
        public async Task<IActionResult> Comment(AddCommentDto dto)
        {
            var c = new Comment { PostId = dto.PostId, UserId = Me!, Text = dto.Text };
            _db.Comments.Add(c);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Comment added" });
        }
    }
}