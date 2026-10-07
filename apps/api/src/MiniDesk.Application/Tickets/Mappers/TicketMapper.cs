using MiniDesk.Application.Tickets.Dtos;
using MiniDesk.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace MiniDesk.Application.Tickets.Mappers;

[Mapper]
public static partial class TicketMapper
{
    [MapperIgnoreTarget(nameof(Ticket.Id))]
    [MapperIgnoreTarget(nameof(Ticket.Status))]
    [MapperIgnoreTarget(nameof(Ticket.Category))]
    [MapperIgnoreTarget(nameof(Ticket.CreatedById))]
    [MapperIgnoreTarget(nameof(Ticket.AssignedToId))]
    [MapperIgnoreTarget(nameof(Ticket.CreatedAt))]
    [MapperIgnoreTarget(nameof(Ticket.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Ticket.Comments))]
    public static partial Ticket ToEntity(this CreateTicketRequest request);

    [MapperIgnoreTarget(nameof(Ticket.Id))]
    [MapperIgnoreTarget(nameof(Ticket.Status))]
    [MapperIgnoreTarget(nameof(Ticket.Category))]
    [MapperIgnoreTarget(nameof(Ticket.CreatedById))]
    [MapperIgnoreTarget(nameof(Ticket.AssignedToId))]
    [MapperIgnoreTarget(nameof(Ticket.CreatedAt))]
    [MapperIgnoreTarget(nameof(Ticket.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Ticket.Comments))]
    public static partial void Apply(this UpdateTicketRequest request, [MappingTarget] Ticket ticket);
}
