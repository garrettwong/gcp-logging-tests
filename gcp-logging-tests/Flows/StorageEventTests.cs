using gcp_logging_tests.API;
using Google.Cloud.Audit;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
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
                "protoPayload.serviceName=\"storage.googleapis.com\" AND protoPayload.serviceName=\"storage.objects.create\" AND " +
                " timestamp >= \"2021-07-27T2:40:00-04:00\"");

            foreach (var row in logEntries)
            {

                var cal = row.ProtoPayload.Unpack<AuditLog>();

                Assert.Equal("storage.objects.create", cal.MethodName);
                Assert.Equal("storage.googleapis.com", cal.ServiceName);
                Assert.Equal("gcp-csharp-app@gwc-core.iam.gserviceaccount.com", cal.AuthenticationInfo.PrincipalEmail);
                Assert.NotNull(cal.RequestMetadata.CallerIp);
                Assert.Equal("projects/_/buckets/gwc-sandbox/objects/superobject", cal.ResourceName);
                Assert.NotNull(cal.RequestMetadata.RequestAttributes.Time.ToString().Replace("\"", ""));

                break; // the first log element should be the one that was just logged
            }

            LoggingAPI.Test("hello");
        }

        static Random random = new Random();
        public static string GetRandomHexNumber(int digits)
        {
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X");
        }

        [Fact]
        public void DataWriteCountTest()
        {
            var projectId = "gwc-sandbox";
            var bucketName = projectId;
            var objectName = "superobject" + GetRandomHexNumber(8);
            var localFilePath = "TEMP.txt";

            // Write Data
            var storage = new Storage();
            storage.DataWrite(projectId, bucketName, objectName, localFilePath);


            // Read Log
            var logEntries = LoggingAPI.ListLogEntriesByLogQuery("gwc-sandbox",
                "logName=\"projects/gwc-sandbox/logs/cloudaudit.googleapis.com%2Fdata_access\" AND " +
                "protoPayload.serviceName=\"storage.googleapis.com\" AND protoPayload.methodName=\"storage.objects.create\" AND " +
                " timestamp >= \"2021-07-27T2:40:00-04:00\"");

            var count = 0;
            foreach (var row in logEntries)
            {
                count++;
            }

            Assert.True(count > 30);
        }
    }
}
