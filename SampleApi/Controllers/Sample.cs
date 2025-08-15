using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace SampleAPi;


public record HelloRequest {
    [Required]
    [MinLength(1)]
    public required string FirstName { get; init; }

    [MinLength(1)]
    public string? LastName { get; init; }
}

public record HelloResponse {
    public required string Message { get; init; }
}

public record PatchUser {
    public required string? FirstName { get; init; }
    public required string? LastName { get; init; }
}

public record User {
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}


[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public partial class UserController() : ControllerBase {
    [HttpGet]
    [ProducesResponseType<string[]>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<User[]> ListPaginated(int skip = 0, int limit = 20) {
        if (skip < 0 || limit <= 0) {
            return BadRequest();
        }

        User[] result = [.. Enumerable.Range(1, 100)
                           .Select(x => new User { FirstName = $"FirstName {x}",
                                                   LastName = $"LastName {x}" })
                           .Skip(skip).Take(limit)];
        return result;
    }

    [HttpPost("hello-world")]
    [ProducesResponseType<HelloResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<HelloResponse> Hello(HelloRequest request) {
        return new HelloResponse { Message = $"Hello {request.FirstName} {request.LastName}" };
    }

    [Authorize]
    [HttpPatch("{id}")]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<User> Update(string id, PatchUser patch) {
        return new User { FirstName = "Tagada", LastName = "Pouet Pouet" };
    }

}
