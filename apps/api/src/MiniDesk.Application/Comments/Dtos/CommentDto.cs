namespace MiniDesk.Application.Comments.Dtos;

public record CommentDto(
    int Id,
    int TicketId,
    string Content,
    Guid AuthorId,
    string AuthorName,
    DateTime CreatedAt);