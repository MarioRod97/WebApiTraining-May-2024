using IssueTracker.Api.Catalog;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IssueTracker.Api.Issues;
[ApiExplorerSettings(GroupName = "Issues")]

public class Api(UserIdentityService userIdentityService, IDocumentSession session) : ControllerBase
{
    [HttpPost("/catalog/{id:guid}/issues")]
    [SwaggerOperation(Tags = ["Issues", "Software Catalog"])]
    public async Task<ActionResult<UserIssueResponse>> AddAnIssueAsync(Guid Id, [FromBody] UserCreateIssueRequestModel request)
    {
        var softwareExists = await session.Query<CatalogItem>().Where(c => c.Id == Id).AnyAsync();

        if (!softwareExists)
        {
            return NotFound("No software found with that Id in the catalog.");
        }

        var userInfo = await userIdentityService.GetUserInformationAsync();

        var fakeResponse = new UserIssueResponse
        {
            Id = Guid.NewGuid(),
            Status = IssueStatusTypes.Submitted,
            User = "/users/" + userInfo.Id,
            Software = new IssueSoftwareEmbeddedResponse(Id, "Fake Title", "Fake Description")
        };

        return Ok(fakeResponse);
    }
}

public record UserCreateIssueRequestModel(string Description);

public record UserIssueResponse
{
    public Guid Id { get; set; }
    public string User { get; set; } = string.Empty;
    public IssueSoftwareEmbeddedResponse? Software { get; set; }
    public IssueStatusTypes Status { get; set; } = IssueStatusTypes.Submitted;
}

public record IssueSoftwareEmbeddedResponse(Guid Id, string Title, string Description);

public enum IssueStatusTypes { Submitted };
