using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Data.Configurations;

public class ColumnEntityConfiguration : IEntityTypeConfiguration<ColumnEntity>
{
    public void Configure(EntityTypeBuilder<ColumnEntity> builder)
    {
        builder.ToTable("columns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Order)
            .HasColumnName("order")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.WipLimit)
            .HasColumnName("wip_limit");

        builder.Property(c => c.BoardId)
            .HasColumnName("board_id")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Внешний ключ
        builder.HasOne<BoardEntity>()
            .WithMany()
            .HasForeignKey(c => c.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Уникальный порядок в пределах доски
        builder.HasIndex(c => new { c.BoardId, c.Order })
            .IsUnique()
            .HasDatabaseName("ix_columns_board_id_order");

        builder.HasIndex(c => c.BoardId)
            .HasDatabaseName("ix_columns_board_id");
    }
}