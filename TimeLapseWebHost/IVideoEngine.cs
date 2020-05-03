using System;
using System.Threading.Tasks;

namespace TimeLapseWebHost
{
    public interface IVideoEngine
    {
        Task Create(string id);
    }
}