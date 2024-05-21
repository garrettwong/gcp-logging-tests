using gcp_logging_tests.API;
using gcp_logging_tests.Utilities;
using Google.Cloud.Audit;
using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace gcp_logging_tests.DataAccessLogTests
{
    public class DataWrite_Tests
    {
        private RandomGenerator _randomGenerator;
        private GCPLogQueryGenerator _gcpLogQueryGenerator;

        public DataWrite_Tests()
        {
            Access.Initiailize();

            _randomGenerator = new RandomGenerator();
            _gcpLogQueryGenerator = new GCPLogQueryGenerator();
        }


        [Fact]
        public void DataWrite_ShouldPass()
        {
            var projectId = Global.PROJECT_ID;
            var bucketName = projectId;
            var objectName = "superobject";
            var localFilePath = "Content/TEMP.txt";

            // Write Data
            var storage = new Storage();
            storage.CreateObject(projectId, bucketName, objectName, localFilePath);

            var d = DateTime.Now.AddHours(-2);
            var v = d.ToString("o");

            // Read Log
            var logEntries = LoggingAPI.ListLogEntriesByLogQuery(projectId,
                $"logName=\"projects/{projectId}/logs/cloudaudit.googleapis.com%2Fdata_access\" AND " +
                "protoPayload.serviceName=\"storage.googleapis.com\" AND protoPayload.serviceName=\"storage.objects.create\" AND " +
                $" timestamp >= \"{v}\"");

            foreach (var row in logEntries)
            {

                var cal = row.ProtoPayload.Unpack<AuditLog>();

                Assert.Equal("storage.objects.create", cal.MethodName);
                Assert.Equal("storage.googleapis.com", cal.ServiceName);
                Assert.Equal(Global.Application.SERVICE_ACCOUNT, cal.AuthenticationInfo.PrincipalEmail);
                Assert.NotNull(cal.RequestMetadata.CallerIp);
                Assert.Equal($"projects/_/buckets/{projectId}/objects/superobject", cal.ResourceName);
                Assert.NotNull(cal.RequestMetadata.RequestAttributes.Time.ToString().Replace("\"", ""));

                break; // the first log element should be the one that was just logged
            }

            LoggingAPI.Test("hello");
        }

        [Fact]
        public void DataWrite_ShouldCreateLog()
        {
            var projectId = Global.PROJECT_ID;
            var bucketName = projectId;
            var objectName = "superobject" + _randomGenerator.GetRandomHexNumber(8);
            var localFilePath = "Content/TEMP.txt";

            // Write Data
            var storage = new Storage();
            storage.CreateObject(projectId, bucketName, objectName, localFilePath);

            var d = DateTime.Now.AddHours(-2);
            var v = d.ToString("o");

            // sleep up to 10 seconds
            Thread.Sleep(10000);

            // Read Log
            var logEntries = LoggingAPI.ListLogEntriesByLogQuery(projectId,

                $"logName=\"projects/{projectId}/logs/cloudaudit.googleapis.com%2Fdata_access\" AND " +
                "protoPayload.serviceName=\"storage.googleapis.com\" AND protoPayload.methodName=\"storage.objects.create\" AND " +
                $" timestamp >= \"{v}\"");

            var count = 0;
            foreach (var row in logEntries)
            {
                count++;
            }

            Assert.True(count > 1);
        }

        [Fact]
        public void DataWrite_ShouldCreateOneNewLog()
        {
            var projectId = Global.PROJECT_ID;
            var bucketName = projectId;
            var objectName = "superobject" + _randomGenerator.GetRandomHexNumber(8);
            var localFilePath = "Content/TEMP.txt";

            var serviceName = "storage.googleapis.com";
            var methodName = "storage.objects.create";

            // Read Log
            var logEntriesBeforeCount = LoggingAPI.ListLogEntriesByLogQuery(projectId,
                _gcpLogQueryGenerator.GetDataAccessLogQuery(projectId, serviceName, methodName, 5)
            ).Count();

            // Write Data
            var storage = new Storage();
            storage.CreateObject(projectId, bucketName, objectName, localFilePath);

            var remainingAttempts = 3;
            var logEntriesAfterCount = 0;
            while (remainingAttempts > 0)
            {
                Thread.Sleep(5000);

                // Read Log
                logEntriesAfterCount = LoggingAPI.ListLogEntriesByLogQuery(projectId,
                    _gcpLogQueryGenerator.GetDataAccessLogQuery(projectId, serviceName, methodName, 5)
                ).Count();

                if (logEntriesBeforeCount < logEntriesAfterCount) break;

                remainingAttempts--;
            }

            Console.WriteLine(logEntriesBeforeCount.ToString() + " " + logEntriesAfterCount.ToString());

            Assert.True(logEntriesBeforeCount < logEntriesAfterCount);
        }
    }
}
