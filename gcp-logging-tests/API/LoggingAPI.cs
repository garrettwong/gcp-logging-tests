using Google.Api;
using Google.Api.Gax.Grpc;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Logging.Type;
using Google.Cloud.Logging.V2;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCS = global::Google.Cloud.Storage;

namespace gcp_logging_tests.API
{
    public class LoggingAPI : IDisposable
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public LoggingAPI()
        {

        }

        public void Dispose()
        {

        }

        private static readonly CallSettings _retryAWhile = CallSettings.FromRetry(
            RetrySettings.FromExponentialBackoff(
                maxAttempts: 15,
                initialBackoff: TimeSpan.FromSeconds(3),
                maxBackoff: TimeSpan.FromSeconds(12),
                backoffMultiplier: 2.0,
                retryFilter: RetrySettings.FilterForStatusCodes(StatusCode.Internal, StatusCode.DeadlineExceeded)));

        public static void WriteLogEntry(string projectId, string logId, string message)
        {
            var client = LoggingServiceV2Client.Create();
            LogName logName = new LogName(projectId, logId);
            LogEntry logEntry = new LogEntry
            {
                LogNameAsLogName = logName,
                Severity = LogSeverity.Info,
                TextPayload = $"{typeof(LogEventsTests).FullName} - {message}"
            };
            MonitoredResource resource = new MonitoredResource { Type = "global" };
            IDictionary<string, string> entryLabels = new Dictionary<string, string>
            {
                { "size", "large" },
                { "color", "red" }
            };
            client.WriteLogEntries(logName, resource, entryLabels,
                new[] { logEntry }, _retryAWhile);

            Console.WriteLine($"Created log entry in log-id: {logId}.");
        }


        /// <summary>
        /// https://cloud.google.com/logging/docs/reference/v2/rest/v2/logs/list
        /// </summary>
        /// <returns></returns>
        public static Google.Api.Gax.PagedEnumerable<ListLogEntriesResponse, LogEntry> ListLogEntries(string projectId, string logId)
        {
            CallSettings _retryAWhile = CallSettings.FromRetry(
            RetrySettings.FromExponentialBackoff(
                maxAttempts: 15, //15
                initialBackoff: TimeSpan.FromSeconds(3),
                maxBackoff: TimeSpan.FromSeconds(12),
                backoffMultiplier: 2.0,
                retryFilter: RetrySettings.FilterForStatusCodes(StatusCode.Internal, StatusCode.DeadlineExceeded)));

            var d = DateTime.Now.AddHours(-2);
            var v = d.ToString("o");

            var client = LoggingServiceV2Client.Create();
            LogName logName = new LogName(projectId, logId);
            ProjectName projectName = new ProjectName(projectId);
            var results = client.ListLogEntries(Enumerable.Repeat(projectName, 1), $"logName={logName.ToString()} AND " +
                $"timestamp >= \"{v}\"", //"timestamp >= \"2021-07-25T2:40:00-04:00\"",
                "timestamp desc", callSettings: _retryAWhile);//_retryAWhile);

            return results;
        }

        /// <summary>
        /// https://cloud.google.com/logging/docs/reference/v2/rest/v2/logs/list
        /// </summary>
        /// <returns></returns>
        public static Google.Api.Gax.PagedEnumerable<ListLogEntriesResponse, LogEntry> ListLogEntriesByLogQuery(string projectId, string logQuery)
        {
            CallSettings _retryAWhile = CallSettings.FromRetry(
            RetrySettings.FromExponentialBackoff(
                maxAttempts: 5, //15
                initialBackoff: TimeSpan.FromSeconds(3),
                maxBackoff: TimeSpan.FromSeconds(12),
                backoffMultiplier: 2.0,
                retryFilter: RetrySettings.FilterForStatusCodes(StatusCode.Internal, StatusCode.DeadlineExceeded)));

            var d = DateTime.Now.AddHours(-12);
            var v = d.ToString("o");

            var client = LoggingServiceV2Client.Create();
            ProjectName projectName = new ProjectName(projectId);
            var results = client.ListLogEntries(Enumerable.Repeat(projectName, 1), logQuery,
                "timestamp desc", callSettings: _retryAWhile);//_retryAWhile);

            return results;
        }

        public static void Test(string logId)
        {
            var client = LoggingServiceV2Client.Create();
            LogName logName = new LogName("gwc-sandbox", logId);
            ProjectName projectName = new ProjectName("gwc-sandbox");
            var results = client.ListLogEntries(Enumerable.Repeat(projectName, 1), $"timestamp >= \"2021-07-27T2:40:00-04:00\" AND logName={logName.ToString()}",
                "timestamp desc", callSettings: _retryAWhile);

            var sb = new StringBuilder();
            foreach (var row in results)
            {
                sb.Append(row.TextPayload.Trim());
                //Console.WriteLine($"{row.TextPayload.Trim()}");
            }
            var res = sb.ToString();
        }
    }
}
