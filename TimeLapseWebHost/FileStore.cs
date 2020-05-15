using COGIF_19.AzureStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace TimeLapseWebHost
{
    public class FileStore : IFileStore
    {
        private readonly IBlobStorage _blobStorage;
        private const string GifFileName = "MyTimelapse.gif";

        public FileStore(IConfiguration env, IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task Create(IFormFile uploadedFile, string id)
        {

            var userBlobContainer = _blobStorage.GetContainer(id);
            await userBlobContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, null, null);
            var filename = DateTime.Now.Ticks + ".png";
            using (var stream = uploadedFile.OpenReadStream())
            {
                await userBlobContainer.CopyToCloud(stream, filename);
            }
        }

        public Task<bool> UserHasGif(ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userBlobContainer = _blobStorage.GetContainer(id).GetBlobReference(GifFileName);
            return userBlobContainer.ExistsAsync();
        }

        public Task DeleteContainer(ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return _blobStorage.GetContainer(id).DeleteFiles();
        }

        public Uri GetResourceWithSas(ClaimsPrincipal user, string resourceId)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userBlobContainer = _blobStorage.GetContainer(id).GetBlobReference(resourceId);
            //todo? DI?
            var sasPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTime.Now.AddMinutes(-10),
                SharedAccessExpiryTime = DateTime.Now.AddMinutes(30)
            };

            string sasToken = userBlobContainer.GetSharedAccessSignature(sasPolicy);
            return new Uri($"{userBlobContainer.Uri}{sasToken}");
        }

    }
}
