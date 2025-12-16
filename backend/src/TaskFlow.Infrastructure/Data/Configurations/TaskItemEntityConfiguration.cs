using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Data.Configurations;

public class TaskEntityConfiguration : IEntityTypeConfiguration<TaskItemEntity>
{
    public void Configure(EntityTypeBuilder<TaskItemEntity> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasColumnName("description");

        builder.Property(t => t.Order)
            .HasColumnName("order")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(t => t.DueDate)
            .HasColumnName("due_date");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(t => t.ColumnId)
            .HasColumnName("column_id")
            .IsRequired();

        builder.Property(t => t.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired();

        builder.Property(t => t.AssignedUserId)
            .HasColumnName("assigned_user_id");

        // Внешние ключи
        builder.HasOne<ColumnEntity>()
            .WithMany()
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(t => t.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Индексы
        builder.HasIndex(t => t.ColumnId)
            .HasDatabaseName("ix_tasks_column_id");

        builder.HasIndex(t => new { t.ColumnId, t.Order })
            .IsUnique()
            .HasDatabaseName("ix_tasks_column_id_order");

        builder.HasIndex(t => t.AssignedUserId)
            .HasDatabaseName("ix_tasks_assigned_user_id");

        builder.HasIndex(t => t.CreatedById)
            .HasDatabaseName("ix_tasks_created_by_id");

        builder.HasIndex(t => t.DueDate)
            .HasDatabaseName("ix_tasks_due_date")
            .HasFilter("due_date IS NOT NULL");

        builder.HasIndex(t => t.CompletedAt)
            .HasDatabaseName("ix_tasks_completed_at")
            .HasFilter("completed_at IS NOT NULL");

        builder.HasIndex(t => t.Priority)
            .HasDatabaseName("ix_tasks_priority");
    }
}