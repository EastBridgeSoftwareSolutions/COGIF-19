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
using System.Diagnostics;
using COGIF_19.AzureStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration.AzureKeyVault;

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

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(
                    azureServiceTokenProvider.KeyVaultTokenCallback));

            var config = new ConfigurationBuilder()
            .SetBasePath(executionContext.FunctionAppDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddAzureKeyVault("https://cogif-19-dev.vault.azure.net", keyVaultClient, new DefaultKeyVaultSecretManager())
            .Build();

            var localInputDir = Path.Combine(Path.GetTempPath(), "input");
            var localOutputDir = Path.Combine(Path.GetTempPath(), "output");
            if (Directory.Exists(localInputDir)) Directory.Delete(localInputDir, true);
            if (Directory.Exists(localOutputDir)) Directory.Delete(localOutputDir, true);
            Directory.CreateDirectory(localInputDir);
            Directory.CreateDirectory(localOutputDir);


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string userid = data["userid"];
            if (string.IsNullOrWhiteSpace(userid))
            {
                throw new ArgumentNullException("userid");
            }

            var blobs = new BlobStorage(config["ConnectionStrings:StorageConnection"]);
            var userContainer = blobs.GetContainer(userid);

            await userContainer.CopyToLocal(localInputDir);
            var tempWorkingFolder = SetTempWorkingFolder(localInputDir);

            var args = string.Format("-y -framerate 5 -i {0}\\%05d.png {1}\\MyTimelapse.gif", tempWorkingFolder, localOutputDir);
            ExecuteFFMpeg(log, args, tempWorkingFolder, localOutputDir, executionContext);

            await userContainer.CopyToCloud(localOutputDir, "MyTimelapse.gif");
            RemoveTempWorkingFolder(localInputDir);

            return new OkResult();
        }

        private static void RemoveTempWorkingFolder(string filePath)
        {
            Directory.Delete(Path.Combine(filePath, "TEMP"), true);
        }

        private static string SetTempWorkingFolder(string filePath)
        {
            var tempPath = Path.Combine(filePath, "TEMP");
            Directory.CreateDirectory(tempPath);
            var files = Directory.GetFiles(filePath, "*.png");
            for (int i = 0; i < files.Length; i++)
            {
                File.Copy(files[i], Path.Combine(tempPath, i.ToString("00000") + ".png"));
            }
            return Path.Combine(tempPath);
        }

        private static string ExecuteFFMpeg(ILogger log, string ffmpegArguments, string pathLocalInput, string pathLocalOutput, ExecutionContext executionContext)
        {
            string output = string.Empty;
            log.LogInformation("Encoding...");
            var file = Path.Combine(executionContext.FunctionAppDirectory, "ffmpeg", "ffmpeg.exe");

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
