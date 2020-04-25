using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace TimeLapseWebHost
{
    public interface IFileStore
    {
        Task Create(IFormFile uploadedFile, string id);
        List<string> GetAll(string id);
        string GetUserFolder(string id);
        string GetFFMPEGFolder();
            }
}