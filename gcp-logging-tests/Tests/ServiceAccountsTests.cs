namespace gcp_logging_tests
{
    /// Log Entries List Example
    /// https://github.com/GoogleCloudPlatform/dotnet-docs-samples/tree/master/logging/api
    /// /Users/garrettwong/Git/dotnet-docs-samples/logging/api/LoggingSample
    /// dotnet 8.0.x
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
