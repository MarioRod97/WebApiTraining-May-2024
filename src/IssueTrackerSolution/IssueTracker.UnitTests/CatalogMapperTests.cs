using IssueTracker.Api.Catalog;

namespace IssueTracker.UnitTests;
public class CatalogMapperTests
{
    [Fact]
    public void CanMapCatalogItems()
    {
        var entity = new CatalogItem { Id = Guid.NewGuid(), Title = "Stuff", AddedBy = "Joe", CreatedAt = DateTimeOffset.Now, Description = "rad" };

        var mappedResponse = entity.MapToResponse();

        Assert.Equal("stuff", mappedResponse.Title);
        Assert.Equal("rad", mappedResponse.Description);
        Assert.Equal(entity.Id, mappedResponse.Id);
    }
}
