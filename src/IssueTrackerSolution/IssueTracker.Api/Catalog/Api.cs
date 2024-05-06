using FluentValidation;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static IssueTracker.Api.Catalog.Entities;

namespace IssueTracker.Api.Catalog;

[Authorize]
[Route("/catalog")]

public class Api(IValidator<CreateCatalogItemRequest> validator, IDocumentSession session) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAllCatalogItemsAsync(CancellationToken token)
    {
        var data = await session.Query<CatalogItem>()
            .Select(c => new CatalogItemResponse(c.Id, c.Title, c.Description))
            .ToListAsync(token);

        return Ok(new { data });
    }

    [HttpPost]
    [Authorize(Roles = "SoftwareCenter")]
    public async Task<ActionResult> AddACatalogItemAsync([FromBody] CreateCatalogItemRequest request,
        CancellationToken token)
    {
        var user = this.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        var userId = user?.Value;

        var validation = await validator.ValidateAsync(request, token);
        if (!validation.IsValid)
        {
            return this.CreateProblemDetailsForModelValidation("Cannot Add Catalog Item", validation.ToDictionary());
        }

        var entityToSave = new CatalogItem(Guid.NewGuid(), request.Title, request.Description, userId!, DateTimeOffset.Now);

        session.Store(entityToSave);

        await session.SaveChangesAsync(); // Do the actual work!

        // get the JSON data they sent and look at it. Is it cool?
        // If not, send them an error (400, with some details)
        // if it is cool, maybe save it to a database or something?
        // we have to create the entity to save the request, and add it to the database, etc.
        // save it.
        // and what are we going to return.
        // return to them, from the entity, the thing we are giving them as the "reciept"

        var response = new CatalogItemResponse(entityToSave.Id, request.Title, request.Description);

        return Ok(response); // I have stored this thing in such a way that you can get it again, it is now
                             // part of this collection.
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetCatalogItemByIdAsync(Guid id, CancellationToken token)
    {
        var response = await session.Query<CatalogItem>()
            .Where(c => c.Id == id)
            .Select(c => new CatalogItemResponse(c.Id, c.Title, c.Description))
            .SingleOrDefaultAsync(token);

        if (response is null)
        {
            return NotFound();
        }
        else
        {
            return Ok(response);
        }
    }
}
