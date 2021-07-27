using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Google.Api;
using Google.Api.Gax.Grpc;
using Google.Api.Gax.ResourceNames;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Logging.Type;
using Google.Cloud.Logging.V2;
using Google.Cloud.Storage.V1;
using Grpc.Core;
using Newtonsoft.Json;
using Xunit;

namespace gcp_logging_tests
{
    public class LogEvents
    {
        public LogEvents()
        {
            if (Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS") == null)
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "/Users/garrettwong/Downloads/sa-key.json");
            }
        }

        private readonly CallSettings _retryAWhile = CallSettings.FromRetry(
            RetrySettings.FromExponentialBackoff(
                maxAttempts: 15,
                initialBackoff: TimeSpan.FromSeconds(3),
                maxBackoff: TimeSpan.FromSeconds(12),
                backoffMultiplier: 2.0,
                retryFilter: RetrySettings.FilterForStatusCodes(StatusCode.Internal, StatusCode.DeadlineExceeded)));

        private void WriteLogEntry(string projectId, string logId, string message)
        {
            var client = LoggingServiceV2Client.Create();
            LogName logName = new LogName(projectId, logId);
            LogEntry logEntry = new LogEntry
            {
                LogNameAsLogName = logName,
                Severity = LogSeverity.Info,
                TextPayload = $"{typeof(LogEvents).FullName} - {message}"
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
        private Google.Api.Gax.PagedEnumerable<ListLogEntriesResponse, LogEntry> ListLogEntries(string projectId, string logId)
        {
            CallSettings _retryAWhile = CallSettings.FromRetry(
            RetrySettings.FromExponentialBackoff(
                maxAttempts: 15, //15
                initialBackoff: TimeSpan.FromSeconds(3),
                maxBackoff: TimeSpan.FromSeconds(12),
                backoffMultiplier: 2.0,
                retryFilter: RetrySettings.FilterForStatusCodes(StatusCode.Internal, StatusCode.DeadlineExceeded)));

            var d = DateTime.Now.AddHours(-12);
            var v = d.ToString("o");

            var client = LoggingServiceV2Client.Create();
            LogName logName = new LogName(projectId, logId);
            ProjectName projectName = new ProjectName(projectId);
            var results = client.ListLogEntries(Enumerable.Repeat(projectName, 1), $"logName={logName.ToString()} AND " +
                $"timestamp >= \"{v}\"", //"timestamp >= \"2021-07-25T2:40:00-04:00\"",
                "timestamp desc", callSettings: _retryAWhile);//_retryAWhile);

            return results;
            
        }

        [Fact]
        public void WriteLogTest()
        {
            WriteLogEntry("gwc-sandbox", "hello", "world");
        }

        /// <summary>
        /// https://cloud.google.com/logging/docs/reference/v2/rest/v2/logs/list
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void ListLogEntriesTest()
        {
            var logEntries = ListLogEntries("gwc-sandbox", "hello");

            var i = 0;
            foreach (var row in logEntries)
            {
                //Console.WriteLine($"{row.TextPayload.Trim()}");
                Console.WriteLine(i + ":");
                Console.WriteLine(JsonConvert.SerializeObject(row));
                i++;
            }
            Assert.NotEmpty(logEntries);
        }
    }
}
