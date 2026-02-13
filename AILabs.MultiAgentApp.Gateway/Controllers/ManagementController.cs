using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AILabs.MultiAgentApp.Gateway.Controllers;

[ApiController]
[Route("management")]
public class ManagementController : ControllerBase
{
    private readonly InMemoryProxyConfigProvider _provider;

    public ManagementController(InMemoryProxyConfigProvider provider) => _provider = provider;

    public record RegisterDto(string ClusterId, string Destination);

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ClusterId) || string.IsNullOrWhiteSpace(dto.Destination))
            return BadRequest("clusterId and destination are required");

        _provider.AddOrUpdateDestination(dto.ClusterId, dto.Destination);
        return Ok();
    }

    [HttpPost("unregister")]
    public IActionResult Unregister([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ClusterId) || string.IsNullOrWhiteSpace(dto.Destination))
            return BadRequest("clusterId and destination are required");

        _provider.RemoveDestination(dto.ClusterId, dto.Destination);
        return Ok();
    }
}
