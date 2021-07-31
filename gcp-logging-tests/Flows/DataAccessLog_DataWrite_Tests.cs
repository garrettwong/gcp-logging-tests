using gcp_logging_tests.API;
using gcp_logging_tests.Utilities;
using Google.Cloud.Audit;
using System.Linq;
using System.Threading;
using Xunit;

namespace gcp_logging_tests.Flows
{
    public class DataAccessLog_DataWrite_Tests
    {
        private RandomGenerator _randomGenerator;
        private GCPLogQueryGenerator _gcpLogQueryGenerator;

        public DataAccessLog_DataWrite_Tests()
        {
            Access.Initiailize();

            _randomGenerator = new RandomGenerator();
            _gcpLogQueryGenerator = new GCPLogQueryGenerator();
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
            storage.CreateObject(projectId, bucketName, objectName, localFilePath);


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

        [Fact]
        public void DataWriteCountTest()
        {
            var projectId = "gwc-sandbox";
            var bucketName = projectId;
            var objectName = "superobject" + _randomGenerator.GetRandomHexNumber(8);
            var localFilePath = "TEMP.txt";

            // Write Data
            var storage = new Storage();
            storage.CreateObject(projectId, bucketName, objectName, localFilePath);


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

        [Fact]
        public void DataWriteShouldIncrementCountBy1Test()
        {
            var projectId = "gwc-sandbox";
            var bucketName = projectId;
            var objectName = "superobject" + _randomGenerator.GetRandomHexNumber(8);
            var localFilePath = "TEMP.txt";

            var serviceName = "storage.googleapis.com";
            var methodName = "storage.objects.create";

            // Read Log
            var logEntriesCount = LoggingAPI.ListLogEntriesByLogQuery(projectId,
                _gcpLogQueryGenerator.GetDataAccessLogQuery(projectId, serviceName, methodName, 5)
            ).Count();

            // Write Data
            var storage = new Storage();
            storage.CreateObject(projectId, bucketName, objectName, localFilePath);

            Thread.Sleep(6000);

            var logEntriesCountNew = LoggingAPI.ListLogEntriesByLogQuery(projectId,
                _gcpLogQueryGenerator.GetDataAccessLogQuery(projectId, serviceName, methodName, 5)
            ).Count();

            Assert.Equal(logEntriesCount+1, logEntriesCountNew);
        }
    }
}
