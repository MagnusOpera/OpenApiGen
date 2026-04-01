using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleApi.Models;

namespace SampleApi.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class UserController : ControllerBase {
    [HttpGet]
    [ProducesResponseType<UserSummary[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<UserSummary[]> ListPaginated([FromQuery] int skip = 0, [FromQuery] int limit = 20) {
        if (skip < 0 || limit <= 0) {
            return BadRequest(new ProblemDetails { Title = "Paging arguments are invalid." });
        }

        var result = Enumerable.Range(1, 6)
            .Skip(skip)
            .Take(limit)
            .Select(id => new UserSummary {
                Id = id.ToString(),
                FirstName = $"FirstName {id}",
                LastName = $"LastName {id}",
                Nickname = id % 2 == 0 ? $"nick-{id}" : null,
                State = (id % 3) switch {
                    0 => ProjectState.Draft,
                    1 => ProjectState.Active,
                    _ => ProjectState.Archived
                },
                CreatedAt = DateTimeOffset.Parse("2025-01-01T12:00:00+00:00").AddDays(id),
                NextReviewOn = DateOnly.Parse("2025-02-01").AddDays(id),
                Labels = new Dictionary<string, string> {
                    ["source"] = "sample",
                    ["segment"] = id % 2 == 0 ? "vip" : "standard"
                }
            })
            .ToArray();

        return result;
    }

    [HttpPost("hello-world")]
    [ProducesResponseType<HelloResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<HelloResponse> Hello(HelloRequest request) {
        if (string.IsNullOrWhiteSpace(request.FirstName)) {
            return BadRequest(new ProblemDetails { Title = "First name is required." });
        }

        return new HelloResponse {
            Message = $"Hello {request.FirstName} {request.LastName}".Trim()
        };
    }

    [Authorize]
    [HttpPatch("{id}")]
    [ProducesResponseType<UserSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<UserSummary> Update([FromRoute] string id, [FromBody] PatchUser patch) {
        if (string.IsNullOrWhiteSpace(id)) {
            return BadRequest(new ProblemDetails { Title = "Id is required." });
        }

        if (id == "missing") {
            return NotFound(new ProblemDetails { Title = "User not found." });
        }

        return new UserSummary {
            Id = id,
            FirstName = patch.FirstName ?? "Tagada",
            LastName = patch.LastName ?? "Pouet",
            Nickname = patch.Nickname,
            State = ProjectState.Active,
            CreatedAt = DateTimeOffset.Parse("2025-03-01T09:30:00+00:00"),
            NextReviewOn = DateOnly.Parse("2025-03-15"),
            Labels = patch.Labels
        };
    }
}
