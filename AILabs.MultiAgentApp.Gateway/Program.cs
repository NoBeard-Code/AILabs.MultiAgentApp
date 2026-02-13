using AILabs.MultiAgentApp.Gateway;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Register the in-memory provider and YARP
builder.Services.AddSingleton<InMemoryProxyConfigProvider>();
builder.Services.AddSingleton<IProxyConfigProvider>(sp => sp.GetRequiredService<InMemoryProxyConfigProvider>());

builder.Services.AddReverseProxy();

// Add management controller
builder.Services.AddControllers();

//builder.Services.AddReverseProxy()
//    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.Urls.Add($"https://localhost:3999");
}

app.Run();
