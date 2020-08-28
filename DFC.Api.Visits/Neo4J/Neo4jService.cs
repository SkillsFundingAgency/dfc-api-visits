using DFC.Api.Visits.Models;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using UAParser;

namespace DFC.Api.Visits.Neo4J
{
    public class Neo4JService : INeo4JService
    {
        private const string NodeNameTransform = "recommendation__";
        private readonly IGraphDatabase graphDatabase;
        private readonly IServiceProvider serviceProvider;
        private readonly int retentionDays;
        private readonly ILogger log;

        public Neo4JService(IGraphDatabase graphDatabase, IServiceProvider serviceProvider, IOptions<VisitSettings> settings, ILoggerFactory factory)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.graphDatabase = graphDatabase;
            this.serviceProvider = serviceProvider;
            this.retentionDays = settings.Value.RetentionInDays;
            this.log = factory.CreateLogger(LogCategories.CreateFunctionCategory(nameof(Neo4JService)));
        }

        public Task InsertNewRequest(CreateVisitRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return InsertNewRequestInternal(model);
        }

        private static string GenerateVisitKey(Guid userId, string page)
        {
            var newGuid = Guid.NewGuid().ToString("D");

            return $"{userId}{page}/{newGuid}";
        }

        private async Task InsertNewRequestInternal(CreateVisitRequest model)
        {
            var parser = Parser.GetDefault();
            var userAgent = parser.Parse(model.UserAgent);

            var visitId = GenerateVisitKey(model.SessionId, model.RequestPath);

            var customCommand = this.serviceProvider.GetRequiredService<ICustomCommand>();

            customCommand.Command =
                $"merge (u:{NodeNameTransform}user {{id: '{model.SessionId}', browser: '{userAgent.UA.Family}', device:'{userAgent.Device}', OS: '{userAgent.OS}'}})" +
                $"\r\nmerge (v:{NodeNameTransform}visit {{visitId: '{visitId}', dateofvisit:'{model.VisitTime}', referrer: \"{model.Referer}\" }})" +
                $"\r\nmerge (p:{NodeNameTransform}page {{id:\"{model.RequestPath}\"}})" +
                "\r\nwith u,v,p" +
                $"\r\ncreate (v)-[:{NodeNameTransform}PageAccessed]->(p)" +
                $"\r\ncreate (u)-[:{NodeNameTransform}visited]->(v)" +
                "\r\nwith u,v,p " +
                $"\r\nmatch(a:{NodeNameTransform}user)-[:{NodeNameTransform}visited]-(parent)-[:{NodeNameTransform}PageAccessed]-(page)" +
                $"\r\nwhere a.id = '{model.SessionId}' and parent.visitId <> '{visitId}'" +
                "\r\nwith u,v,p,parent,page \r\n" +
                "order by parent.dateOfVisit DESC limit 1" +
                $"\r\nForeach (i in case when page.id = v.referrer then [1] else [] end |\r\ncreate (v)-[:{NodeNameTransform}hasReferrer]->(parent)\r\n)" +
                "\r\nwith u,v,parent \r\n" +
                $"CALL apoc.ttl.expireIn(u,toInteger({this.retentionDays}),'d') " +
                "\r\nwith u,v,parent \r\n" +
                $"CALL apoc.ttl.expireIn(v,toInteger({this.retentionDays}),'d') " +
                "\r\nwith u,v,parent \r\n" +
                $"CALL apoc.ttl.expireIn(parent,toInteger({this.retentionDays}),'d') return u";

            log.LogInformation("Adding visit to neo4j");

            try
            {
                await this.graphDatabase.Run(customCommand).ConfigureAwait(false);
                log.LogInformation("visit successfully added to neo4j");
            }
            catch (Exception e)
            {
                log.LogError($"Unable to add visit to neo4j, failed with error : {e}");
            }
        }
    }
}
