using AILabs.MultiAgentApp.CoreApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Use Newtonsoft.Json for Activity deserialization
builder.Services.AddControllers()
    .AddNewtonsoftJson(options => 
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); 
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });

builder.Services.AddSingleton<IAgentOrchestrator, AgentOrchestrator>();

builder.Services.AddHttpClient("gateway-mgmt", client =>
{
    client.BaseAddress = new Uri("https://localhost:3999");
    // configure auth/cert if needed
});

builder.Services.AddHostedService<AgentSupervisorService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapControllers();

app.Run();
