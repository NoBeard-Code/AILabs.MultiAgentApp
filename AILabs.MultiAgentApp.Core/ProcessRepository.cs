namespace AILabs.MultiAgentApp.Core;

public interface IProcessRepository
{
    Task<ProcessDefinition?> GetProcessForTenantAgentAsync(string tenantId, string agentId, CancellationToken cancellationToken);
}

public class ProcessRepository : IProcessRepository
{
    // Replace with your DB access (EF Core, Dapper, Cosmos, etc.)
    private static readonly List<TenantAgentProcess> _mockDb = new()
    {
        new TenantAgentProcess
        {
            TenantId = "9f08d951-52d4-4953-9b47-44072988d50a", // GDW Innovations tenant
            AgentId = "f55b52bc-d987-41ea-9808-3c077213c6db", // DefComm Dev - HR Generalist
            Process = new ProcessDefinition
            {
                Id = "proc-hr-1",
                Name = "HR Generalist Process",
                ConfigJson = "{ \"prompt\": \"Handle HR request...\" }"
            }
        },
        new TenantAgentProcess
        {
            TenantId = "9f08d951-52d4-4953-9b47-44072988d50a", // GDW Innovations tenant
            AgentId = "adf94c44-f42b-4971-b18e-5d4010ae5e86", // DefComm Dev - Accountant
            Process = new ProcessDefinition
            {
                Id = "proc-acc-1",
                Name = "Accountant Process",
                ConfigJson = "{ \"prompt\": \"Handle accounting request...\" }"
            }
        },
    };

    public Task<ProcessDefinition?> GetProcessForTenantAgentAsync(string tenantId, string agentId, CancellationToken cancellationToken)
    {
        var match = _mockDb.FirstOrDefault(x =>
            x.TenantId == tenantId &&
            x.AgentId == agentId);

        return Task.FromResult(match?.Process);
    }
}

public class TenantAgentProcess
{
    public string TenantId { get; set; } = "";
    public string AgentId { get; set; } = "";
    public ProcessDefinition Process { get; set; } = default!;
}

public class ProcessDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string ConfigJson { get; set; } = "";
}
