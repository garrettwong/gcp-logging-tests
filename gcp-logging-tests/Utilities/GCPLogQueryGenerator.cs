using System;
using System.Text;

namespace gcp_logging_tests.Utilities
{
    public class GCPLogQueryGenerator
    {
        public string GetDataAccessLogQuery(string projectId, int minutesAgo)
        {
            var timestamp = DateTime.Now.AddMinutes(-1 * minutesAgo)
                .ToString("o");

            var sb = new StringBuilder();
            sb.Append(@"logName=""projects/[PROJECT_ID]/logs/cloudaudit.googleapis.com%2Fdata_access"" AND ".Replace("[PROJECT_ID]", projectId));
            sb.Append($@" timestamp >= ""{timestamp}""");

            return sb.ToString();
        }

        public string GetDataAccessLogQuery(string projectId, string serviceName, string methodName, int minutesAgo)
        {
            var timestamp = DateTime.Now.AddMinutes(-1 * minutesAgo)
                .ToString("o");

            var sb = new StringBuilder();
            sb.Append(@"logName=""projects/[PROJECT_ID]/logs/cloudaudit.googleapis.com%2Fdata_access"" AND ".Replace("[PROJECT_ID]", projectId));
            sb.Append($"protoPayload.serviceName=\"{serviceName}\" AND ");
            sb.Append($"protoPayload.methodName=\"{methodName}\" AND ");
            sb.Append($@" timestamp >= ""{timestamp}""");

            return sb.ToString();
        }
    }
}
