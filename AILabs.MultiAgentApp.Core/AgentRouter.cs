using Microsoft.Agents.Builder;

namespace AILabs.MultiAgentApp.Core;

public interface IAgentRouter
{
    Task<ProcessDefinition?> ResolveAsync(ITurnContext turnContext, CancellationToken cancellationToken);
}

public class AgentRouter(IProcessRepository repo) : IAgentRouter
{
    public async Task<ProcessDefinition?> ResolveAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        var tenantId = turnContext.Activity.Conversation.TenantId;
        var agentId = turnContext.Activity.Recipient?.Id; // Bot/App/Agent ID

        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(agentId))
            return null;

        // Normalize AppId
        var agentAppId = agentId.StartsWith("28:") ? agentId.Substring(3) : agentId;

        await turnContext.SendActivityAsync($"DEBUG: TenantId: {tenantId}", cancellationToken: cancellationToken);
        await turnContext.SendActivityAsync($"DEBUG: AgentAppId: {agentAppId}", cancellationToken: cancellationToken);

        return await repo.GetProcessForTenantAgentAsync(tenantId, agentId, cancellationToken);
    }
}
