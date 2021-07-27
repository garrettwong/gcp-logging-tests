using System;
using Xunit;

namespace gcp_logging_tests.Flows
{
    public class EmptyClassTests
    {
        public EmptyClassTests()
        {
            Access.Initiailize();
        }


        [Fact]
        public void ApplicationDefaultCredentails()
        {
            var credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

            Assert.NotNull(credentialsPath);
        }

    }
}
