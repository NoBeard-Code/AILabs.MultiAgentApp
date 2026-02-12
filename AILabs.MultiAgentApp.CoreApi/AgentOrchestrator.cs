using Microsoft.Bot.Schema;

namespace AILabs.MultiAgentApp.CoreApi;

public interface IAgentOrchestrator
{
    Task<Activity> ProcessAsync(string agentId, Activity incoming);
}

public class AgentOrchestrator : IAgentOrchestrator
{
    public async Task<Activity> ProcessAsync(string agentId, Activity incoming)
    {
        // TODO: Replace with Microsoft.Agents orchestration logic

        var reply = incoming.CreateReply();
        reply.Text = $"[{agentId}] You said: {incoming.Text}";

        return await Task.FromResult(reply);
    }
}
