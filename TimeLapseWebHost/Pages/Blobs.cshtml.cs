using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TimeLapseWebHost.Pages
{
    public class BlobsModel : PageModel
    {
        private readonly IConfiguration _config;

        public BlobsModel(IConfiguration config)
        {
            _config = config;
        }

        public CloudBlobContainer Container { get; set; }
        public bool Success { get; private set; }

        public void OnGet()
        {
            Container = GetCloudBlobContainer();
            Success = Container.CreateIfNotExistsAsync().Result;
        }

        private CloudBlobContainer GetCloudBlobContainer()
        {
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(_config["ConnectionStrings:StorageConnection"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("test-blob-container");
            return container;
        }
    }
}