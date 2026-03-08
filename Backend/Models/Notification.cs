using System.ComponentModel.DataAnnotations;

namespace SocialNetworkApi.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;   // receiver

        [Required]
        public string Type { get; set; } = string.Empty;     // FriendRequest, Like, Comment, Message etc.

        [Required]
        public string Text { get; set; } = string.Empty;

        public string? Link { get; set; }                    // optional: "/feed", "/friends", "/chat"
        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}