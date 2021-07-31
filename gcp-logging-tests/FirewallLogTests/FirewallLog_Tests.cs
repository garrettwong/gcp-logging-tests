using gcp_logging_tests.API;
using gcp_logging_tests.Utilities;
using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace gcp_logging_tests.DataAccessTests
{
    public class FirewallLog_Tests
    {
        private RandomGenerator _randomGenerator;
        private GCPLogQueryGenerator _gcpLogQueryGenerator;

        public FirewallLog_Tests()
        {
            Access.Initiailize();

            _randomGenerator = new RandomGenerator();
            _gcpLogQueryGenerator = new GCPLogQueryGenerator();

            // TODO: Enable a FW LOG
            
        }

        [Fact]
        public void FirewallLog_Test()
        {
            
        }
    }
}
