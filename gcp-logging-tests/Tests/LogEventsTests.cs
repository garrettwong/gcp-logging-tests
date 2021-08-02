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
        public LogEventsTests()
        {
            Access.Initiailize();
        }

        [Fact]
        public void WriteLogTest()
        {
            LoggingAPI.WriteLogEntry("gwc-sandbox", "hello", "world");
        }

        /// <summary>
        /// https://cloud.google.com/logging/docs/reference/v2/rest/v2/logs/list
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void ListLogEntriesTest()
        {
            // Write twice just in case
            LoggingAPI.WriteLogEntry("gwc-sandbox", "hello", "world");
            Thread.Sleep(5000);
            LoggingAPI.WriteLogEntry("gwc-sandbox", "hello", "world2");
            Thread.Sleep(5000);

            var logEntries = LoggingAPI.ListLogEntries("gwc-sandbox", "hello");

            var i = 0;
            foreach (var row in logEntries)
            {
                //Console.WriteLine($"{row.TextPayload.Trim()}");
                //Console.WriteLine(i + ":");
                //Console.WriteLine(JsonConvert.SerializeObject(row));
                i++;
            }
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
            var storageUrl = "https://storage.googleapis.com/storage/v1/b?project=gwc-sandbox";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var r = await client.GetStringAsync(storageUrl);

            Assert.NotNull(r);

            var logEntries = LoggingAPI.ListLogEntriesByLogQuery("gwc-sandbox",
                "logName=\"projects/gwc-sandbox/logs/cloudaudit.googleapis.com%2Fdata_access\" AND " +
                "protoPayload.serviceName=\"storage.googleapis.com\" AND timestamp >= \"2021-07-26T2:40:00-04:00\"");
            
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
            var storageUrl = "https://storage.googleapis.com/storage/v1/b?project=gwc-sandbox";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var r = await client.GetStringAsync(storageUrl);

            Assert.NotNull(r);
        }
    }
}
