﻿using FluentValidation;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace IssueTracker.Api.Catalog;

[Authorize(Policy = "IsSoftwareAdmin")]
[Route("/catalog")]
[Produces("application/json")]
[ApiExplorerSettings(GroupName = "Software Catalog")]
public class ApiCommands(IValidator<CreateCatalogItemRequest> validator, IDocumentSession session) : ControllerBase
{
    /// <summary>
    /// Add an Item to the Software Catalog
    /// </summary>
    /// <param name="request"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <response code="201">The new software item</response>
    /// <response code="400">A application/problems+json response</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Tags = ["Software Catalog"], OperationId = "AddCatalog")]
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

        await session.SaveChangesAsync(token); // Do the actual work!

        var response = entityToSave.MapToResponse();

        return CreatedAtRoute("catalog#get-by-id", new { id = response.Id }, response); // I have stored this thing in such a way that you can get it again, it is now
                                                                                        // part of this collection.
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Tags = ["Software Catalog"])]
    public async Task<ActionResult> RemoveCatalogItemsAsync(Guid id, CancellationToken token)
    {
        var storedItem = await session.LoadAsync<CatalogItem>(id);

        if (storedItem != null)
        {
            var user = this.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            storedItem.RemovedAt = DateTimeOffset.Now;

            session.Store(storedItem); // "Upsert"

            await session.SaveChangesAsync(token); // save it
        }

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Tags = ["Software Catalog"], OperationId = "Replace")]
    public async Task<ActionResult> ReplaceCatalogItemAsync(Guid id, [FromBody] ReplaceCatalogItemRequest request, CancellationToken token)
    {
        var item = await session.LoadAsync<CatalogItem>(id);

        if (item is null)
        {
            return NotFound(0); // or do an upstart?
        }

        // I'd also validate the id in the request matches the route id, but you do you.

        if (id != request.id)
        {
            return BadRequest("Ids don't match");
        }

        item.Title = request.Title;
        item.Description = request.Description;

        session.Store(item);

        await session.SaveChangesAsync(token);

        return Ok();
    }
}
