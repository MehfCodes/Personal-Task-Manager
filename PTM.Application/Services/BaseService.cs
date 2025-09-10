using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using PTM.Application.Extentions;

namespace PTM.Application.Services;

public abstract class BaseService
{
    private readonly IServiceProvider serviceProvider;

    protected BaseService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    protected async Task ValidateAsync<T>(T model)
    {
        var validator = serviceProvider.GetService<IValidator<T>>();
        if (validator is null) return;
        await validator.ValidateAndThrowAsync(model);
    }
}

