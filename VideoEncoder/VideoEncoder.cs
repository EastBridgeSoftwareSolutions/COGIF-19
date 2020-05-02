using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Diagnostics;

namespace VideoEncoder
{
    public static class VideoEncoder
    {
        [FunctionName("VideoEncoder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext executionContext)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var localInputDir = Path.Combine(Path.GetTempPath(), "input");
            var localOutputDir = Path.Combine(Path.GetTempPath(), "output");
            if (Directory.Exists(localInputDir)) Directory.Delete(localInputDir, true);
            if (Directory.Exists(localOutputDir)) Directory.Delete(localOutputDir, true);
            Directory.CreateDirectory(localInputDir);
            Directory.CreateDirectory(localOutputDir);


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            CloudBlobContainer inputContainer = GetCloudBlobContainer("input");
            await CopyToLocal(log, localInputDir, inputContainer);

            var args = string.Format("-y -framerate 5 -i {0}\\%05d.png {1}\\MyTimelapse.gif", localInputDir, localOutputDir);
            var result = ExecuteFFMpeg(log, args, localInputDir, localOutputDir, executionContext);

            CopyToCloud(localOutputDir, "MyTimelapse.gif");

            return new OkResult();
        }

        private static void CopyToCloud(string localOutputDir, string filename)
        {
            CloudBlobContainer outputContainer = GetCloudBlobContainer("output");
            CloudBlockBlob outputBlob = outputContainer.GetBlockBlobReference(filename);

            using (var fileStream = File.OpenRead(Path.Combine(localOutputDir, filename)))
            {
                outputBlob.UploadFromStreamAsync(fileStream).Wait();
            }
        }

        private static async Task CopyToLocal(ILogger log, string inputDir, CloudBlobContainer blobContainer)
        {
            foreach (var item in blobContainer.ListBlobsSegmentedAsync(null).Result.Results)
            {
                var blob = (CloudBlockBlob)item;
                var filepath = Path.Combine(inputDir, blob.Name);
                using (FileStream fs = File.Create(filepath))
                {
                    try
                    {
                        await blob.DownloadToStreamAsync(fs);
                    }
                    catch (Exception ex)
                    {
                        log.LogError("There was a problem downloading input file from blob. " + ex.ToString());
                    }
                }
            }
        }

        private static CloudBlobContainer GetCloudBlobContainer(string containerName)
        {
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=cogif19;AccountKey=h5Wjo0DFfcumYLxa0qBKeIEj7Pe1ZrSytL8DXi7KQp71ViWfWyIt3opsacMWL1lXlEo+ix1Y8A/wVO6zZxtMgw==;EndpointSuffix=core.windows.net");
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            return container;
        }

        private static string ExecuteFFMpeg(ILogger log, string ffmpegArguments, string pathLocalInput, string pathLocalOutput, ExecutionContext executionContext)
        {
            string output = string.Empty;
            log.LogInformation("Encoding...");
            var file = Path.Combine(executionContext.FunctionAppDirectory, "ffmpeg","ffmpeg.exe");

            var process = new Process();
            process.StartInfo.FileName = file;

            process.StartInfo.Arguments = (ffmpegArguments ?? " -i {input} {output} -y")
                .Replace("{input}", "\"" + pathLocalInput + "\"")
                .Replace("{output}", "\"" + pathLocalOutput + "\"")
                .Replace("'", "\"");

            log.LogInformation(process.StartInfo.Arguments);

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.OutputDataReceived += new DataReceivedEventHandler(
                (s, e) =>
                {
                    log.LogInformation("O: " + e.Data);
                }
            );
            process.ErrorDataReceived += new DataReceivedEventHandler(
                (s, e) =>
                {
                    log.LogInformation("E: " + e.Data);
                }
            );
            //start process
            process.Start();
            log.LogInformation("process started");
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            int exitCode = process.ExitCode;
            log.LogInformation("Video Converted");

            return output;

        }
    }
}
