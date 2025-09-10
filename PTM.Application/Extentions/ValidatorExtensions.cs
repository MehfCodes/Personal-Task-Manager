using FluentValidation;
using PTM.Application.Exceptions;

namespace PTM.Application.Extentions;

public static class ValidatorExtensions
{
    public static async Task ValidateAndThrowAsync<T>(this IValidator<T> validator, T instance)
    {
        var result = await validator.ValidateAsync(instance);
        if (!result.IsValid) throw new Exceptions.ValidationException(result.Errors);
    }
}
