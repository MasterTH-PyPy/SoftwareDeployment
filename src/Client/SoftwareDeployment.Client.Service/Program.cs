using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoftwareDeployment.Client.Service;
using SoftwareDeployment.Shared;

// Installer-driven headless registration: "register --server <url> --key <key>".
if (args.Length > 0 && args[0].Equals("register", StringComparison.OrdinalIgnoreCase))
{
    return await RegisterCommand.RunAsync(args);
}

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Run as a Windows Service (installed mode) or console (debug).
builder.Services.AddWindowsService(options => options.ServiceName = DeploymentConstants.ClientServiceName);
builder.Services.AddHostedService<PollingWorker>();

builder.Build().Run();
return 0;
