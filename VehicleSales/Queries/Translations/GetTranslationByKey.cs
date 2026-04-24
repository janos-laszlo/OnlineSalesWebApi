using Microsoft.EntityFrameworkCore;

namespace VehicleSales.Queries.Translations;

public interface IGetTranslationByKey
{
    Task<TranslationDto?> Execute(string key, CancellationToken cancellationToken);
}

internal sealed class GetTranslationByKey(VehicleSalesDbContext dbContext) : IGetTranslationByKey
{
    public async Task<TranslationDto?> Execute(string key, CancellationToken cancellationToken) =>
        await dbContext.Translations
            .Where(t => t.Key == key)
            .Select(t => new TranslationDto(t.Key, t.Value))
            .FirstOrDefaultAsync(cancellationToken);
}

public sealed record TranslationDto(string Key, string Value);
