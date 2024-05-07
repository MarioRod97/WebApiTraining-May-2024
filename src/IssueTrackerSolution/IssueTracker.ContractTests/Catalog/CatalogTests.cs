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
            api.StatusCodeShouldBe(201);
            api.Header("Location").SingleValueShouldMatch(Constants.GetLocationForPathRegex("catalog"));
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
            .With(ClaimTypes.Role, "TacoNose"); // this adds this role

        await using var host = await AlbaHost.For<Program>(stubbedToken);

        var itemToAdd = new CreateCatalogItemRequest("Notepad", "A text editor on Windows");

        var response = await host.Scenario(api =>
        {
            api.Post.Json(itemToAdd).ToUrl("/catalog");
            api.StatusCodeShouldBe(403); // unauthorized
        });
    }

    [Fact]
    public async Task NewItemsCanBeAddedAndDeleted()
    {
        // have a softwareadmin add an item.
        // then get that item.
        // then delete the item.

        var stubbedToken = new AuthenticationStub()
            .With(ClaimTypes.NameIdentifier, "carl@aol.com") // "sub" claim
            .With(ClaimTypes.Role, "SoftwareCenter"); // this adds this role

        await using var host = await AlbaHost.For<Program>(stubbedToken);

        var itemToAdd = new CreateCatalogItemRequest("Notepad", "A text editor on Windows");

        var response = await host.Scenario(api =>
        {
            api.Post.Json(itemToAdd).ToUrl("/catalog");
            api.StatusCodeShouldBe(201);
        });

        var actualResponse = await response.ReadAsJsonAsync<CatalogItemResponse>();

        Assert.NotNull(actualResponse);

        var id = actualResponse.Id;

        var responseTwo = await host.Scenario(api =>
        {
            api.Delete.Url("/catalog/" + id);
            api.StatusCodeShouldBe(204);
        });

        await host.Scenario(api =>
        {
            api.Get.Url("/catalog/" + id);
            api.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task UsersCanOnlyDeleteItemsTheyCreated()
    {
        // have a softwareadmin add an item.
        // then get that item.
        // then delete the item.

        var stubbedToken = new AuthenticationStub()
            .With(ClaimTypes.NameIdentifier, "carl@aol.com") // "sub" claim
            .With(ClaimTypes.Role, "SoftwareCenter"); // this adds this role

        await using var host = await AlbaHost.For<Program>(stubbedToken);

        var itemToAdd = new CreateCatalogItemRequest("Notepad", "A text editor on Windows");

        var response = await host.Scenario(api =>
        {
            api.Post.Json(itemToAdd).ToUrl("/catalog");
            api.StatusCodeShouldBe(201);
        });

        var actualResponse = await response.ReadAsJsonAsync<CatalogItemResponse>();

        Assert.NotNull(actualResponse);

        var id = actualResponse.Id;

        var userTwoToken = new AuthenticationStub()
            .With(ClaimTypes.NameIdentifier, "beth@aol.com") // "sub" claim
            .With(ClaimTypes.Role, "SoftwareCenter"); // this adds this role

        await using var hostTwo = await AlbaHost.For<Program>(userTwoToken);

        var responseTwo = await hostTwo.Scenario(api =>
        {
            api.Delete.Url("/catalog/" + id);
            api.StatusCodeShouldBe(403);
        });

        await host.Scenario(api =>
        {
            api.Get.Url("/catalog/" + id);
            api.StatusCodeShouldBe(200);
        });
    }
}
