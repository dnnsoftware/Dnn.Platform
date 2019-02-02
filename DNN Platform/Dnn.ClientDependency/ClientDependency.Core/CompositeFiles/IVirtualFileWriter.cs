using System.IO;
using System.Web;
using System.Web.Hosting;
using ClientDependency.Core.CompositeFiles.Providers;

namespace ClientDependency.Core.CompositeFiles
{
    public interface IVirtualFileWriter 
    {
        IVirtualFileProvider FileProvider { get; }

        /// <summary>
        /// writes the file to the stream and returns true if it was successful, false if not successful
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="sw"></param>
        /// <param name="vf"></param>
        /// <param name="type"></param>
        /// <param name="origUrl"></param>
        /// <param name="http"></param>
        /// <returns></returns>
        bool WriteToStream(BaseCompositeFileProcessingProvider provider, StreamWriter sw, IVirtualFile vf, ClientDependencyType type, string origUrl, HttpContextBase http);
    }
}