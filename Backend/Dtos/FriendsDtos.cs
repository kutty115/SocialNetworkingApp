namespace SocialNetworkApi.Dtos;


    public record SendFriendRequestDto(string Email);
    public record RespondFriendRequestDto(int RequestId);
