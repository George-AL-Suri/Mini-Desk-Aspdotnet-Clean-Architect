using MiniDesk.Domain.Enums;
namespace MiniDesk.Application.Tickets.Dtos;

public record ChangeTicketStatusRequest(TicketStatus Status);