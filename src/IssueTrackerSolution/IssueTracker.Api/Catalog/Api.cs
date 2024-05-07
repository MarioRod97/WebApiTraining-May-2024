using FluentValidation;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IssueTracker.Api.Catalog;

[Authorize]
[Route("/catalog")]

public class Api(IValidator<CreateCatalogItemRequest> validator, IDocumentSession session) : ControllerBase
{
    [HttpGet]
    [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Client)]
    public async Task<ActionResult> GetAllCatalogItemsAsync(CancellationToken token)
    {
        var data = await session.Query<CatalogItem>()
            .Where(c => c.RemovedAt == null)
            .Select(c => new CatalogItemResponse(c.Id, c.Title, c.Description))
            .ToListAsync(token);

        return Ok(new { data });
    }

    [HttpPost]
    [Authorize(Policy = "IsSoftwareAdmin")]
    public async Task<ActionResult> AddACatalogItemAsync([FromBody] CreateCatalogItemRequest request,
        CancellationToken token)
    {
        var user = this.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
        var userId = user.Value;

        var validation = await validator.ValidateAsync(request, token);
        if (!validation.IsValid)
        {
            return this.CreateProblemDetailsForModelValidation("Cannot Add Catalog Item", validation.ToDictionary());
        }

        var entityToSave = request.MapToCatalogItem(userId);

        session.Store(entityToSave);

        await session.SaveChangesAsync(); // Do the actual work!

        // get the JSON data they sent and look at it. Is it cool?
        // If not, send them an error (400, with some details)
        // if it is cool, maybe save it to a database or something?
        // we have to create the entity to save the request, and add it to the database, etc.
        // save it.
        // and what are we going to return.
        // return to them, from the entity, the thing we are giving them as the "reciept"

        var response = entityToSave.MapToResponse();

        return CreatedAtRoute("catalog#get-by-id", new { id = response.Id }, response); // I have stored this thing in such a way that you can get it again, it is now
                                                                                        // part of this collection.
    }

    [HttpGet("{id:guid}", Name = "catalog#get-by-id")]
    public async Task<ActionResult> GetCatalogItemByIdAsync(Guid id, CancellationToken token)
    {
        var response = await session.Query<CatalogItem>()
            .Where(c => c.Id == id && c.RemovedAt == null)
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

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "IsSoftwareAdmin")]
    public async Task<ActionResult> RemoveCatalogItemsAsync(Guid id)
    {
        var storedItem = await session.LoadAsync<CatalogItem>(id);

        if (storedItem != null)
        {
            var user = this.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            //if (storedItem.AddedBy != user.Value)
            //{
            //    return StatusCode(403);
            //}

            // if it does, do a "soft delete"
            storedItem.RemovedAt = DateTimeOffset.Now;

            session.Store(storedItem); // "Upsert"

            await session.SaveChangesAsync(); // save it
        }

        return NoContent();
    }
}
