var builder = DistributedApplication.CreateBuilder(args);

var coreApi = builder.AddProject<Projects.AILabs_MultiAgentApp_CoreApi>("coreapi");

var hrGeneralistAgent = builder.AddProject<Projects.AILabs_MultiAgentApp_AgentHost>("agent-hrgeneralist")
    .WithEnvironment("Connections__ServiceConnection__Settings__ClientId", "1f68832a-05c0-4725-9f77-a7d590758a3a")
    .WithEnvironment("Connections__ServiceConnection__Settings__ClientSecret", "qCo8Q~kqV_BRgvTf8M.xNcwBj4GcUY0MtcJ5DdeD")
    .WithEnvironment("Connections__ServiceConnection__Settings__AuthorityEndpoint", "https://login.microsoftonline.com/9f08d951-52d4-4953-9b47-44072988d50a")
    .WithEnvironment("BOT_ID", "hrgeneralist")
    .WithEnvironment("BACKEND_URL", coreApi.GetEndpoint("https"))
    .WithEndpoint("https", e => e.Port = 7101)
    .WithReference(coreApi);

var accountantAgent = builder.AddProject<Projects.AILabs_MultiAgentApp_AgentHost>("agent-accountant")
    .WithEnvironment("Connections__ServiceConnection__Settings__ClientId", "1d280068-1b03-4590-b5c0-8b36c686bdc5")
    .WithEnvironment("Connections__ServiceConnection__Settings__ClientSecret", "mt48Q~F7LlGgCj.JSzgm8EpuXTB0rHQWTCbI9arU")
    .WithEnvironment("Connections__ServiceConnection__Settings__AuthorityEndpoint", "https://login.microsoftonline.com/9f08d951-52d4-4953-9b47-44072988d50a")
    .WithEnvironment("BOT_ID", "accountant")
    .WithEnvironment("BACKEND_URL", coreApi.GetEndpoint("https"))
    .WithEndpoint("https", e => e.Port = 7102)
    .WithReference(coreApi);

var gateway = builder.AddProject<Projects.AILabs_MultiAgentApp_Gateway>("gateway")
    //.WithEnvironment("HR_PORT", hrGeneralistAgent.GetEndpoint("http"))
    //.WithEnvironment("ACCOUNTANT_PORT", accountantAgent.GetEndpoint("http"))
    .WithEndpoint("http", e => e.Port = 3999)
    .WithReference(hrGeneralistAgent)
    .WithReference(accountantAgent);

builder.Build().Run();
