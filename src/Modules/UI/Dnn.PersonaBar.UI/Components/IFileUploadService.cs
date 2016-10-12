using System.IO;
using DotNetNuke.Services.FileSystem;

namespace Dnn.PersonaBar.UI.Components
{ 
    public interface IFileUploadService
    {
        IFileInfo AddImage(int portalId, string fileName, Stream stream);

        IFileInfo AddDocument(int portalId, string fileName, Stream stream);
    }
}