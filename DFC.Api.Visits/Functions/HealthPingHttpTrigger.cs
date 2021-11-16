using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace DFC.Api.Visits.Functions
{
    public static class HealthPingHttpTrigger
    {
        [FunctionName("HealthPing")]
        [Display(Name = "Health ping", Description = "Simple OK response to a health ping")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "OK", ShowSchema = false)]
        public static IActionResult Run(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health/ping")] HttpRequest req, ILogger logger)
        {
            logger.LogInformation($"pinged");

            return new OkResult();
        }
    }
}
