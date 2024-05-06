namespace IssueTracker.Api.Catalog;

public class Entities
{
    public record CatalogItem(Guid Id, string Title, string Description, string AddedBy, DateTimeOffset CreatedAt);
}
