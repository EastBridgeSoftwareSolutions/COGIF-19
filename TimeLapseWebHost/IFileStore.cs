using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TimeLapseWebHost
{
    public interface IFileStore
    {
        Task Create(IFormFile uploadedFile, string id);
        Task<bool> UserHasGif(ClaimsPrincipal user);
    }
}