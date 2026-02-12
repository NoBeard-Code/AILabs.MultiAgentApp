using AILabs.MultiAgentApp.HRGeneralist;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddControllers();

// Bot Framework auth (Azure Bot Service)
builder.Services.AddSingleton<BotFrameworkAuthentication>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new ConfigurationBotFrameworkAuthentication(config);
});

// Adapter
builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(sp =>
{
    var auth = sp.GetRequiredService<BotFrameworkAuthentication>();
    var logger = sp.GetRequiredService<ILogger<CloudAdapter>>();
    return new CloudAdapter(auth, logger);
});

builder.Services.AddTransient<IBot, GenericAgentBot>();

builder.Services.AddHttpClient("CoreBackend", client => 
{ 
    client.BaseAddress = new Uri(builder.Configuration["BACKEND_URL"]); 
});

var app = builder.Build();

var routePrefix = builder.Configuration["BOT_ID"];

// This receives incoming messages from Azure Bot Service or other SDK Agents
var incomingRoute = app.MapPost($"/{routePrefix}/api/messages", (HttpRequest req, HttpResponse res, IBotFrameworkHttpAdapter adapter, IBot bot) =>
{
    adapter.ProcessAsync(req, res, bot);
});    

if (!app.Environment.IsDevelopment())
{
    incomingRoute.RequireAuthorization();
}
else
{
    // Hardcoded for brevity and ease of testing. 
    // In production, this should be set in configuration.
    app.Urls.Add($"http://localhost:3999");
}

app.Run();
