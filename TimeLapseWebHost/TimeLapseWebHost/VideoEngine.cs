using System;
using System.Diagnostics;
using System.IO;

namespace TimeLapseWebHost
{
    public class VideoEngine : IVideoEngine
    {
        private readonly IFileStore _fileStore;

        public VideoEngine(IFileStore fileStore)
        {
            _fileStore = fileStore;
        }

        public void Create(string id)
        {
            var filePath = _fileStore.GetUserFolder(id);
            string frameRate = "5";

            var outputPngs = SetTempWorkingFolder(filePath);
            var finalPath = Path.Combine(filePath, "MyTimelapse.gif");
            var argumentsGif = string.Format("-y -framerate {0} -i {1} {2}", frameRate, outputPngs, finalPath);

            try
            {
                Execute("ffmpeg.exe", _fileStore.GetFFMPEGFolder(), argumentsGif);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                RemoveTempWorkingFolder(filePath);
            }

            
        }

        private void RemoveTempWorkingFolder(string filePath)
        {
            Directory.Delete(Path.Combine(filePath, "TEMP"), true);
        }

        private string SetTempWorkingFolder(string filePath)
        {
            var tempPath = Path.Combine(filePath, "TEMP");
            Directory.CreateDirectory(tempPath);
            var files = Directory.GetFiles(filePath, "*.png");
            for (int i = 0; i < files.Length; i++)
            {
                File.Copy(files[i], Path.Combine(tempPath, i.ToString("00000") + ".png"));
            }
            return Path.Combine(tempPath,"%05d.png");
        }

        private static string Execute(string exeName, string exePath, string parameters)
        {
            string result = string.Empty;

            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = Path.Combine(exePath, exeName);
                p.StartInfo.WorkingDirectory = exePath;
                p.StartInfo.Arguments = parameters;
                p.Start();
                p.WaitForExit();

                result = p.StandardOutput.ReadToEnd();
            }

            return result;
        }
    }
}