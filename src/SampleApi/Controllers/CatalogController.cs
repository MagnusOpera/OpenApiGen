using Microsoft.AspNetCore.Mvc;
using SampleApi.Models;

namespace SampleApi.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class CatalogController : ControllerBase {
    [HttpGet("dashboard")]
    [DefaultProblemResponse]
    [ProducesResponseType<DashboardResponse>(StatusCodes.Status200OK)]
    public ActionResult<DashboardResponse> GetDashboard() {
        return new DashboardResponse {
            Owner = new UserSummary {
                Id = "owner-1",
                FirstName = "Ada",
                LastName = "Lovelace",
                Nickname = "architect",
                State = ProjectState.Active,
                CreatedAt = DateTimeOffset.Parse("2024-10-01T08:15:00+00:00"),
                NextReviewOn = DateOnly.Parse("2025-04-10"),
                Labels = new Dictionary<string, string> {
                    ["team"] = "platform"
                }
            },
            State = ProjectState.Active,
            Tags = ["featured", "priority"],
            Metrics = new Dictionary<string, Metric> {
                ["conversion"] = new() { Value = 0.42m, Unit = "%" },
                ["retention"] = new() { Value = 0.87m, Unit = "%" }
            },
            PrimaryPet = new CatPet {
                Name = "Pixel",
                LivesLeft = 8
            },
            BackupPet = new DogPet {
                Name = "Patch",
                GoodBoy = true
            }
        };
    }

    [HttpPost("adoptions")]
    [DefaultProblemResponse]
    [ProducesResponseType<AdoptionRecord>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public ActionResult<AdoptionRecord> CreateAdoption([FromBody] CreateAdoptionRequest request) {
        if (request.Notes == "conflict") {
            return Conflict(new ProblemDetails { Title = "Animal already adopted." });
        }

        return StatusCode(StatusCodes.Status201Created, new AdoptionRecord {
            Id = Guid.Parse("3d5cd6ff-85ed-4b60-9e53-11c9f7c2cb2f"),
            Pet = request.Pet,
            CreatedAt = DateTimeOffset.Parse("2025-04-01T10:00:00+00:00")
        });
    }
}
