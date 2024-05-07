using FluentValidation.TestHelper;
using IssueTracker.Api.Catalog;

namespace IssueTracker.UnitTests;
public class AddCatalogItemValidationTests
{
    [Fact]
    public async void CanValidate()
    {
        var validator = new CreateCatalogItemRequestValidator();

        var model = new CreateCatalogItemRequest("", new string('x', 1026));

        var results = await validator.TestValidateAsync(model);

        results.ShouldHaveValidationErrorFor(m => m.Title).WithErrorMessage("We need a title");
        results.ShouldHaveValidationErrorFor(m => m.Description);
    }
}
