using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using PTM.Application.Interfaces.Services;
using PTM.Domain.Models;
using PTM.Infrastructure.Authentication;
using PTM.Infrastructure.Database;

namespace PTM.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private bool _dbCreated;
    private Guid userId1;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();


            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer($"server=localhost,1433;Initial Catalog=PTM_IntegrationTest;User ID=SA;Password=dbPass@123;TrustServerCertificate=True;")
            );
            userId1 = Guid.NewGuid();
            var mockRequestContext = new Mock<IRequestContext>();
            mockRequestContext.Setup(r => r.GetIpAddress()).Returns("127.0.0.1");
            mockRequestContext.Setup(r => r.GetUserAgent()).Returns("IntegrationTest");
            mockRequestContext.Setup(r => r.GetUserId()).Returns(userId1);
            services.AddSingleton(mockRequestContext.Object);
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!_dbCreated)
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                SeedDatabase(db);
                _dbCreated = true;
            }
        });
    }

    private void SeedDatabase(AppDbContext db)
    {
        var hasher = new PasswordHasher();
        var password = hasher.HashPassword("hashed1");
        var password2 = hasher.HashPassword("hashed2");

        db.Users.AddRange(
            new User { Id = Guid.NewGuid(), Email = "user1@test.com", Password = password, PhoneNumber = "09110000001", Username="user1" },
            new User { Id = userId1, Email = "user2@test.com", Password = password2, PhoneNumber = "09110000002", Username="user2",Role = UserRole.User },
            new User { Id = Guid.NewGuid(), Email = "user3@test.com", Password = "hashed3", PhoneNumber = "09110000003", Username="user3" },
            new User { Id = Guid.NewGuid(), Email = "user4@test.com", Password = "hashed4", PhoneNumber = "09110000004", Username="user4" }
        );

        db.SaveChanges();
        db.Plans.AddRange(
            new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Free, Description="Free Plan", Price=0, MaxTasks=5, DurationDays=7 },
            new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Premium, Description="Premium Plan", Price=10, MaxTasks=20, DurationDays=30 },
            new Plan { Id = Guid.NewGuid(), Title = PlanTitle.Business, Description="Business Plan", Price=20, MaxTasks=50, DurationDays=465 }
        );
        db.SaveChanges();

        db.Tasks.AddRange(
            new TaskItem { Id = Guid.NewGuid(), Title="Task 1", Description="Description 1", UserId=db.Users.First().Id },
            new TaskItem { Id = Guid.NewGuid(), Title="Task 2", Description="Description 2", UserId=db.Users.Skip(1).First().Id },
            new TaskItem { Id = Guid.NewGuid(), Title="Task 3", Description="Description 3", UserId=db.Users.Skip(2).First().Id },
            new TaskItem { Id = Guid.NewGuid(), Title="Task 4", Description="Description 4", UserId=db.Users.Skip(3).First().Id }
        );

        db.SaveChanges();
    }

    public new void Dispose()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        base.Dispose();
    }
}
