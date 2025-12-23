using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.DTOs.Board;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Application.Validators.BoardValidators;
using TaskFlow.Application.Validators.UserValidators;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Регистрация репозиториев
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IColumnRepository, ColumnRepository>();
        services.AddScoped<ILabelRepository, LabelRepository>();
        services.AddScoped<ITaskLabelRepository, TaskLabelRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

        // Регистрация валидаторов
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserValidator>();
        services.AddScoped<IValidator<UpdateUserNameRequest>, UpdateUserNameValidator>();
        services.AddScoped<IValidator<UpdateUserProfileRequest>, UpdateUserProfileValidator>();
        services.AddScoped<IValidator<CreateBoardRequest>, CreateBoardValidator>();
        services.AddScoped<IValidator<UpdateBoardRequest>, UpdateBoardValidator>();
        services.AddScoped<IValidator<AddMemberRequest>, AddMemberValidator>();

        return services;
    }
}
