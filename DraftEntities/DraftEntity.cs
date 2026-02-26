namespace DraftEntities;

public sealed record DraftEntity(
    int Id,
    string Name,
    string JsonValue,
    DateTime CreatedAt)
{
    internal const int NameMaxLength = 30;
}