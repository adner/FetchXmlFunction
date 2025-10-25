using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddSingleton<IOrganizationService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    string url = configuration["Dataverse:Url"] ?? throw new InvalidOperationException("Dataverse:Url not configured");
    string clientId = configuration["Dataverse:ClientId"] ?? throw new InvalidOperationException("Dataverse:ClientId not configured");
    string secret = configuration["Dataverse:Secret"] ?? throw new InvalidOperationException("Dataverse:Secret not configured");

    string connectionString = $@"
    Url = {url};
    AuthType = ClientSecret;
    ClientId = {clientId};
    Secret = {secret}";

    return new ServiceClient(connectionString);
});

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
