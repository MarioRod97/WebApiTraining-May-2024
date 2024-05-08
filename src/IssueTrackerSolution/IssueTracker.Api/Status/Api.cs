using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IssueTracker.Api.Status;
/// <summary>
/// This is the Api for the Status stuff
/// </summary>
/// <param name="logger"></param>
public class Api(ILogger<Api> logger) : ControllerBase
{
    /// <summary>
    /// Use this to get the status of the API
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <response code="201"></response>
    [HttpGet("/status")]
    public async Task<ActionResult<StatusResponseModel>> GetTheStatus(CancellationToken token)
    {
        // Some real work here, that we have to await (it's going to be a database call, an API call, whatever.)
        logger.LogInformation("Starting the Async Call");

        await Task.Delay(3000, token); // the API call, the database lookup, etc.

        var response = new StatusResponseModel
        {
            Message = "Looks Good",
            CheckedAt = DateTimeOffset.UtcNow
        };

        logger.LogInformation("Finished the Call");

        return Ok(response);
    }

    [HttpPost("/status")]
    public async Task<ActionResult> AddNewStatusMessage()
    {
        return Ok();
    }
}

public record StatusRequestModel
{
    [Required, MinLength(5), MaxLength(30)]
    public string Message { get; set; } = string.Empty;
}

public record StatusResponseModel
{
    [Required, MinLength(5), MaxLength(30)]
    public string Message { get; set; } = string.Empty;
    [Required]
    public DateTimeOffset CheckedAt { get; init; }
    public string? CheckedBy { get; set; }
};
