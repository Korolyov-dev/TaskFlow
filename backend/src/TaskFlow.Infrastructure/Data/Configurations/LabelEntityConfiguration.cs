using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Data.Configurations;

public class LabelEntityConfiguration : IEntityTypeConfiguration<LabelEntity>
{
    public void Configure(EntityTypeBuilder<LabelEntity> builder)
    {
        builder.ToTable("labels");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Color)
            .HasColumnName("color")
            .IsRequired()
            .HasMaxLength(7)
            .HasDefaultValue("#6b7280");

        builder.Property(l => l.BoardId)
            .HasColumnName("board_id")
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Внешний ключ
        builder.HasOne<BoardEntity>()
            .WithMany()
            .HasForeignKey(l => l.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Уникальное имя в пределах доски
        builder.HasIndex(l => new { l.BoardId, l.Name })
            .IsUnique()
            .HasDatabaseName("ix_labels_board_id_name");

        builder.HasIndex(l => l.BoardId)
            .HasDatabaseName("ix_labels_board_id");
    }
}