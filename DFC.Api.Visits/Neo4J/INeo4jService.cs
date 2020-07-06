using DFC.Api.Visits.Models;
using System.Threading.Tasks;

namespace DFC.Api.Visits.Neo4J
{
    public interface INeo4JService
    {
        public Task InsertNewRequest(CreateVisitRequest model);
    }
}
