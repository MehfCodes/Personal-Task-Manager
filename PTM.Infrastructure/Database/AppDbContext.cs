using System;
using Microsoft.EntityFrameworkCore;
using PTM.Domain.Models;

namespace PTM.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Plan>().Property(p => p.Title).HasConversion<string>();
        modelBuilder.Entity<Plan>().Property(p => p.Price).HasPrecision(18,2);
        modelBuilder.Entity<TaskItem>().Property(p => p.Priority).HasConversion<string>();
        modelBuilder.Entity<TaskItem>().Property(p => p.Status).HasConversion<string>();
    }
}
