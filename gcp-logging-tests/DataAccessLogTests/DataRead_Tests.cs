using gcp_logging_tests.API;
using gcp_logging_tests.Utilities;
using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace gcp_logging_tests.DataAccessLogTests
{
    public class DataRead_Tests
    {
        private RandomGenerator _randomGenerator;
        private GCPLogQueryGenerator _gcpLogQueryGenerator;

        public DataRead_Tests()
        {
            Access.Initiailize();

            _randomGenerator = new RandomGenerator();
            _gcpLogQueryGenerator = new GCPLogQueryGenerator();
        }

        [Fact]
        public void DataRead_ShouldPass()
        {
            var projectId = "gwc-sandbox";
            var bucketName = projectId;
            var objectName = "superobject";

            // Write Data
            var storage = new Storage();
            var res = storage.ReadObject(projectId, bucketName, objectName);
            Assert.NotNull(res);
        }

        [Fact]
        public void DataRead_ShouldCreateLog()
        {
            var projectId = "gwc-sandbox";
            var bucketName = projectId;
            var objectName = "superobject";

            // Write Data
            var storage = new Storage();
            var res = storage.ReadObject(projectId, bucketName, objectName);

            // Check Log

            var serviceName = "storage.googleapis.com";
            var methodName = "storage.objects.get";

            // Read Log
            var logEntriesCount = LoggingAPI.ListLogEntriesByLogQuery(projectId,
                _gcpLogQueryGenerator.GetDataAccessLogQuery(projectId, serviceName, methodName, 5)
            ).Count();

            Assert.True(logEntriesCount > 0);
        }

        [Fact]
        public void DataRead_ShouldCreateOneNewLog()
        {
            var projectId = "gwc-sandbox";
            var bucketName = projectId;
            var objectName = "superobject";

            // Check Log
            var serviceName = "storage.googleapis.com";
            var methodName = "storage.objects.get";

            // Read Log
            var logEntriesBeforeCount = LoggingAPI.ListLogEntriesByLogQuery(projectId,
                _gcpLogQueryGenerator.GetDataAccessLogQuery(projectId, serviceName, methodName, 5)
            ).Count();

            // Write Data
            var storage = new Storage();
            var res = storage.ReadObject(projectId, bucketName, objectName);

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
