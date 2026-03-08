using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Data;
using System.Security.Claims;

namespace SocialNetworkApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public NotificationsController(AppDbContext db)
        {
            _db = db;
        }

        private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: api/notifications
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var uid = UserId();
            var data = await _db.Notifications
                .Where(x => x.UserId == uid)
                .OrderByDescending(x => x.CreatedAt)
                .Take(50)
                .Select(x => new {
                    x.Id,
                    x.Type,
                    x.Text,
                    x.Link,
                    x.IsRead,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var uid = UserId();
            var count = await _db.Notifications.CountAsync(x => x.UserId == uid && !x.IsRead);
            return Ok(new { count });
        }

        // PUT: api/notifications/read/5
        [HttpPut("read/{id}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var uid = UserId();
            var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
            if (n == null) return NotFound();

            n.IsRead = true;
            await _db.SaveChangesAsync();
            return Ok(new { message = "marked read" });
        }

        // PUT: api/notifications/read-all
        [HttpPut("read-all")]
        public async Task<IActionResult> ReadAll()
        {
            var uid = UserId();
            var list = await _db.Notifications.Where(x => x.UserId == uid && !x.IsRead).ToListAsync();
            foreach (var n in list) n.IsRead = true;

            await _db.SaveChangesAsync();
            return Ok(new { message = "all read" });
        }
    }
}
