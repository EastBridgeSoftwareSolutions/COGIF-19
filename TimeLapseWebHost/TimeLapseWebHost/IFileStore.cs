using Microsoft.AspNetCore.Http;
using System.Security.Principal;
using System.Threading.Tasks;

namespace TimeLapseWebHost
{
    public interface IFileStore
    {
        Task Create(IFormFile uploadedFile, string id);
    }
}