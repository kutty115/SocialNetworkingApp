namespace SocialNetworkApi.Dtos;

public record CreatePostDto(string Content, string? ImageUrl);
public record AddCommentDto(int PostId, string Text);