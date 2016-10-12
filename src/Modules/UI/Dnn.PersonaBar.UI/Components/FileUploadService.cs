using System;
using System.IO;
using System.Linq;
using Dnn.PersonaBar.Library;
using DotNetNuke.Common;
using DotNetNuke.Framework;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.UI.Components
{
    public class FileUploadService : ServiceLocator<IFileUploadService, FileUploadService> , IFileUploadService
    {
        private const string ResourceFile = Constants.PersonaBarRelativePath + "App_LocalResources/FileUploadService.resx";

        private const string DefaultImagesFolder = "Images";

        private const string DefaultDocumentsFolder = "Documents";
                
        protected override Func<IFileUploadService> GetFactory()
        {
            return () => new FileUploadService();
        }

        public IFileInfo AddImage(int portalId, string fileName, Stream stream)
        {
            if (!IsImageExtension(Path.GetExtension(fileName)))
            {
                var message = Localization.GetString("InvalidImageExtension", ResourceFile);
                throw new Exception(string.Format(message, Globals.glbImageFileTypes));
            }

            var folder = GetFolder(portalId, DefaultImagesFolder);

            fileName = GetAvailableFileName(folder, fileName);

            return FileManager.Instance.AddFile(folder, fileName, stream);            
        }

        public IFileInfo AddDocument(int portalId, string fileName, Stream stream)
        {
            if (!IsDocumentExtension(Path.GetExtension(fileName)))
            {
                var message = Localization.GetString("InvalidDocumentExtension", ResourceFile);
                var extensions = string.Join(",", Constants.DefaultDocumentExtensions);
                throw new Exception(string.Format(message, extensions));
            }

            var folder = GetFolder(portalId, DefaultDocumentsFolder);

            fileName = GetAvailableFileName(folder, fileName);

            return FileManager.Instance.AddFile(folder, fileName, stream);
        }
        
        private static bool IsImageExtension(string extension)
        {
            extension = extension.Replace(".", "").ToLower();
            var extensions = Globals.glbImageFileTypes.Split(',');
            return extensions.Contains(extension);
        }

        private static bool IsDocumentExtension(string extension)
        {
            extension = extension.Replace(".", "").ToLower();
            return Constants.DefaultDocumentExtensions.Contains(extension.ToLower());
        }

        private static string GetAvailableFileName(IFolderInfo folder, string fileName)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            var i = 1;
            while (FileManager.Instance.FileExists(folder, fileName))
            {
                fileName = fileNameWithoutExtension + "-" + i + Path.GetExtension(fileName);
                i++;
            }

            return fileName;
        }

        private static IFolderInfo GetFolder(int portalId, string folderName)
        {
            var folder = FolderManager.Instance.GetFolder(portalId, folderName);
            return folder ?? FolderManager.Instance.AddFolder(portalId, folderName);
        }
    }
}