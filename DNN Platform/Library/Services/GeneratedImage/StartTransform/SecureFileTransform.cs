using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
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

        public override string UniqueString
		{
            get{ return base.UniqueString + this.SecureFile.FileId.ToString() ;}
		}

        #endregion 
       
        public SecureFileTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
		}

		public override Image ProcessImage(Image image)
		{
		    // if SecureFile is no ImageFile return FileType-Image instead
            if (!IsImageExtension(SecureFile.Extension))
		    {
		        string replaceFile = Globals.ApplicationMapPath +"\\" + 
                    PortalSettings.Current.DefaultIconLocation.Replace("/","\\") + "\\"+
                    "Ext" + SecureFile.Extension + "_32x32_Standard.png";

                if(File.Exists(replaceFile))
                    return new Bitmap(replaceFile);

		        return EmptyImage;
		    }

            IFolderInfo folder = FolderManager.Instance.GetFolder(SecureFile.FolderId);
            FolderPermissionCollection folderPermissions = folder.FolderPermissions;

            if (FileManager.Instance.FileExists(folder, SecureFile.FileName) &&
                FolderPermissionController.HasFolderPermission(folderPermissions, "Read"))
                return new Bitmap(folder.PhysicalPath + SecureFile.FileName);

		    return EmptyImage;
		}

        private bool IsImageExtension(string extension)
        {
            if (!extension.StartsWith("."))
            {
                extension = string.Format(".{0}", extension);
            }

            List<string> imageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", ".JPEG", ".ICO" };
            return imageExtensions.Contains(extension.ToUpper());
        }
        
    }
}
