using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AILabs.MultiAgentApp.CoreApi;

public record AgentEntry(string BotId, int Port);
public record AgentManifest(string AgentExe, List<AgentEntry> Agents, string GatewayManagementUrl);

public class AgentSupervisorService : BackgroundService
{
    private readonly ILogger<AgentSupervisorService> _log;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly Dictionary<string, Process> _processes = new();

    public AgentSupervisorService(ILogger<AgentSupervisorService> log, IHttpClientFactory httpFactory, IConfiguration config)
    {
        _log = log;
        _httpFactory = httpFactory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var manifest = LoadManifest();
        var tasks = manifest.Agents.Select(a => RunAgentLoop(manifest.AgentExe, a, manifest.GatewayManagementUrl, stoppingToken)).ToArray();
        await Task.WhenAll(tasks);
    }

    private AgentManifest LoadManifest()
    {
        var exe = _config["AgentExe"] ?? throw new Exception("AgentExe missing");
        var agents = _config.GetSection("Agents").Get<List<AgentEntry>>() ?? new List<AgentEntry>();
        var mgmt = _config["Gateway:ManagementUrl"] ?? throw new Exception("Gateway:ManagementUrl missing");
        return new AgentManifest(exe, agents, mgmt);
    }

    private string ResolveAgentExe(string relativePath)
    {
        var baseDir = AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(baseDir, relativePath));
    }

    private async Task RunAgentLoop(string relativeExe, AgentEntry entry, string gatewayMgmtUrl, CancellationToken stoppingToken)
    {
        var exePath = ResolveAgentExe(relativeExe);
        var backoff = TimeSpan.FromSeconds(1);
        var maxBackoff = TimeSpan.FromSeconds(30);
        var attempts = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _log.LogInformation("Starting agent {bot} on port {port} using {exe}", entry.BotId, entry.Port, exePath);
                var proc = StartProcess(exePath, entry.BotId, entry.Port);
                _processes[entry.BotId] = proc;

                await WaitForHealthAsync(entry.Port, TimeSpan.FromSeconds(15), stoppingToken);

                await RegisterWithGatewayAsync(gatewayMgmtUrl, entry.BotId, $"http://localhost:{entry.Port}", stoppingToken);

                await proc.WaitForExitAsync(stoppingToken);
                _log.LogWarning("Agent {bot} exited with code {code}", entry.BotId, proc.ExitCode);

                await UnregisterFromGatewayAsync(gatewayMgmtUrl, entry.BotId, $"http://localhost:{entry.Port}", stoppingToken);

                attempts++;
                if (attempts > 10)
                {
                    _log.LogError("Agent {bot} exceeded restart attempts, stopping restarts", entry.BotId);
                    break;
                }

                await Task.Delay(backoff, stoppingToken);
                backoff = TimeSpan.FromSeconds(Math.Min(maxBackoff.TotalSeconds, backoff.TotalSeconds * 2));
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _log.LogError(ex, "Supervisor loop error for {bot}", entry.BotId);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private Process StartProcess(string exePath, string botId, int port)
    {
        var psi = new ProcessStartInfo(exePath)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        psi.Environment["BOT_ID"] = botId;
        psi.Environment["BACKEND_URL"] = _config["CoreApi:Url"] ?? "";
        psi.Environment["ASPNETCORE_URLS"] = $"http://localhost:{port}";
        psi.WorkingDirectory = Path.GetDirectoryName(exePath);

        var proc = Process.Start(psi)!;
        proc.OutputDataReceived += (s, e) => { if (e.Data != null) _log.LogInformation("[{bot}] {line}", botId, e.Data); };
        proc.ErrorDataReceived += (s, e) => { if (e.Data != null) _log.LogError("[{bot}] {line}", botId, e.Data); };
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();
        return proc;
    }

    private async Task WaitForHealthAsync(int port, TimeSpan timeout, CancellationToken ct)
    {
        using var client = new HttpClient();
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < timeout)
        {
            try
            {
                var r = await client.GetAsync($"http://localhost:{port}/health", ct);
                if (r.IsSuccessStatusCode) return;
            }
            catch { }
            await Task.Delay(200, ct);
        }
        throw new TimeoutException($"Agent on port {port} did not become healthy in {timeout}");
    }

    private async Task RegisterWithGatewayAsync(string mgmtUrl, string botId, string address, CancellationToken ct)
    {
        var client = _httpFactory.CreateClient();
        client.BaseAddress = new Uri(mgmtUrl);
        var payload = new { clusterId = $"{botId}-cluster", destination = address };
        var r = await client.PostAsJsonAsync("/management/register", payload, ct);
        r.EnsureSuccessStatusCode();
        _log.LogInformation("Registered {bot} -> {addr}", botId, address);
    }

    private async Task UnregisterFromGatewayAsync(string mgmtUrl, string botId, string address, CancellationToken ct)
    {
        var client = _httpFactory.CreateClient();
        client.BaseAddress = new Uri(mgmtUrl);
        var payload = new { clusterId = $"{botId}-cluster", destination = address };
        await client.PostAsJsonAsync("/management/unregister", payload, ct);
        _log.LogInformation("Unregistered {bot}", botId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Supervisor stopping, terminating children");
        foreach (var kv in _processes)
        {
            try
            {
                if (!kv.Value.HasExited)
                {
                    kv.Value.Kill(true);
                    await kv.Value.WaitForExitAsync(cancellationToken);
                }
            }
            catch (Exception ex) { _log.LogWarning(ex, "Error killing {bot}", kv.Key); }
        }
        await base.StopAsync(cancellationToken);
    }
}
