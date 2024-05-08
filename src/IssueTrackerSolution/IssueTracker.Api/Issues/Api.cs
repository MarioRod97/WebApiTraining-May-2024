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
    public async Task<ActionResult<UserIssueResponse>> AddAnIssueAsync(Guid Id, [FromBody] UserCreateIssueRequestModel request, CancellationToken token)
    {
        var software = await session.Query<CatalogItem>()
            .Where(c => c.Id == Id)
            .Select(c => new IssueSoftwareEmbeddedResponse(c.Id, c.Title, c.Description))
            .SingleOrDefaultAsync();

        if (software is null)
        {
            return NotFound("No software found with that Id in the catalog.");
        }

        var userInfo = await userIdentityService.GetUserInformationAsync();

        var userUrl = Url.RouteUrl("users#get-by-id", new { id = userInfo.Id }) ?? throw new Exception("Need a User URL");

        var entity = new UserIssue
        {
            Id = Guid.NewGuid(),
            Status = IssueStatusTypes.Submitted,
            User = userUrl,
            Software = software,
            Created = DateTimeOffset.Now
        };

        session.Store<UserIssue>(entity);

        await session.SaveChangesAsync(token);

        var response = new UserIssueResponse
        {
            Id = entity.Id,
            Status = entity.Status,
            User = entity.User,
            Software = entity.Software
        };

        return Ok(response);
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

public record UserIssue
{
    public Guid Id { get; set; }
    public string User { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; }
    public IssueSoftwareEmbeddedResponse? Software { get; set; }
    public IssueStatusTypes Status { get; set; } = IssueStatusTypes.Submitted;
}

public record IssueSoftwareEmbeddedResponse(Guid Id, string Title, string Description);

public enum IssueStatusTypes { Submitted };
