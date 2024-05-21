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
            var gc = adc.CreateScoped(new string[] { Global.Scopes.CLOUD_PLATFORM });

            var token = await gc.UnderlyingCredential.GetAccessTokenForRequestAsync();

            return token;
        }

        public static void Initiailize()
        {
            if (Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS") == null)
            {
                if (File.Exists(Global.Application.PATH_TO_SA_KEY))
                {
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Global.Application.PATH_TO_SA_KEY);
                }
                else
                {
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Global.Application.PATH_TO_SA_KEY_WINDOWS);
                }
            }
        }
    }

}
