using Google.Apis.Auth.OAuth2;
using Google.Apis.Compute.v1.Data;
using Google.Apis.Iam.v1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCS = global::Google.Cloud.Storage;

namespace gcp_logging_tests.API
{
    public class ServiceAccounts : IDisposable
    {
        private IamService _service;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ServiceAccounts()
        {
            var credential = GoogleCredential.GetApplicationDefault()
            .CreateScoped(IamService.Scope.CloudPlatform);

            _service = new IamService(new IamService.Initializer
            {
                HttpClientInitializer = credential
            });
        }

        public List<string>  GetServiceAccounts(string projectId) { 
            var response = _service.Projects.ServiceAccounts.List(
                "projects/" + projectId).Execute();

            var serviceAccounts = response.Accounts.Select(a => a.Email).ToList();
            
            return serviceAccounts;
        }
    }
}
