using Microsoft.WindowsAzure.Storage.Blob;

namespace COGIF_19.AzureStorage
{
    public interface IBlobStorage
    {
        CloudBlobContainer GetContainer(string containerName);
    }
}