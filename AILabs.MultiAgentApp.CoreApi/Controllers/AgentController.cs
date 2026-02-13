using Microsoft.AspNetCore.Mvc;

namespace AILabs.MultiAgentApp.CoreApi.Controllers;

[ApiController]
[Route("api/teams/{agentId}/messages")]
public class AgentController : ControllerBase
{
    private readonly IAgentOrchestrator _orchestrator; 
    
    public AgentController(IAgentOrchestrator orchestrator) 
    { 
        _orchestrator = orchestrator; 
    }

    [HttpPost] 
    public async Task<string> PostAsync(string agentId, [FromBody] string message)
    {
        return await _orchestrator.ProcessAsync(agentId, message); 
    }
}
