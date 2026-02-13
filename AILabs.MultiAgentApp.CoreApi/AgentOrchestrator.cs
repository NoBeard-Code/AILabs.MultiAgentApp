namespace AILabs.MultiAgentApp.CoreApi;

public interface IAgentOrchestrator
{
    Task<string> ProcessAsync(string agentId, string message);
}

public class AgentOrchestrator : IAgentOrchestrator
{
    public async Task<string> ProcessAsync(string agentId, string message)
    {
        // TODO: Replace with Microsoft.Agents orchestration logic

        var result = $"[{agentId}] You said: {message}";

        return result;
    }
}
