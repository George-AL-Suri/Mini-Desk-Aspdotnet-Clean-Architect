using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MiniDesk.Application.Categories;
using MiniDesk.Application.Comments;
using MiniDesk.Application.Tickets;

namespace MiniDesk.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);


        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICommentService, CommentService>();


        return services;
    }
}