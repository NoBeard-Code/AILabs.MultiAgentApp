using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace AILabs.MultiAgentApp.Core;

public class MyAgent : AgentApplication
{
    private readonly IAgentRouter _router;
    private readonly IProcessExecutor _executor;
    private readonly HttpClient _core;
    private readonly IConfiguration _config;

    public MyAgent(AgentApplicationOptions options,
        IAgentRouter router, IProcessExecutor executor, IHttpClientFactory factory, IConfiguration config) : base(options)
    {
        _router = router;
        _executor = executor;
        _core = factory.CreateClient("CoreBackend");
        _config = config;

        OnConversationUpdate(ConversationUpdateEvents.MembersAdded, WelcomeMessageAsync);
        OnActivity(ActivityTypes.Message, OnMessageAsync, rank: RouteRank.Last);
    }

    private async Task WelcomeMessageAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        foreach (ChannelAccount member in turnContext.Activity.MembersAdded)
        {
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Hello and Welcome!"), cancellationToken);
            }
        }
    }

    private async Task OnMessageAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        if (turnContext.Activity.Type != ActivityTypes.Message)
            return;

        // 1. Resolve tenant + agent → process
        var process = await _router.ResolveAsync(turnContext, cancellationToken);

        //if (process == null)
        //{
        //    await turnContext.SendActivityAsync("No process configured for this agent in your tenant.", cancellationToken: cancellationToken);
        //    return;
        //}

        var text = turnContext.Activity.Text ?? string.Empty;

        var agentId = _config["BOT_ID"] ?? string.Empty;

        var response = await _core.PostAsJsonAsync($"/api/teams/{agentId}/messages", text, cancellationToken);

        var echo = await response.Content.ReadAsStringAsync(cancellationToken);

        var reply = turnContext.Activity.CreateReply(echo);

        //var reply = await response.Content.ReadFromJsonAsync<Activity>(cancellationToken: cancellationToken);

        await turnContext.SendActivityAsync(reply, cancellationToken);

        // 2. Execute the process
        //await _executor.ExecuteAsync(process, turnContext, cancellationToken);
    }
}