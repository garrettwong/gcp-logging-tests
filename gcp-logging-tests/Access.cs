using Google.Apis.Auth.OAuth2;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gcp_logging_tests
{
    public class Access
    {
        public static async Task<string> GetAccessToken()
        {
            var adc = await GoogleCredential
                .GetApplicationDefaultAsync();
            var gc = adc.CreateScoped(new string[] { "https://www.googleapis.com/auth/cloud-platform" });

            var token = await gc.UnderlyingCredential.GetAccessTokenForRequestAsync();

            return token;
        }

        public static void Initiailize()
        {
            if (Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS") == null)
            {
                if (File.Exists("/Users/garrettwong/Downloads/sa-key.json"))
                {
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "/Users/garrettwong/Downloads/sa-key.json");
                }
                else
                {
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"C:\Users\garre\Downloads\sa-key.json");
                }
                
            }
        }
    }

}
