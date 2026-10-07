using MiniDesk.Application.Comments.Dtos;
using MiniDesk.Domain.Entities;
using Riok.Mapperly.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniDesk.Application.Comments.Mappers;

[Mapper]
public static partial class CommentMapper
{
    [MapperIgnoreTarget(nameof(Comment.Id))]
    [MapperIgnoreTarget(nameof(Comment.TicketId))]
    [MapperIgnoreTarget(nameof(Comment.Ticket))]
    [MapperIgnoreTarget(nameof(Comment.AuthorId))]
    [MapperIgnoreTarget(nameof(Comment.CreatedAt))]
    public static partial Comment ToEntity(this CreateCommentRequest request);
}