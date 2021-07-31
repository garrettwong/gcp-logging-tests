using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace gcp_logging_tests
{
    public class DefaultTests
    {
        public DefaultTests()
        {
            Access.Initiailize();
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

        public async Task<string> GetAccessToken()
        {
            var adc = await GoogleCredential
                .GetApplicationDefaultAsync();
            var gc = adc.CreateScoped(new string[] { "https://www.googleapis.com/auth/cloud-platform" });

            var token = await gc.UnderlyingCredential.GetAccessTokenForRequestAsync();

            return token;
        }

        [Fact]
        public void ApplicationDefaultCredentails()
        {
            var credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

            Assert.NotNull(credentialsPath);
        }

        [Fact]
        public async Task FunctionsCall()
        {
            var functionUrl = "https://us-central1-gwc-sandbox.cloudfunctions.net/dotnet-time-function";

            var token = await GetBearerToken(functionUrl);

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var r = await client.GetStringAsync(functionUrl);

            Assert.NotNull(r);
        }


        [Fact]
        public async Task MultiApiFlow()
        {
            var token = await GetAccessToken();

            // generate at
            var url = "https://iamcredentials.googleapis.com/v1/projects/-/serviceAccounts/csharp-on-gcp%40gwc-service-accounts.iam.gserviceaccount.com:generateAccessToken";

            // IAM Creds
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

            var scopes = new
            {
                scope = new string[] { "https://www.googleapis.com/auth/cloud-platform" }
            };
            var json = JsonConvert.SerializeObject(scopes);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var r = await client.PostAsync(url, data);

            var s = r.Content.ReadAsStringAsync();
            var o = JsonConvert.DeserializeObject<dynamic>(s.Result);
            var accessToken = o.accessToken.ToString();


            // API Client Lib Call
            var gc = GoogleCredential.GetApplicationDefault();
            var sc = StorageClient.Create(gc);
            var projectId = "gwc-sandbox";
            var buckets = sc.ListBuckets(projectId);
            foreach (var bucket in buckets)
            {
                Console.WriteLine(bucket.Name);
            }


            // Get Buckets API Call
            var storageUrl = "https://storage.googleapis.com/storage/v1/b?project=gwc-sandbox";

            using var client2 = new HttpClient();
            client2.Timeout = TimeSpan.FromSeconds(10);
            client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var r2 = await client2.GetStringAsync(storageUrl);

            Console.WriteLine(r);
            Assert.NotNull(r);

        }

        /// <summary>
        /// https://cloud.google.com/iam/docs/reference/credentials/rest/v1/projects.serviceAccounts/generateAccessToken
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task IAMCredentialsGet()
        {
            var token = await GetAccessToken();

            // generate at
            var url = "https://iamcredentials.googleapis.com/v1/projects/-/serviceAccounts/csharp-on-gcp%40gwc-service-accounts.iam.gserviceaccount.com:generateAccessToken";

            // IAM Creds
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json")); // Accept Header

            var scopes = new
            {
                scope = new string[] { "https://www.googleapis.com/auth/cloud-platform" }
            };
            var json = JsonConvert.SerializeObject(scopes);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var r = await client.PostAsync(url, data);

            var s = r.Content.ReadAsStringAsync();
            var o = JsonConvert.DeserializeObject<dynamic>(s.Result);
            var accessToken = o.accessToken.ToString();

            Assert.NotNull(accessToken);
        }

        /// <summary>
        /// https://cloud.google.com/storage/docs/json_api/v1/buckets/list?apix_params=%7B%22project%22%3A%22gwc-sandbox%22%7D
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ClientLibBucketsList()
        {
            var token = await GetAccessToken();

            // API Client Lib Call
            var gc = GoogleCredential.GetApplicationDefault();
            var sc = StorageClient.Create(gc);
            var projectId = "gwc-sandbox";
            var buckets = sc.ListBuckets(projectId);
            foreach (var bucket in buckets)
            {
                Console.WriteLine(bucket.Name);
            }
        }

        /// <summary>
        /// https://cloud.google.com/storage/docs/json_api/v1/buckets/list?apix_params=%7B%22project%22%3A%22gwc-sandbox%22%7D
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BucketsList()
        {
            var token = await GetAccessToken();

            // Get Buckets API Call
            var storageUrl = "https://storage.googleapis.com/storage/v1/b?project=gwc-sandbox";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var r = await client.GetStringAsync(storageUrl);

            Console.WriteLine(r);
            Assert.NotNull(r);
        }

        /// <summary>
        /// https://cloud.google.com/logging/docs/reference/v2/rest/v2/logs/list
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LogsList()
        {
            var token = await GetAccessToken();
            var url = "https://logging.googleapis.com/v2/projects/gwc-sandbox/logs";
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var r = await client.GetStringAsync(url);

            Console.WriteLine(r);
            Assert.NotNull(r);
        }
    }
}
