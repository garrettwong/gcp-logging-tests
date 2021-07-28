using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCS = global::Google.Cloud.Storage;

namespace gcp_logging_tests.API
{
    public class Storage : IDisposable
    {
        private GCS::V1.StorageClient _storageClient;

        /// <summary>
        /// Constructor
        /// </summary>
        public Storage()
        {
            _storageClient = GCS::V1.StorageClient.Create();
        }

        public void Dispose()
        {
            if (_storageClient != null) _storageClient.Dispose();
        }


        public object DataWrite(string projectId, string bucketName, string objectName, string localFilePath)
        {
            try
            {
                bucketName = bucketName.Replace("gs://", "");

                var contentType = "";
                // var source = (new ChaosStream ());
                var options = new GCS.V1.UploadObjectOptions();

                using (var fs = File.Open(localFilePath, FileMode.Open))
                {
                    var gcsObj = _storageClient.UploadObject(bucketName, objectName, contentType, fs, options);
                    return gcsObj;
                }
            }
            catch (Google.GoogleApiException e)
          when (e.Error.Code == 409)
            {
                // The bucket already exists.  That's fine.
                Console.WriteLine(e.Error.Message);
            }

            return null;
        }
    }
}
