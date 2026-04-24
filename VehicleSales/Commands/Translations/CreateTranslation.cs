using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using VehicleSales.Entities.Translation;

namespace VehicleSales.Commands.Translations;

public interface ICreateTranslation
{
    Task<Result<int>> Execute(CreateTranslationDto dto, CancellationToken cancellationToken);
}

internal sealed class CreateTranslation(VehicleSalesDbContext dbContext) : ICreateTranslation
{
    public async Task<Result<int>> Execute(CreateTranslationDto dto, CancellationToken cancellationToken)
    {
        if (await dbContext.Translations.AnyAsync(t => t.Key == dto.Key, cancellationToken))
            return Result.Failure<int>($"A translation with key '{dto.Key}' already exists");

        var translation = new Translation { Key = dto.Key, Value = dto.Value };
        dbContext.Translations.Add(translation);
        await dbContext.SaveChangesAsync(cancellationToken);

        return translation.Id;
    }
}

public sealed record CreateTranslationDto(string Key, string Value);
