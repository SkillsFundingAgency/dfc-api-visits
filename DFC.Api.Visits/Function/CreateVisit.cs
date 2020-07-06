using DFC.Api.Visits.Models;
using DFC.Api.Visits.Neo4J;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace DFC.Api.Visits.Function
{
    public class CreateVisit
    {
        private readonly INeo4JService _neo4JService;

        public CreateVisit(INeo4JService neo4JService)
        {
            _neo4JService = neo4JService;
        }


        [FunctionName("CreateVisit")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateVisit")]
            HttpRequest req, ILogger log)
        {
            var content = await new StreamReader(req.Body).ReadToEndAsync();

            var model = JsonConvert.DeserializeObject<CreateVisitRequest>(content);

            await _neo4JService.InsertNewRequest(model);

            return new OkResult();
        }
    }
}
