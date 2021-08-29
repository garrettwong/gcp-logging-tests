using gcp_logging_tests.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace gcp_logging_tests
{
    /// Log Entries List Example
    /// https://github.com/GoogleCloudPlatform/dotnet-docs-samples/tree/master/logging/api
    /// /Users/garrettwong/Git/dotnet-docs-samples/logging/api/LoggingSample
    /// dotnet 5.0.x
    public class LogEventsTests
    {
        private readonly string _projectId;

        public LogEventsTests()
        {
            _projectId = Global.PROJECT_ID;

            Access.Initiailize();
        }

        [Fact]
        public void WriteLogTest()
        {
            LoggingAPI.WriteLogEntry(Global.PROJECT_ID, "hello", "world");
        }

        /// <summary>
        /// https://cloud.google.com/logging/docs/reference/v2/rest/v2/logs/list
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void ListLogEntriesTest()
        {
            // Write twice just in case
            LoggingAPI.WriteLogEntry(Global.PROJECT_ID, "hello", "world");
            Thread.Sleep(5000);
            LoggingAPI.WriteLogEntry(Global.PROJECT_ID, "hello", "world2");
            Thread.Sleep(5000);

            var logEntries = LoggingAPI.ListLogEntries(Global.PROJECT_ID, "hello");

            Assert.NotEmpty(logEntries);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BucketsListLogEvent()
        {
            var token = await Access.GetAccessToken();

            // Get Buckets API Call
            var storageUrl = $"https://storage.googleapis.com/storage/v1/b?project={_projectId}";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var r = await client.GetStringAsync(storageUrl);

            Assert.NotNull(r);

            var d = DateTime.Now.AddHours(-2);
            var v = d.ToString("o");
            var logEntries = LoggingAPI.ListLogEntriesByLogQuery(_projectId,
                $"logName=\"projects/{_projectId}/logs/cloudaudit.googleapis.com%2Fdata_access\" AND " +
                $"protoPayload.serviceName=\"storage.googleapis.com\" AND timestamp >= \"{v}\"");
            
            foreach (var row in logEntries)
            {
                var log = JsonConvert.SerializeObject(row);

                var jsonLog = JsonConvert.DeserializeObject(log);

                var jo = JObject.Parse(log);

                JToken acme = jo.SelectToken("$.protoPayload.authenticationInfo.principalEmail");
                //var s = acme.ToString();

                Assert.Equal("gcs_bucket", jo.SelectToken("$.Resource.Type").ToString());

                break; // the first log element should be the one that was just logged
            }

            Assert.NotEmpty(logEntries);
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
            var storageUrl = $"https://storage.googleapis.com/storage/v1/b?project={_projectId}";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await client.GetStringAsync(storageUrl);

            Assert.NotNull(res);
        }
    }
}
