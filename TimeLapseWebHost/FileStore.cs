using COGIF_19.AzureStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        private readonly IWebHostEnvironment _env;
        private readonly IBlobStorage _blobStorage;
        private const string GifFileName = "MyTimelapse.gif";

        public FileStore(IWebHostEnvironment env, IBlobStorage blobStorage)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _blobStorage = blobStorage;
        }

        public async Task Create(IFormFile uploadedFile, string id)
        {

            var userBlobContainer = _blobStorage.GetContainer(id);
            await userBlobContainer.CreateIfNotExistsAsync(Microsoft.WindowsAzure.Storage.Blob.BlobContainerPublicAccessType.Off,null,null);
            var filename = DateTime.Now.Ticks + ".png";
            using (var stream = uploadedFile.OpenReadStream())
            {
                await userBlobContainer.CopyToCloud(stream, filename);
            }
        }

        public async Task<bool> UserHasGif(ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userBlobContainer = _blobStorage.GetContainer(id).GetBlobReference(GifFileName);
            try
            {
                await userBlobContainer.FetchAttributesAsync();
                return true;
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                {
                    return false;
                }

            }
            return false;
        }

        public Uri GetResourceWithSas(ClaimsPrincipal user, string resourceId)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userBlobContainer = _blobStorage.GetContainer(id).GetBlobReference(resourceId);
            var sasPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTime.Now.AddMinutes(-10),
                SharedAccessExpiryTime = DateTime.Now.AddMinutes(30)
            };

            string sasToken = userBlobContainer.GetSharedAccessSignature(sasPolicy);
            var baseUrl = "https://cogif19.blob.core.windows.net";

            return new Uri($"{baseUrl}/{id}/{resourceId}{sasToken}");
        }

    }
}
