using System;
using FluentValidation;

namespace PTM.Application.Extentions;

public static class ValidatorExtensions
{
    public static async Task ValidateAndThrowAsync<T>(this IValidator<T> validator, T instance)
    {
        var result = await validator.ValidateAsync(instance);
        if (!result.IsValid) throw new ValidationException(result.Errors);
    }
}
