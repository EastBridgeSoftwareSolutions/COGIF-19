using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading.Tasks;

namespace COGIF_19.AzureStorage
{
    public class BlobStorage : IBlobStorage
    {
        readonly CloudBlobClient blobClient;

        public BlobStorage(string connectionstring)
        {
            var storageAccount =
                CloudStorageAccount.Parse(connectionstring);
            blobClient = storageAccount.CreateCloudBlobClient();
        }

        public CloudBlobContainer GetContainer(string containerName)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            return container;
        }
    }
}
