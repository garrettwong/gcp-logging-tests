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
    public class ServiceAccountsTests
    {
        private readonly string _projectId;

        public ServiceAccountsTests()
        {
            _projectId = Global.PROJECT_ID;

            Access.Initiailize();
        }
    }
}
