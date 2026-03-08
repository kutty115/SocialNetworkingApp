using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SocialNetworkApi.Data;
using SocialNetworkApi.Models;
using SocialNetworkApi.Services;
using System.Security.Claims;

namespace SocialNetworkApi.Realtime;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _db;
    private readonly NotificationService _notify;

    public ChatHub(AppDbContext db, NotificationService notify)
    {
        _db = db;
        _notify = notify;
    }

    public async Task SendMessage(string toUserId, string text)
    {
        var fromUserId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(fromUserId)) return;

        // ✅ Save message to DB (match your Message.cs property names)
        var msg = new Message
        {
            SenderId = fromUserId,
            ReceiverId = toUserId,
            Text = text,
            SentAt = DateTime.UtcNow
        };

        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();

        // ✅ Send realtime message to receiver
        await Clients.User(toUserId).SendAsync("ReceiveMessage", new
        {
            fromUserId,
            toUserId,
            text,
            sentAt = msg.SentAt
        });

        // ✅ Also echo back to sender (so sender sees it instantly)
        await Clients.User(fromUserId).SendAsync("ReceiveMessage", new
        {
            fromUserId,
            toUserId,
            text,
            sentAt = msg.SentAt
        });

        // ✅ Optional: create notification
        await _notify.NewMessage(toUserId, fromUserId, text);
    }
}