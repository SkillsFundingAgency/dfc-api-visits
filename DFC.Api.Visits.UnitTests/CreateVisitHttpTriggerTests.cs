using DFC.Api.Visits.Function;
using DFC.Api.Visits.Models;
using DFC.Api.Visits.Neo4J;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Api.Visits.UnitTests
{
    public class CreateVisitHttpTriggerTests
    {
        [Fact]
        public async Task WhenCreateVisitCalledThenCallNeo4JService()
        {
            var neoService = A.Fake<INeo4JService>();
            var function = new CreateVisit(neoService);

            var request = A.Fake<HttpRequest>();

            var model = new CreateVisitRequest
            {
                SessionId = Guid.NewGuid(),
                RequestPath = "/",
                Referer = "/",
                VisitTime = DateTime.UtcNow,
                UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36",
            };

            var bytearray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(model));

            A.CallTo(() => request.Body).Returns(new MemoryStream(bytearray));

            await function.Run(request, A.Fake<ILogger>());

            A.CallTo(() => neoService.InsertNewRequest(A<CreateVisitRequest>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenRequestNullThrowsException()
        {
            var neoService = A.Fake<INeo4JService>();
            var function = new CreateVisit(neoService);
            var result = await function.Run(null, A.Fake<ILogger>()).ConfigureAwait(false);
            var statusCodeResult = (StatusCodeResult)result;

            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCodeResult.StatusCode);
        }
    }
}
