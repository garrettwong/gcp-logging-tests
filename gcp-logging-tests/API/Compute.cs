using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System.Threading.Tasks;
using Compute = Google.Apis.Compute;

namespace gcp_logging_tests.API
{
    public class Compute
    {

        private Compute::v1.ComputeService _computeService;

        /// <summary>
        /// Constructor
        /// </summary>
        public Compute()
        {

            GoogleCredential credential = GoogleCredential.GetApplicationDefault();
            credential = GoogleCredential.GetApplicationDefault();
            if (credential.IsCreateScopedRequired)
            {
                credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
            }

            _computeService = new Compute::v1.ComputeService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google-ComputeSample/0.1",
            });
        }

        public void Dispose()
        {
            if (_computeService != null) _computeService.Dispose();
        }

        public Google.Apis.Compute.v1.Data.InstanceList GetInstances(string projectId, string zone)
        {
            var request = _computeService.Instances.List(projectId, zone);
            var list = request.Execute();
            return list;
        }

        public Google.Apis.Compute.v1.Data.Instance GetInstance(string projectId, string zone, string name)
        {
            var request = _computeService.Instances.Get(projectId, zone, name);
            var instance = request.Execute();
            return instance;
        }
    }
}
