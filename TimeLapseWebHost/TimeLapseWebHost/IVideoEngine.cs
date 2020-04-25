using System;
using System.Threading.Tasks;

namespace TimeLapseWebHost
{
    public interface IVideoEngine
    {
        void Create(string id);
    }
}