using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;

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
    public async Task<Activity> PostAsync(string agentId, [FromBody] Activity incoming)
    {
        return await _orchestrator.ProcessAsync(agentId, incoming); 
    }
}
