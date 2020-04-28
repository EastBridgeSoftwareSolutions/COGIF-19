using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TimeLapseWebHost
{
    public interface IFileStore
    {
        Task Create(IFormFile uploadedFile, string id);
        List<string> GetAll(string id);
        string GetFFMPEGFolder();
        string GetRelativeGifPath(ClaimsPrincipal user);
        string GetUserFolder(string id);
        bool UserHasGif(ClaimsPrincipal user);
    }
}