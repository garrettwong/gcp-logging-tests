namespace gcp_logging_tests
{
    /// <summary>
    /// Global configuration settings
    /// </summary>
    public class Global
    {
        public static readonly string PROJECT_ID = "gwc-sandbox";

        public static class Application
        {
            public const string APP_NAME = "gcp-logging-tests";
            public const string SERVICE_ACCOUNT = "gcp-csharp-app@gwc-core.iam.gserviceaccount.com";

            public const string PATH_TO_SA_KEY = "/Users/garrettwong/Downloads/sa-key.json";
            public const string PATH_TO_SA_KEY_WINDOWS = @"C:\Users\garre\Downloads\sa-key.json";
        }

        public static class Scopes
        {
            public const string CLOUD_PLATFORM = "https://www.googleapis.com/auth/cloud-platform";
        }
    }
}
