using MiniDesk.Domain.Enums;

namespace MiniDesk.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid CreatedById { get; set; }
    public Guid? AssignedToId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
