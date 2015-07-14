using System.IO;
using System.Web;
using ClientDependency.Core.CompositeFiles.Providers;

namespace ClientDependency.Core.CompositeFiles
{
    /// <summary>
    /// An interface defining a file writer for a local file
    /// </summary>
    public interface IFileWriter
    {
        /// <summary>
        /// writes the file to the stream and returns true if it was successful, false if not successful
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="sw"></param>
        /// <param name="fi"></param>
        /// <param name="type"></param>
        /// <param name="origUrl"></param>
        /// <param name="http"></param>
        /// <returns></returns>
        bool WriteToStream(BaseCompositeFileProcessingProvider provider, StreamWriter sw, FileInfo fi, ClientDependencyType type, string origUrl, HttpContextBase http);
    }
}
