using System.Diagnostics.CodeAnalysis;
using System.IO;
using DFC.Api.Visits.Models;
using DFC.Api.Visits.Neo4J;
using DFC.Api.Visits.StartUp;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Configuration;
using DFC.ServiceTaxonomy.Neo4j.Log;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;

[assembly: FunctionsStartup(typeof(FunctionStartupExtension))]

namespace DFC.Api.Visits.StartUp
{
    [ExcludeFromCodeCoverage]
    public class FunctionStartupExtension : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();




            builder.Services.AddSingleton<IConfiguration>(config);
            
            builder.Services.AddOptions<Neo4jConfiguration>()
                .Configure<IConfiguration>((settings, configuration) => { configuration.GetSection("Neo4j").Bind(settings); });
            builder.Services.AddOptions<VisitSettings>()
                .Configure<IConfiguration>((settings, configuration) => { configuration.GetSection(nameof(VisitSettings)).Bind(settings); });
            builder.Services.AddTransient<ILogger, NeoLogger>();
            builder.Services.AddTransient<INeo4JService, Neo4JService>();
            builder.Services.AddSingleton<INeoDriverBuilder, NeoDriverBuilder>();
            builder.Services.AddSingleton<IGraphDatabase, NeoGraphDatabase>(); 
            builder.Services.AddTransient<ICustomCommand, CustomCommand>();
        }
    }
}
