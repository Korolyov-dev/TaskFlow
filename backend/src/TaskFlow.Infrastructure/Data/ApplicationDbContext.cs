using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Common;
using TaskFlow.Infrastructure.Entities;

namespace TaskFlow.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<BoardEntity> Boards { get; set; } = null!;
    public DbSet<BoardMemberEntity> BoardMembers { get; set; } = null!;
    public DbSet<ColumnEntity> Columns { get; set; } = null!;
    public DbSet<TaskItemEntity> Tasks { get; set; } = null!;
    public DbSet<LabelEntity> Labels { get; set; } = null!;
    public DbSet<TaskLabelEntity> TaskLabels { get; set; } = null!;
    public DbSet<CommentEntity> Comments { get; set; } = null!;
    public DbSet<AttachmentEntity> Attachments { get; set; } = null!;
    public DbSet<ActivityLogEntity> ActivityLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Применяем все конфигурации из сборки
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Отключаем каскадное удаление по умолчанию
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        SetSpecialTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        SetSpecialTimestamps();
        return base.SaveChanges();
    }

    private void SetTimestamps()
    {
        var entries = ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            // Для Entity с CreatedAt и UpdatedAt
            if (entry.Entity is ITimestampedEntity timestampedEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    timestampedEntity.CreatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    timestampedEntity.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Для Entity только с CreatedAt
            else if (entry.Entity is ICreationTimestampedEntity creationEntity &&
                     entry.State == EntityState.Added)
            {
                creationEntity.CreatedAt = DateTime.UtcNow;
            }
        }
    }

    private void SetSpecialTimestamps()
    {
        var entries = ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            // Для BoardMemberEntity - дата присоединения
            if (entry.Entity is BoardMemberEntity boardMember && entry.State == EntityState.Added)
            {
                boardMember.JoinedAt = DateTime.UtcNow;
            }

            // Для TaskLabelEntity - дата назначения метки
            if (entry.Entity is TaskLabelEntity taskLabel && entry.State == EntityState.Added)
            {
                taskLabel.AssignedAt = DateTime.UtcNow;
            }

            // Для AttachmentEntity - дата загрузки (если не через интерфейс)
            if (entry.Entity is AttachmentEntity attachment && entry.State == EntityState.Added)
            {
                attachment.UploadedAt = DateTime.UtcNow;
            }

            // Для ActivityLogEntity - дата события
            if (entry.Entity is ActivityLogEntity activityLog && entry.State == EntityState.Added)
            {
                activityLog.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
