namespace MiniDesk.Domain.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }

}