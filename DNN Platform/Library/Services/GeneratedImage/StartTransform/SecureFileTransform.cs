// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;

    /// <summary>
    /// Secure File ImageTransform class.
    /// </summary>
    public class SecureFileTransform : ImageTransform
    {
        public SecureFileTransform()
        {
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
        }

        /// <summary>
        /// Gets provides an Unique String for the image transformation.
        /// </summary>
        public override string UniqueString => base.UniqueString + this.SecureFile.FileId;

        /// <summary>
        /// Gets or sets set IFileInfo object of given FileId.
        /// </summary>
        public IFileInfo SecureFile { get; set; }

        /// <summary>
        /// Gets or sets the Image to return if no image or error.
        /// </summary>
        public Image EmptyImage { get; set; }

        /// <summary>
        /// Processes an input image returing a secure file image.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        /// <remarks>
        /// If the secure file is not an image, it returns an image representing the file extension.
        /// </remarks>
        public override Image ProcessImage(Image image)
        {
            // if SecureFile is no ImageFile return FileType-Image instead
            if (!IsImageExtension(this.SecureFile.Extension))
            {
                return this.GetSecureFileExtensionIconImage();
            }

            using (var content = FileManager.Instance.GetFileContent(this.SecureFile))
            {
                return this.CopyImage(content);
            }
        }

        /// <summary>
        /// Checks if the current user have READ permission on a given folder.
        /// </summary>
        /// <param name="folder">Folder info object.</param>
        /// <returns>True if the user has READ permission, false otherwise.</returns>
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

        private Image GetSecureFileExtensionIconImage()
        {
            var extensionImageAbsolutePath = Globals.ApplicationMapPath + "\\" +
                       PortalSettings.Current.DefaultIconLocation.Replace("/", "\\") + "\\" +
                       "Ext" + this.SecureFile.Extension + "_32x32_Standard.png";

            if (!File.Exists(extensionImageAbsolutePath))
            {
                return this.EmptyImage;
            }

            using (var stream = new FileStream(extensionImageAbsolutePath, FileMode.Open))
            {
                return this.CopyImage(stream);
            }
        }
    }
}
