﻿using System;
using Xunit;

namespace gcp_logging_tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.True(1 == 1);
        }


        [Fact]
        public void ApplicationDefaultCredentails()
        {
            var credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

            Assert.NotNull(credentialsPath);
        }


    }
}
