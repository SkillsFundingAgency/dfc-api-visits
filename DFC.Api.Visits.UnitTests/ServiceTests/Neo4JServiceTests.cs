using DFC.Api.Visits.Models;
using DFC.Api.Visits.Neo4J;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using FakeItEasy;
using Microsoft.Extensions.Logging;
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
        private readonly ILoggerFactory logFactory;
        private readonly FakeLogger log;

        public Neo4JServiceTests()
        {
            provider = A.Fake<IServiceProvider>();
            graphDatabase = A.Fake<IGraphDatabase>();
            logFactory = A.Fake<ILoggerFactory>();
            log = A.Fake<FakeLogger>();

            A.CallTo(() => graphDatabase.Run(A<ICommand[]>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => provider.GetService(typeof(ICustomCommand))).Returns(new CustomCommand());
            A.CallTo(() => logFactory.CreateLogger(A<string>.Ignored)).Returns(log);
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

            var service = new Neo4JService(graphDatabase, provider, settings, logFactory);
            await service.InsertNewRequest(model).ConfigureAwait(false);
            A.CallTo(() => graphDatabase.Run(A<ICommand[]>.Ignored)).MustHaveHappened();
            A.CallTo(() => log.Log(LogLevel.Information, A<Exception>.Ignored, A<string>.Ignored))
                .MustHaveHappened(2, Times.Exactly);
        }

        [Fact]
        public Task WhenCreateVisitRequestNullThrowException()
        {
            var service = new Neo4JService(graphDatabase, provider, settings, logFactory);

            return Assert
                .ThrowsAsync<ArgumentNullException>(async () =>
                    await service.InsertNewRequest(null).ConfigureAwait(false));
        }

        [Fact]
        public void WhenSettingsNullThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new Neo4JService(graphDatabase, provider, null, logFactory));
        }

        [Fact]
        public async Task WhenInsertNewRequestFailsThenLogError()
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

            A.CallTo(() => graphDatabase.Run(A<ICommand[]>.Ignored)).Throws(new Exception());

            var service = new Neo4JService(graphDatabase, provider, settings, logFactory);
            await service.InsertNewRequest(model).ConfigureAwait(false);
            A.CallTo(() => graphDatabase.Run(A<ICommand[]>.Ignored)).MustHaveHappened();
            A.CallTo(() => log.Log(LogLevel.Error, A<Exception>.Ignored, A<string>.Ignored))
                .MustHaveHappened(1, Times.Exactly);
        }
    }
}
