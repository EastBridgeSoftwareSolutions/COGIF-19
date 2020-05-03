using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace COGIF_19.AzureStorage
{
    public class BlobStorage : IBlobStorage
    {
        CloudBlobClient blobClient;

        public BlobStorage()
        {
            var storageAccount =
                CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=cogif19;AccountKey=h5Wjo0DFfcumYLxa0qBKeIEj7Pe1ZrSytL8DXi7KQp71ViWfWyIt3opsacMWL1lXlEo+ix1Y8A/wVO6zZxtMgw==;EndpointSuffix=core.windows.net");
            blobClient = storageAccount.CreateCloudBlobClient();

        }

        public CloudBlobContainer GetContainer(string containerName)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            return container;
        }
    }
}
