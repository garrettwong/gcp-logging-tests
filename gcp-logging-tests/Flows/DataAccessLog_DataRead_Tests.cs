using gcp_logging_tests.API;
using gcp_logging_tests.Utilities;
using Google.Cloud.Audit;
using System.Linq;
using System.Threading;
using Xunit;

namespace gcp_logging_tests.Flows
{
    public class DataAccessLog_DataRead_Tests
    {
        private RandomGenerator _randomGenerator;
        private GCPLogQueryGenerator _gcpLogQueryGenerator;

        public DataAccessLog_DataRead_Tests()
        {
            Access.Initiailize();

            _randomGenerator = new RandomGenerator();
            _gcpLogQueryGenerator = new GCPLogQueryGenerator();
        }



        [Fact]
        public void DataAccessLog_DataRead_Test()
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
        public void DataAccessLog_DataRead_CreatesLog_Test()
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
        public void DataAccessLog_DataRead_ShouldCreateOneNewLog_Test()
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

            Thread.Sleep(8000);

            // Read Log
            var logEntriesAfterCount = LoggingAPI.ListLogEntriesByLogQuery(projectId,
                _gcpLogQueryGenerator.GetDataAccessLogQuery(projectId, serviceName, methodName, 5)
            ).Count();

            Assert.Equal(logEntriesBeforeCount  + 1, logEntriesAfterCount);
        }

    }
}
