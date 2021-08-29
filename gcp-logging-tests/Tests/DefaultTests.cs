using gcp_logging_tests.Utilities;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace gcp_logging_tests
{
    public class DefaultTests
    {
        private readonly string _projectId;
        public DefaultTests()
        {
            _projectId = Global.PROJECT_ID;

            Access.Initiailize();
        }

        [Fact] // 13:14 minutes
        public void GenerateSSNTest()
        {
            var ssnGen = new SSNGenerator();
            var list = new List<string>();
            
            ssnGen.GetSSNs(list, "");
            Console.Write("HI");
            //Assert.Equal(1000000000, list.Count);
            Assert.Equal(10000000, list.Count);
        }

        [Fact] // 18 minutes
        public void GenerateSSNUsingSbTest()
        {
            var ssnGen = new SSNGenerator();
            var list = new List<string>();
            var sb = new StringBuilder();
            ssnGen.GetSSNs(list, sb);
            Console.Write("HI");
            //Assert.Equal(1000000000, list.Count);
            Assert.Equal(10000000, list.Count);
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
            var functionUrl = $"https://us-central1-{_projectId}.cloudfunctions.net/dotnet-time-function";

            var token = await GetBearerToken(functionUrl);

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await client.GetStringAsync(functionUrl);

            Assert.NotNull(res);
        }


        [Fact]
        public async Task GetIamCredentials()
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
            Assert.NotNull(accessToken);
        }

        [Fact]
        public async Task MultiApiFlow()
        {
            var token = await GetAccessToken();

            // API Client Lib Call
            var gc = GoogleCredential.GetApplicationDefault();
            var sc = StorageClient.Create(gc);
            var projectId = Global.PROJECT_ID;
            var buckets = sc.ListBuckets(projectId);

            // Get Buckets API Call
            var storageUrl = $"https://storage.googleapis.com/storage/v1/b?project={projectId}";

            using var client2 = new HttpClient();
            client2.Timeout = TimeSpan.FromSeconds(10);
            client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var r = await client2.GetStringAsync(storageUrl);

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
        /// Lists buckets using the Client Library
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ClientLibBucketsList()
        {
            var token = await GetAccessToken();

            // API Client Lib Call
            var gc = GoogleCredential.GetApplicationDefault();
            var sc = StorageClient.Create(gc);
            var projectId = Global.PROJECT_ID;
            var buckets = sc.ListBuckets(projectId);
            // foreach (var bucket in buckets)
            // {
            //     Console.WriteLine(bucket.Name);
            // }
        }

        /// <summary>
        /// Lists buckets using an API call
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BucketsList()
        {
            var token = await GetAccessToken();
            var projectId = Global.PROJECT_ID;

            // Get Buckets API Call
            var storageUrl = $"https://storage.googleapis.com/storage/v1/b?project={projectId}";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await client.GetStringAsync(storageUrl);

            Assert.NotNull(res);
        }

        /// <summary>
        /// https://cloud.google.com/logging/docs/reference/v2/rest/v2/logs/list
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LogsList()
        {
            var token = await GetAccessToken();
            var url = $"https://logging.googleapis.com/v2/projects/{_projectId}/logs";
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await client.GetStringAsync(url);

            Assert.NotNull(res);
        }
    }
}
