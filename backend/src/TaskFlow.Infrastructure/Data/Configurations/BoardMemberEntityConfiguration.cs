using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Data.Configurations;

public class BoardMemberEntityConfiguration : IEntityTypeConfiguration<BoardMemberEntity>
{
    public void Configure(EntityTypeBuilder<BoardMemberEntity> builder)
    {
        builder.ToTable("board_members");

        // Составной первичный ключ
        builder.HasKey(bm => new { bm.BoardId, bm.UserId });

        builder.Property(bm => bm.BoardId)
            .HasColumnName("board_id");

        builder.Property(bm => bm.UserId)
            .HasColumnName("user_id");

        builder.Property(bm => bm.Role)
            .HasColumnName("role")
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("member");

        builder.Property(bm => bm.JoinedAt)
            .HasColumnName("joined_at")
            .IsRequired();

        // Внешние ключи (без навигационных свойств)
        builder.HasOne<BoardEntity>()
            .WithMany()
            .HasForeignKey(bm => bm.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(bm => bm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(bm => bm.UserId)
            .HasDatabaseName("ix_board_members_user_id");

        builder.HasIndex(bm => bm.BoardId)
            .HasDatabaseName("ix_board_members_board_id");
    }
}