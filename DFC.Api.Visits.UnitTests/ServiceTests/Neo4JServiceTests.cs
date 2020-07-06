using DFC.Api.Visits.Models;
using DFC.Api.Visits.Neo4J;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using FakeItEasy;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Visits.UnitTests.ServiceTests
{
    public class Neo4JServiceTests
    {
        private readonly IServiceProvider _provider;
        private readonly IGraphDatabase _graphDatabase;

        public Neo4JServiceTests()
        {
            _provider = A.Fake<IServiceProvider>();
            _graphDatabase = A.Fake<IGraphDatabase>();
            A.CallTo(() => _graphDatabase.Run(A<ICommand[]>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => _provider.GetService(typeof(ICustomCommand))).Returns(new CustomCommand());

        }

        [Fact]
        public async Task WhenInsertNewRequestThenUpdateNeo4J()
        {
            var model = new CreateVisitRequest
            {
                SessionId = Guid.NewGuid(),
                RequestPath = "/",
                Referer = "/",
                VisitTime = DateTime.UtcNow,
                UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36",
            };

            var service = new Neo4JService(_graphDatabase, _provider);
            await service.InsertNewRequest(model).ConfigureAwait(false);
            A.CallTo(() => _graphDatabase.Run(A<ICommand[]>.Ignored)).MustHaveHappened();
        }
    }
}
