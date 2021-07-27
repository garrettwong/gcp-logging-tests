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

        // logName="projects/gwc-sandbox/logs/cloudaudit.googleapis.com%2Fdata_access"


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BucketsListLogEvent()
        {
            var token = await Access.GetAccessToken();

            // Get Buckets API Call
            var storageUrl = "https://storage.googleapis.com/storage/v1/b?project=gwc-sandbox";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var r = await client.GetStringAsync(storageUrl);

            Console.WriteLine(r);
            Assert.NotNull(r);

            var logEntries = ListLogEntries("gwc-sandbox", "cloudaudit.googleapis.com%2Fdata_access");
            foreach (var row in logEntries)
            {
                Console.WriteLine(JsonConvert.SerializeObject(row));
            }
            Assert.NotEmpty(logEntries);

            /*
{
  "protoPayload": {
    "@type": "type.googleapis.com/google.cloud.audit.AuditLog",
    "status": {},
    "authenticationInfo": {
      "principalEmail": "gcp-csharp-app@gwc-core.iam.gserviceaccount.com",
      "serviceAccountKeyName": "//iam.googleapis.com/projects/gwc-core/serviceAccounts/gcp-csharp-app@gwc-core.iam.gserviceaccount.com/keys/e0a63191fe4c4c3e58e39908732d437c9e77ed39"
    },
    "requestMetadata": {
      "callerIp": "107.139.105.69",
      "callerSuppliedUserAgent": "gzip(gfe)",
      "requestAttributes": {
        "time": "2021-07-27T04:43:45.059367125Z",
        "auth": {}
      },
      "destinationAttributes": {}
    },
    "serviceName": "storage.googleapis.com",
    "methodName": "storage.buckets.list",
    "authorizationInfo": [
      {
        "permission": "storage.buckets.list",
        "granted": true,
        "resourceAttributes": {}
      }
    ],
    "resourceLocation": {
      "currentLocations": [
        "global"
      ]
    }
  },
  "insertId": "-1ded1dejcljm",
  "resource": {
    "type": "gcs_bucket",
    "labels": {
      "project_id": "gwc-sandbox",
      "location": "global",
      "bucket_name": ""
    }
  },
  "timestamp": "2021-07-27T04:43:45.052936915Z",
  "severity": "INFO",
  "logName": "projects/gwc-sandbox/logs/cloudaudit.googleapis.com%2Fdata_access",
  "receiveTimestamp": "2021-07-27T04:43:45.907857582Z"
}
             */
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BucketsObjectReadLogEvent()
        {
            var token = await Access.GetAccessToken();

            // Get Buckets API Call
            var storageUrl = "https://storage.googleapis.com/storage/v1/b?project=gwc-sandbox";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var r = await client.GetStringAsync(storageUrl);

            Console.WriteLine(r);
            Assert.NotNull(r);

            /*
             {
  "protoPayload": {
    "@type": "type.googleapis.com/google.cloud.audit.AuditLog",
    "status": {},
    "authenticationInfo": {
      "principalEmail": "garrettwong@gwongcloud.com"
    },
    "requestMetadata": {
      "callerIp": "35.230.51.15",
      "callerSuppliedUserAgent": "apitools Python/3.7.3 gsutil/4.65 (linux) analytics/disabled interactive/True command/cat google-cloud-sdk/349.0.0",
      "requestAttributes": {
        "time": "2021-07-27T04:43:37.997614383Z",
        "auth": {}
      },
      "destinationAttributes": {}
    },
    "serviceName": "storage.googleapis.com",
    "methodName": "storage.objects.get",
    "authorizationInfo": [
      {
        "resource": "projects/_/buckets/gwc-sandbox-dataflow/objects/my.json",
        "permission": "storage.objects.get",
        "granted": true,
        "resourceAttributes": {}
      }
    ],
    "resourceName": "projects/_/buckets/gwc-sandbox-dataflow/objects/my.json",
    "resourceLocation": {
      "currentLocations": [
        "us"
      ]
    }
  },
  "insertId": "jpsl71dk08y",
  "resource": {
    "type": "gcs_bucket",
    "labels": {
      "location": "us",
      "bucket_name": "gwc-sandbox-dataflow",
      "project_id": "gwc-sandbox"
    }
  },
  "timestamp": "2021-07-27T04:43:37.988793649Z",
  "severity": "INFO",
  "logName": "projects/gwc-sandbox/logs/cloudaudit.googleapis.com%2Fdata_access",
  "receiveTimestamp": "2021-07-27T04:43:38.031478250Z"
} 
             */
        }
    }
}
