using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TimeLapseWebHost
{
    public static class AzureFunction
    {
        public static void Run()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://cogif19encoder.azurewebsites.net/ffmpeg-encoding");
            req.Method = "POST";
            req.ContentType = "application/json";
            using (Stream stream = req.GetRequestStream())
            {
                string json = "{\"name\": \"Azure\" }";
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                stream.Write(buffer, 0, buffer.Length);
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            };
        }
    }
}
