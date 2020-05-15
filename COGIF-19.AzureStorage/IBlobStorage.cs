using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace COGIF_19.AzureStorage
{
    public interface IBlobStorage
    {
        CloudBlobContainer GetContainer(string containerName);
    }
}