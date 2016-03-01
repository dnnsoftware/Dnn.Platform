using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
    /// <summary>
    /// Secure File ImageTransform class
    /// </summary>
	public class SecureFileTransform : ImageTransform
    {
        #region Properties
        /// <summary>
        /// Set IFileInfo object of given FileId
        /// </summary>
        public IFileInfo SecureFile { get; set; }

        /// <summary>
        /// Sets the Image to return if no image or error
        /// </summary>
        public Image EmptyImage { get; set; }

        /// <summary>
        /// Provides an Unique String for the image transformation
        /// </summary>
        public override string UniqueString => base.UniqueString + SecureFile.FileId;
	    #endregion 
       
        public SecureFileTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
		}

        /// <summary>
        /// Processes an input image returing a secure file image
        /// </summary>
        /// <param name="image">Input image</param>
        /// <returns>Image result after image transformation</returns>
        /// <remarks>
        /// If the secure file is not an image, it returns an image representing the file extension
        /// </remarks>
        public override Image ProcessImage(Image image)
        {
            // if SecureFile is no ImageFile return FileType-Image instead
            if (!IsImageExtension(SecureFile.Extension))
            {
                return GetSecureFileExtensionIconImage();
            }

            using (var content = FileManager.Instance.GetFileContent(SecureFile))
            {
                return CopyImage(content);
            }
		}

        private Image GetSecureFileExtensionIconImage()
        {
            var extensionImageAbsolutePath = Globals.ApplicationMapPath + "\\" +
                       PortalSettings.Current.DefaultIconLocation.Replace("/", "\\") + "\\" +
                       "Ext" + SecureFile.Extension + "_32x32_Standard.png";

            if (!File.Exists(extensionImageAbsolutePath))
            {
                return EmptyImage;
            }

            using (var stream = new FileStream(extensionImageAbsolutePath, FileMode.Open))
            {
                return CopyImage(stream);
            }
        }

        /// <summary>
        /// Checks if the current user have READ permission on a given folder
        /// </summary>
        /// <param name="folder">Folder info object</param>
        /// <returns>True if the user has READ permission, false otherwise</returns>
        public bool DoesHaveReadFolderPermission(IFolderInfo folder)
        {
            return FolderPermissionController.HasFolderPermission(folder.FolderPermissions, "Read");
        }

        private static bool IsImageExtension(string extension)
        {
            if (!extension.StartsWith("."))
            {
                extension = $".{extension}";
            }

            var imageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", ".JPEG", ".ICO" };
            return imageExtensions.Contains(extension.ToUpper());
        }
    }
}
