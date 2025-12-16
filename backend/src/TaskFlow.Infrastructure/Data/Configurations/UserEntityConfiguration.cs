using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Data.Configurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // Генерируем в коде

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.UserName)
            .HasColumnName("username")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(255);

        builder.Property(u => u.AvatarUrl)
            .HasColumnName("avatar_url");

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        // Индексы
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        builder.HasIndex(u => u.UserName)
            .IsUnique()
            .HasDatabaseName("ix_users_username");

        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("ix_users_created_at");
    }
}