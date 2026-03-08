using System.ComponentModel.DataAnnotations;

namespace SocialNetworkApi.Models;

public class Friendship
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    [Required]
    public string FriendId { get; set; } = default!;
    public ApplicationUser Friend { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}