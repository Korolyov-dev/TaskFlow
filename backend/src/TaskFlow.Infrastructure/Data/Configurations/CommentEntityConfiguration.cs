using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Data.Configurations;

public class CommentEntityConfiguration : IEntityTypeConfiguration<CommentEntity>
{
    public void Configure(EntityTypeBuilder<CommentEntity> builder)
    {
        builder.ToTable("comments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(c => c.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(c => c.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(c => c.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        // Внешние ключи
        builder.HasOne<TaskItemEntity>()
            .WithMany()
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(c => c.TaskId)
            .HasDatabaseName("ix_comments_task_id");

        builder.HasIndex(c => c.UserId)
            .HasDatabaseName("ix_comments_user_id");

        builder.HasIndex(c => c.CreatedAt)
            .HasDatabaseName("ix_comments_created_at");

        builder.HasIndex(c => c.DeletedAt)
            .HasDatabaseName("ix_comments_deleted_at")
            .HasFilter("deleted_at IS NOT NULL");
    }
}