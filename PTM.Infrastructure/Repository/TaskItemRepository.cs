using System;
using Microsoft.EntityFrameworkCore;
using PTM.Application.Interfaces.Repositories;
using PTM.Domain.Models;
using PTM.Infrastructure.Database;

namespace PTM.Infrastructure.Repository;

public class TaskItemRepository : BaseRepository<TaskItem>, ITaskItemRepository
{
    private readonly AppDbContext context;

    public TaskItemRepository(AppDbContext context) : base(context)
    {
        this.context = context;
    }

    public async Task<int> GetTaskCount(Guid userId)
    {
        return await context.Tasks.CountAsync(t => t.UserId == userId);
    }
}
