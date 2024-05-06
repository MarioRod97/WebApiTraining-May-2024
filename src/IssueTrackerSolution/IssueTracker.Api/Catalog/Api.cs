using FluentValidation;
using Marten;
using Microsoft.AspNetCore.Mvc;
using static IssueTracker.Api.Catalog.Entities;

namespace IssueTracker.Api.Catalog;

public class Api(IValidator<CreateCatalogItemRequest> validator, IDocumentSession session) : ControllerBase
{
    [HttpGet("/catalog")]
    public async Task<ActionResult> GetAllCatalogItemsAsync(CancellationToken token)
    {
        var data = await session.Query<CatalogItem>()
            .Select(c => new CatalogItemResponse(c.Id, c.Title, c.Description))
            .ToListAsync(token);

        return Ok(new { data });
    }

    [HttpPost("/catalog")]
    public async Task<ActionResult> AddACatalogItemAsync([FromBody] CreateCatalogItemRequest request,
        CancellationToken token)
    {
        var validation = await validator.ValidateAsync(request, token);
        if (!validation.IsValid)
        {
            return this.CreateProblemDetailsForModelValidation("Cannot Add Catalog Item", validation.ToDictionary());
        }

        var entityToSave = new CatalogItem(Guid.NewGuid(), request.Title, request.Description, "todo", DateTimeOffset.Now);

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
}
