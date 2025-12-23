using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Data.Configurations;

public class ActivityLogEntityConfiguration : IEntityTypeConfiguration<ActivityLogEntity>
{
    public void Configure(EntityTypeBuilder<ActivityLogEntity> builder)
    {
        builder.ToTable("activity_logs");

        builder.HasKey(al => al.Id);

        builder.Property(al => al.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(al => al.BoardId)
            .HasColumnName("board_id")
            .IsRequired();

        builder.Property(al => al.UserId)
            .HasColumnName("user_id");

        builder.Property(al => al.ActivityType)
            .HasColumnName("activity_type")
            .IsRequired();

        builder.Property(al => al.Description)
            .HasColumnName("description")
            .IsRequired();

     

        builder.Property(al => al.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        builder.Property(al => al.RelatedTaskId)
           .HasColumnName("related_task_id")
           .IsRequired();
        builder.Property(al => al.RelatedColumnId)
            .HasColumnName("related_column_id")
            .IsRequired();
        builder.Property(al => al.RelatedUserId)
                .HasColumnName("related_user_id")
                .IsRequired();
        builder.Property(al => al.OldValue)
            .HasColumnName("old_value")
            .IsRequired();
        builder.Property(al => al.NewValue)
           .HasColumnName("new_value")
           .IsRequired();
  builder.Property(al => al.OldValue)
            .HasColumnName("old_value ")
            .IsRequired();

    // Внешние ключи
    builder.HasOne<BoardEntity>()
            .WithMany()
            .HasForeignKey(al => al.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Индексы
        builder.HasIndex(al => al.BoardId)
            .HasDatabaseName("ix_activity_logs_board_id");

        builder.HasIndex(al => al.UserId)
            .HasDatabaseName("ix_activity_logs_user_id");

        builder.HasIndex(al => al.ActivityType)
            .HasDatabaseName("ix_activity_logs_activity_type");

        builder.HasIndex(al => al.CreatedAt)
            .HasDatabaseName("ix_activity_logs_created_at");
    }
}