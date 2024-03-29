﻿using DFC.Api.Visits.Models;
using DFC.Api.Visits.Neo4J;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DFC.Api.Visits.Functions
{
    public class CreateVisit
    {
        private readonly INeo4JService neo4JService;

        public CreateVisit(INeo4JService neo4JService)
        {
            this.neo4JService = neo4JService;
        }

        [FunctionName("CreateVisit")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateVisit")]
            HttpRequest req, ILogger logger)
        {
            try
            {
                if (req == null)
                {
                    throw new ArgumentNullException(nameof(req));
                }

                string content;

                using (var str = new StreamReader(req.Body))
                {
                    content = await str.ReadToEndAsync().ConfigureAwait(false);
                }

                var model = JsonConvert.DeserializeObject<CreateVisitRequest>(content);

                await this.neo4JService.InsertNewRequest(model).ConfigureAwait(false);

                return new OkResult();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                logger.LogError(ex.ToString());
                return new StatusCodeResult(500);
            }
        }
    }
}
