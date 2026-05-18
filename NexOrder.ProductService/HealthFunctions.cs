using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;

public class HealthFunctions
{
    private readonly HealthCheckService healthCheckService;

    public HealthFunctions(HealthCheckService healthCheckService)
    {
        this.healthCheckService = healthCheckService;
    }

    [Function("Health")]
    [OpenApiOperation(operationId: "Health", tags: new[] { "Health" }, Description = "Health check endpoint for the service.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Service is healthy.")]
    public async Task<IActionResult> Health([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest req)
    {
       var healthCheckResult = await this.healthCheckService.CheckHealthAsync();
       // Map the results into a clean, detailed anonymous object
        var detailedResponse = new
        {
            status = healthCheckResult.Status.ToString(),
            totalDurationMs = healthCheckResult.TotalDuration.TotalMilliseconds,
            results = healthCheckResult.Entries.Select(entry => new
            {
                component = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                errorMessage = entry.Value.Exception?.Message, 
                tags = entry.Value.Tags
            })
        };
        
       return new OkObjectResult(detailedResponse);
    }
}