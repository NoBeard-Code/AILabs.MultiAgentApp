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

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapControllers();

app.Run();
