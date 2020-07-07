// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;

    /// <summary>
    /// User Profile Picture ImageTransform class.
    /// </summary>
    public class UserProfilePicTransform : ImageTransform
    {
        public UserProfilePicTransform()
        {
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
        }

        /// <summary>
        /// Gets provides an Unique String for the image transformation.
        /// </summary>
        public override string UniqueString => base.UniqueString + this.UserID;

        /// <summary>
        /// Gets a value indicating whether is reusable.
        /// </summary>
        public bool IsReusable => false;

        /// <summary>
        /// Gets or sets the UserID of the profile pic.
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// Processes an input image returning the user profile picture.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        public override Image ProcessImage(Image image)
        {
            IFileInfo photoFile;

            if (this.TryGetPhotoFile(out photoFile))
            {
                if (!IsImageExtension(photoFile.Extension))
                {
                    return this.GetNoAvatarImage();
                }

                using (var content = FileManager.Instance.GetFileContent(photoFile))
                {
                    return this.CopyImage(content);
                }
            }

            return this.GetNoAvatarImage();
        }

        /// <summary>
        /// Get the Bitmap of the No Avatar Image.
        /// </summary>
        /// <returns></returns>
        public Bitmap GetNoAvatarImage()
        {
            var avatarAbsolutePath = Globals.ApplicationMapPath + @"\images\no_avatar.gif";
            using (var content = File.OpenRead(avatarAbsolutePath))
            {
                return this.CopyImage(content);
            }
        }

        /// <summary>
        /// whether current user has permission to view target user's photo.
        /// </summary>
        /// <param name="photoFile"></param>
        /// <returns></returns>
        public bool TryGetPhotoFile(out IFileInfo photoFile)
        {
            photoFile = null;

            var settings = PortalController.Instance.GetCurrentPortalSettings();
            var targetUser = UserController.Instance.GetUser(settings.PortalId, this.UserID);
            if (targetUser == null)
            {
                return false;
            }

            var photoProperty = targetUser.Profile.GetProperty("Photo");
            if (photoProperty == null)
            {
                return false;
            }

            var user = UserController.Instance.GetCurrentUserInfo();
            var isVisible = ProfilePropertyAccess.CheckAccessLevel(settings, photoProperty, user, targetUser);

            if (!string.IsNullOrEmpty(photoProperty.PropertyValue) && isVisible)
            {
                photoFile = FileManager.Instance.GetFile(int.Parse(photoProperty.PropertyValue));
                if (photoFile == null)
                {
                    isVisible = false;
                }
            }
            else
            {
                isVisible = false;
            }

            return isVisible;
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
