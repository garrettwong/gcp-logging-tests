using gcp_logging_tests.API;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Xunit;

namespace gcp_logging_tests.Flows
{
    public class StorageEventTests
    {
        public StorageEventTests()
        {
            Access.Initiailize();
        }


        [Fact]
        public void DataWriteTest()
        {
            var projectId = "gwc-sandbox";
            var bucketName = projectId;
            var objectName = "superobject";
            var localFilePath = "TEMP.txt";

            // Write Data
            var storage = new Storage();
            storage.DataWrite(projectId, bucketName, objectName, localFilePath);


            // Read Log
            var logEntries = LoggingAPI.ListLogEntriesByLogQuery("gwc-sandbox",
                "logName=\"projects/gwc-sandbox/logs/cloudaudit.googleapis.com%2Fdata_access\" AND " +
                "protoPayload.serviceName=\"storage.googleapis.com\" AND timestamp >= \"2021-07-27T2:40:00-04:00\"");

            foreach (var row in logEntries)
            {
                var log = JsonConvert.SerializeObject(row);

                //var protoPayload = ByteString.CopyFrom(row.ProtoPayload, Encoding.Unicode);
                var jsonLog = JsonConvert.DeserializeObject(log);
                
                var jo = JObject.Parse(log);

                JToken acme = jo.SelectToken("$.protoPayload.authenticationInfo.principalEmail");
                //var s = acme.ToString();

                Assert.Equal("gcs_bucket", jo.SelectToken("$.Resource.Type").ToString());

                break; // the first log element should be the one that was just logged
            }
        }
    }
}
