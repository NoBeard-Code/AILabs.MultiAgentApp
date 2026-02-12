using AILabs.MultiAgentApp.Core;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddControllers();

// Add AgentApplicationOptions from appsettings section "AgentApplication".
builder.AddAgentApplicationOptions();

// Domain services
builder.Services.AddSingleton<IAgentRouter, AgentRouter>();
builder.Services.AddSingleton<IProcessRepository, ProcessRepository>();
builder.Services.AddSingleton<IProcessExecutor, ProcessExecutor>();

// Add the AgentApplication, which contains the logic for responding to user messages.
builder.AddAgent<MyAgent>();

// Add AspNet token validation for Azure Bot Service and Entra.  Authentication is
// configured in the appsettings.json "TokenValidation" section.
builder.Services.AddAgentAspNetAuthentication(builder.Configuration);

// Register IStorage.  For development, MemoryStorage is suitable.
// For production Agents, persisted storage should be used so
// that state survives Agent restarts, and operates correctly
// in a cluster of Agent instances.
builder.Services.AddSingleton<IStorage, MemoryStorage>();

builder.Services.AddHttpClient("CoreBackend", client => 
{ 
    client.BaseAddress = new Uri(builder.Configuration["BACKEND_URL"]); 
});

var app = builder.Build();

var routePrefix = builder.Configuration["BOT_ID"];


// Enable AspNet authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// This receives incoming messages from Azure Bot Service or other SDK Agents
var incomingRoute = app.MapPost($"/{routePrefix}/api/messages",
    async (HttpRequest request, HttpResponse response, IAgentHttpAdapter adapter, IAgent agent, CancellationToken cancellationToken) =>
    {
        await adapter.ProcessAsync(request, response, agent, cancellationToken);
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
