using Microsoft.Agents.Builder;

namespace AILabs.MultiAgentApp.Core;

public interface IProcessExecutor
{
    Task ExecuteAsync(ProcessDefinition process, ITurnContext turnContext, CancellationToken cancellationToken);
}

public class ProcessExecutor : IProcessExecutor
{
    public async Task ExecuteAsync(ProcessDefinition process, ITurnContext turnContext, CancellationToken cancellationToken)
    {
        // Example: simple echo with process name
        await turnContext.SendActivityAsync(
            $"[{process.Name}] You said: {turnContext.Activity.Text}",
            cancellationToken: cancellationToken);
    }
}

