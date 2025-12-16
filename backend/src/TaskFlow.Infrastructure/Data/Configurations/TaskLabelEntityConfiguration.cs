using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Data.Configurations;

public class TaskLabelEntityConfiguration : IEntityTypeConfiguration<TaskLabelEntity>
{
    public void Configure(EntityTypeBuilder<TaskLabelEntity> builder)
    {
        builder.ToTable("task_labels");

        // Составной первичный ключ
        builder.HasKey(tl => new { tl.TaskId, tl.LabelId });

        builder.Property(tl => tl.TaskId)
            .HasColumnName("task_id");

        builder.Property(tl => tl.LabelId)
            .HasColumnName("label_id");

        builder.Property(tl => tl.AssignedAt)
            .HasColumnName("assigned_at")
            .IsRequired();

        // Внешние ключи
        builder.HasOne<TaskItemEntity>()
            .WithMany()
            .HasForeignKey(tl => tl.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<LabelEntity>()
            .WithMany()
            .HasForeignKey(tl => tl.LabelId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(tl => tl.TaskId)
            .HasDatabaseName("ix_task_labels_task_id");

        builder.HasIndex(tl => tl.LabelId)
            .HasDatabaseName("ix_task_labels_label_id");
    }
}