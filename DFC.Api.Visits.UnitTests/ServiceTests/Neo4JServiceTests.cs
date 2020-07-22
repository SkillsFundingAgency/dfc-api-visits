using DFC.Api.Visits.Models;
using DFC.Api.Visits.Neo4J;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using FakeItEasy;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Visits.UnitTests.ServiceTests
{
    public class Neo4JServiceTests
    {
        private readonly IServiceProvider provider;
        private readonly IGraphDatabase graphDatabase;
        private readonly IOptions<VisitSettings> settings;

        public Neo4JServiceTests()
        {
            provider = A.Fake<IServiceProvider>();
            graphDatabase = A.Fake<IGraphDatabase>();
            A.CallTo(() => graphDatabase.Run(A<ICommand[]>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => provider.GetService(typeof(ICustomCommand))).Returns(new CustomCommand());
            settings = Options.Create(new VisitSettings
            {
                RetentionInDays = 90,
            });
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

            var service = new Neo4JService(graphDatabase, provider, settings);
            await service.InsertNewRequest(model).ConfigureAwait(false);
            A.CallTo(() => graphDatabase.Run(A<ICommand[]>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenCreateVisitRequestNullThrowException()
        {
            var service = new Neo4JService(graphDatabase, provider, settings);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.InsertNewRequest(null).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task WhenSettingsNullThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new Neo4JService(graphDatabase, provider, null));
        }
    }
}
