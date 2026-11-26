using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoftwareDeployment.Client.Service;
using SoftwareDeployment.Shared;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Run as a Windows Service (installed mode) or console (debug).
builder.Services.AddWindowsService(options => options.ServiceName = DeploymentConstants.ClientServiceName);
builder.Services.AddHostedService<PollingWorker>();

builder.Build().Run();
