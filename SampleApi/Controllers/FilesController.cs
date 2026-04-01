using Microsoft.AspNetCore.Mvc;
using SampleApi.Models;

namespace SampleApi.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class FilesController : ControllerBase {
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [DefaultProblemResponse]
    [ProducesResponseType<UploadAssetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<UploadAssetResponse> Upload([FromForm] UploadAssetRequest request) {
        if (request.File.Length == 0) {
            return BadRequest(new ProblemDetails { Title = "File is empty." });
        }

        return new UploadAssetResponse {
            Id = "asset-1",
            FileName = request.File.FileName,
            Metadata = new Dictionary<string, string> {
                ["contentType"] = request.File.ContentType,
                ["description"] = request.Description ?? string.Empty
            }
        };
    }

    [HttpGet("{id}/content")]
    [Produces("application/octet-stream")]
    [BinaryResponse]
    [JsonProblemResponses]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public IActionResult Download([FromRoute] string id) {
        if (id == "missing") {
            return NotFound(new ProblemDetails { Title = "File not found." });
        }

        return File([0x01, 0x02, 0x03, 0x04], "application/octet-stream", $"{id}.bin");
    }
}
