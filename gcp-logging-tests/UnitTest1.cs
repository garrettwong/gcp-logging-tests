using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
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



        public async Task<string> GetBearerToken(string aud)
        {
            // get bearer token
            var oidcToken = await GoogleCredential
                .GetApplicationDefault()
                .GetOidcTokenAsync(OidcTokenOptions.FromTargetAudience(aud));
            var token = await oidcToken.GetAccessTokenAsync();
            return token;
        }

        [Fact]
        public async Task TestApiCall()
        {
            var functionUrl = "https://us-central1-gwc-sandbox.cloudfunctions.net/dotnet-time-function";

            var token = await GetBearerToken(functionUrl);

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var r = await client.GetStringAsync(functionUrl);

            Assert.NotNull(r);

        }

    }
}
