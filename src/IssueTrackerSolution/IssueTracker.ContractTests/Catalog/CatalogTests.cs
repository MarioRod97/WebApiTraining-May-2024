using Alba;
using Alba.Security;
using IssueTracker.Api.Catalog;
using System.Security.Claims;

namespace IssueTracker.ContractTests.Catalog;
public class CatalogTests
{
    [Fact]
    public async Task CanAddAnItemToTheCatalog()
    {
        var stubbedToken = new AuthenticationStub()
            .With(ClaimTypes.NameIdentifier, "carl@aol.com") // "sub" claim
            .With(ClaimTypes.Role, "SoftwareCenter"); // this adds this role

        await using var host = await AlbaHost.For<Program>(stubbedToken);

        var itemToAdd = new CreateCatalogItemRequest("Notepad", "A text editor on Windows");

        var response = await host.Scenario(api =>
        {
            api.Post.Json(itemToAdd).ToUrl("/catalog");
            api.StatusCodeShouldBeOk();
        });

        var actualResponse = await response.ReadAsJsonAsync<CatalogItemResponse>();

        Assert.NotNull(actualResponse);
        Assert.Equal("Notepad", actualResponse.Title);
        Assert.Equal("A text editor on Windows", actualResponse.Description);

        // we will the 'GET' tomorrow to round this out...
    }

    [Fact]
    public async Task OnlySoftwareCenterPeopleCanDoThings()
    {
        var stubbedToken = new AuthenticationStub()
            .With(ClaimTypes.NameIdentifier, "carl@aol.com") // "sub" claim
            .With(ClaimTypes.Role, "SoftwareCenter"); // this adds this role

        await using var host = await AlbaHost.For<Program>(stubbedToken);

        var itemToAdd = new CreateCatalogItemRequest("Notepad", "A text editor on Windows");

        var response = await host.Scenario(api =>
        {
            api.Post.Json(itemToAdd).ToUrl("/catalog");
            api.StatusCodeShouldBeOk();
        });
    }
}
