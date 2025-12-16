using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Data.Configurations;

public class AttachmentEntityConfiguration : IEntityTypeConfiguration<AttachmentEntity>
{
    public void Configure(EntityTypeBuilder<AttachmentEntity> builder)
    {
        builder.ToTable("attachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.FileName)
            .HasColumnName("file_name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.FileUrl)
            .HasColumnName("file_url")
            .IsRequired();

        builder.Property(a => a.FileSize)
            .HasColumnName("file_size")
            .IsRequired();

        builder.Property(a => a.MimeType)
            .HasColumnName("mime_type")
            .HasMaxLength(100);

        builder.Property(a => a.UploadedAt)
            .HasColumnName("uploaded_at")
            .IsRequired();

        builder.Property(a => a.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(a => a.UploadedBy)
            .HasColumnName("uploaded_by")
            .IsRequired();

        // Внешние ключи
        builder.HasOne<TaskItemEntity>()
            .WithMany()
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(a => a.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Индексы
        builder.HasIndex(a => a.TaskId)
            .HasDatabaseName("ix_attachments_task_id");

        builder.HasIndex(a => a.UploadedBy)
            .HasDatabaseName("ix_attachments_uploaded_by");

        builder.HasIndex(a => a.UploadedAt)
            .HasDatabaseName("ix_attachments_uploaded_at");
    }
}