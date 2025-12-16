using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Data.Configurations;

public class BoardEntityConfiguration : IEntityTypeConfiguration<BoardEntity>
{
    public void Configure(EntityTypeBuilder<BoardEntity> builder)
    {
        builder.ToTable("boards");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(b => b.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(b => b.Color)
            .HasColumnName("color")
            .IsRequired()
            .HasMaxLength(7)
            .HasDefaultValue("#4f46e5");

        builder.Property(b => b.IsFavorite)
            .HasColumnName("is_favorite")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(b => b.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        // Внешний ключ на UserEntity (без навигационного свойства)
        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(b => b.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Индексы
        builder.HasIndex(b => b.OwnerId)
            .HasDatabaseName("ix_boards_owner_id");

        builder.HasIndex(b => b.CreatedAt)
            .HasDatabaseName("ix_boards_created_at");

        builder.HasIndex(b => b.IsFavorite)
            .HasDatabaseName("ix_boards_is_favorite")
            .HasFilter("is_favorite = true");
    }
}