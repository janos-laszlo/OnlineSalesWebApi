using Microsoft.EntityFrameworkCore;

namespace DraftEntities.Interface;

public interface IDraftEntityOperations
{
    Task CreateDraftEntityAsync(DraftEntity entity, CancellationToken cancellationToken);
    Task<DraftEntity?> GetDraftEntityByIdAsync(int id, CancellationToken cancellationToken);
}

internal sealed class DraftEntityOperations(
    DraftEntitiesDbContext dbContext) : IDraftEntityOperations
{
    public async Task CreateDraftEntityAsync(DraftEntity entity, CancellationToken cancellationToken)
    {
        dbContext.DraftEntities.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<DraftEntity?> GetDraftEntityByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.DraftEntities
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
}
