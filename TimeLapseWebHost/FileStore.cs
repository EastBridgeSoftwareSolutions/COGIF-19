using COGIF_19.AzureStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace TimeLapseWebHost
{
    public class FileStore : IFileStore
    {
        private readonly IWebHostEnvironment _env;
        private readonly IBlobStorage _blobStorage;
        private readonly string imagesRoot = "Images";
        private const string GifFileName = "MyTimelapse.gif";

        public FileStore(IWebHostEnvironment env, IBlobStorage blobStorage)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _blobStorage = blobStorage;
        }

        public async Task Create(IFormFile uploadedFile, string id)
        {
            
            var userBlobContainer = _blobStorage.GetContainer(id);
            await userBlobContainer.CreateIfNotExistsAsync();
            var filename = DateTime.Now.Ticks + ".png";
            using (var stream = uploadedFile.OpenReadStream())
            {
                await userBlobContainer.CopyToCloud(stream,filename);
            }
        }

        public List<string> GetAll(string id)
        {
            string userpath = GetUserFolder(id);
            return Directory.GetFiles(userpath).ToList();
        }

        public string GetUserFolder(string id)
        {
            return Path.Combine(_env.ContentRootPath, imagesRoot, id);
        }

        public string GetFFMPEGFolder()
        {
            return Path.Combine(_env.ContentRootPath, "FFMPEG");
        }

        public string GetRelativeGifPath(ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Path.Combine("\\", imagesRoot, id, GifFileName);
        }

        public bool UserHasGif(ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userFolder = GetUserFolder(id);
            var gifPath = Path.Combine(userFolder, GifFileName);
            return File.Exists(gifPath);
        }

    }
}
