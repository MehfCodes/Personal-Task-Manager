using System;
using Microsoft.EntityFrameworkCore;
using PTM.Domain.Models;

namespace PTM.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<UserPlan> UserPlans { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<string>();
        modelBuilder.Entity<Plan>().Property(p => p.Title).HasConversion<string>();
        modelBuilder.Entity<Plan>().Property(p => p.Price).HasPrecision(18, 2);
        modelBuilder.Entity<TaskItem>().Property(ti => ti.Priority).HasConversion<string>();
        modelBuilder.Entity<TaskItem>().Property(ti => ti.Status).HasConversion<string>();

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasMany(u => u.UserPlans)
                .WithOne(up => up.User)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Plan>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasMany(p => p.UserPlans)
                .WithOne(up => up.Plan)
                .HasForeignKey(up => up.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserPlan>(e =>
        {
            e.HasKey(up => up.Id);
            e.HasIndex(up => new { up.UserId, up.PlanId }).IsUnique(false);
            e.Property(up => up.PurchasedAt).IsRequired();
            e.Property(up => up.IsActive).HasDefaultValue(true);
        });
    }
}
