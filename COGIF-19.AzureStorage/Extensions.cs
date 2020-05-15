using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace COGIF_19.AzureStorage
{
    public static class Extensions
    {
        public static async Task CopyToLocal(this CloudBlobContainer blobContainer, string localDir)
        {
            var blobSegments = await blobContainer.ListBlobsSegmentedAsync(null);
            foreach (var segment in blobSegments.Results)
            {
                var blob = (CloudBlockBlob)segment;
                var filepath = Path.Combine(localDir, blob.Name);
                using (FileStream fs = File.Create(filepath))
                {
                    await blob.DownloadToStreamAsync(fs);
                }
            }
        }

        public static async Task DeleteFiles(this CloudBlobContainer blobContainer)
        {
            var blobSegments = await blobContainer.ListBlobsSegmentedAsync(null);
            foreach (var segment in blobSegments.Results)
            {
                var blob = (CloudBlockBlob)segment;
                _ = blob.DeleteAsync();
            }
        }

        public static async Task CopyToCloud(this CloudBlobContainer blobContainer, string localOutputDir, string filename)
        {
            var outputBlob = blobContainer.GetBlockBlobReference(filename);

            using (var fileStream = File.OpenRead(Path.Combine(localOutputDir, filename)))
            {
                await outputBlob.UploadFromStreamAsync(fileStream);
            }
        }

        public static async Task CopyToCloud(this CloudBlobContainer blobContainer, Stream stream, string filename)
        {
            var outputBlob = blobContainer.GetBlockBlobReference(filename);
            await outputBlob.UploadFromStreamAsync(stream);
        }
    }
}
