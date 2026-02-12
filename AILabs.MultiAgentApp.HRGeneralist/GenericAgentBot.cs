namespace AILabs.MultiAgentApp.HRGeneralist;

//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Connector.Authentication;
//using Microsoft.Bot.Schema;

//public class GenericAgentBot : IBot
//{
//    private readonly HttpClient _core;
//    private readonly IConfiguration _config;

//    public GenericAgentBot(IHttpClientFactory factory, IConfiguration config)
//    {
//        _core = factory.CreateClient("CoreBackend");
//        _config = config;
//    }

//    public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
//    {
//        if (turnContext.Activity.Type != ActivityTypes.Message)
//            return;

//        var message = turnContext.Activity.AsMessageActivity();

//        Console.WriteLine("Incoming activity recipient: " + turnContext.Activity.Recipient?.Id);
//        Console.WriteLine("Bot AppId configured: " + _config["MicrosoftAppId"]);

//        var tenantId =
//            turnContext.Activity.Conversation?.TenantId ??
//            turnContext.Activity.GetChannelData<dynamic>()?.tenant?.id ??
//            string.Empty;

//        var rawBotId = turnContext.Activity.Recipient?.Id ?? string.Empty; // "28:<appId>"
//        var appId = rawBotId.StartsWith("28:") ? rawBotId[3..] : rawBotId;

//        var text = turnContext.Activity.Text ?? string.Empty;

//        var agentId = _config["BOT_ID"] ?? string.Empty;

//        var response = await _core.PostAsJsonAsync(
//            $"/api/teams/{agentId}/messages",
//            turnContext.Activity,
//            cancellationToken);

//        var reply = await response.Content.ReadFromJsonAsync<Activity>(cancellationToken: cancellationToken);

//        //MicrosoftAppCredentials.TrustServiceUrl(turnContext.Activity.ServiceUrl);

//        await turnContext.SendActivityAsync(reply, cancellationToken);
//    }
//}
