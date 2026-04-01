using Microsoft.AspNetCore.Mvc;

namespace SampleApi.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class DiagnosticsController : ControllerBase {
    [HttpPost("echo")]
    [Consumes("text/plain")]
    [Produces("text/plain")]
    [TextPlainRequestBody]
    [JsonProblemResponses]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<string> Echo([FromBody] string body) {
        if (string.IsNullOrWhiteSpace(body)) {
            return BadRequest(new ProblemDetails { Title = "Body is required." });
        }

        return body.Trim();
    }

    [HttpDelete("cache/{key}")]
    [DefaultProblemResponse]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult PurgeCache([FromRoute] string key) {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return NoContent();
    }
}
