using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialNetworkApi.Data;
using SocialNetworkApi.Models;
using SocialNetworkApi.Realtime;

namespace SocialNetworkApi.Services
{
    public interface INotificationService
    {
        Task NotifyAsync(string toUserId, string type, string text, string? link = null);
        Task<int> UnreadCountAsync(string userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(AppDbContext db, IHubContext<NotificationHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        public async Task NotifyAsync(string toUserId, string type, string text, string? link = null)
        {
            var n = new Notification
            {
                UserId = toUserId,
                Type = type,
                Text = text,
                Link = link,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Notifications.Add(n);
            await _db.SaveChangesAsync();

            // push realtime
            await _hub.Clients.User(toUserId).SendAsync("ReceiveNotification", new
            {
                n.Id,
                n.Type,
                n.Text,
                n.Link,
                n.IsRead,
                n.CreatedAt
            });
        }

        public Task<int> UnreadCountAsync(string userId)
        {
            return _db.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead);
        }
        public Task NewMessage(string toUserId, string fromUserId, string text)
        {
            // TODO: Save notification to DB or send via SignalR notification hub
            return Task.CompletedTask;
        }
    }
}