namespace SocialNetworkApi.Models;

public enum FriendRequestStatus { Pending, Accepted, Rejected }

public class FriendRequest
{
    public int Id { get; set; }

    public string SenderId { get; set; } = "";
    public ApplicationUser? Sender { get; set; }

    public string ReceiverId { get; set; } = "";
    public ApplicationUser? Receiver { get; set; }

    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}