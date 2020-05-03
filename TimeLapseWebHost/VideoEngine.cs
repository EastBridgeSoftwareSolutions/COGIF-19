using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TimeLapseWebHost
{
    public class VideoEngine : IVideoEngine
    {
        private readonly IFileStore _fileStore;

        public VideoEngine(IFileStore fileStore)
        {
            _fileStore = fileStore;
        }

        public async Task Create(string id)
        {
            //string frameRate = "5";
            await PostRequest(id);
        }

        public static async Task<string> PostRequest(string text)
        {
            using (var client = new HttpClient())
            {

                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("userid", text);

                string json = JsonConvert.SerializeObject(dictionary);
                var requestData = new StringContent(json, Encoding.UTF8, "application/json");
#if DEBUG
                var baseUrl = "http://localhost:7071";
#else
                var baseUrl = "https://cogif19encoder.azurewebsites.net";
#endif
                var response = await client.PostAsync(string.Format("{0}/api/VideoEncoder?code=65OlFmVuDVP4P/w5hY4xDDaaBT0PiraXdb7RmVSiblxZHXJ2C4IASQ==", baseUrl), requestData);
                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }




    }
}