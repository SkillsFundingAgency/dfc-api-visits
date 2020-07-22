using System;

namespace DFC.Api.Visits.Models
{
    public class CreateVisitRequest
    {
        public Guid SessionId { get; set; }

        public string Referer { get; set; }

        public string UserAgent { get; set; }

        public string RequestPath { get; set; }

        public DateTime VisitTime { get; set; }
    }
}
