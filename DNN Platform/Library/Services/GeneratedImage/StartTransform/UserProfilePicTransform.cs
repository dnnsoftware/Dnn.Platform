using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
    /// <summary>
    /// User Profile Picture ImageTransform class
    /// </summary>
	public class UserProfilePicTransform : ImageTransform
    {
        #region Properties
        /// <summary>
		/// Sets the UserID of the profile pic
		/// </summary>
		public int UserID { get; set; }

        /// <summary>
        /// Provides an Unique String for the image transformation
        /// </summary>
        public override string UniqueString => base.UniqueString + UserID;

        /// <summary>
        /// Is reusable
        /// </summary>
        public bool IsReusable => false;
        #endregion

        public UserProfilePicTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
		}

        /// <summary>
        /// Processes an input image returning the user profile picture
        /// </summary>
        /// <param name="image">Input image</param>
        /// <returns>Image result after image transformation</returns>
        public override Image ProcessImage(Image image)
		{
            var settings = PortalController.Instance.GetCurrentPortalSettings();
            var user = UserController.Instance.GetUser(settings.PortalId, UserID);

            IFileInfo photoFile;

            var noAvatar = new Bitmap(Globals.ApplicationMapPath +  @"\images\no_avatar.gif");
		    if (user != null && TryGetPhotoFile(user, out photoFile))
		    {
		        if (!IsImageExtension(photoFile.Extension))
                    return noAvatar;

		        var folder = FolderManager.Instance.GetFolder(photoFile.FolderId);
                return new Bitmap(folder.PhysicalPath + photoFile.FileName);
		    }
		    return noAvatar;
		}
        
        /// <summary>
        /// whether current user has permission to view target user's photo.
        /// </summary>
        /// <param name="targetUser"></param>
        /// <param name="photoFile"></param>
        /// <returns></returns>
        private bool TryGetPhotoFile(UserInfo targetUser, out IFileInfo photoFile)
        {
            var isVisible = false;
            photoFile = null;

            var user = UserController.Instance.GetCurrentUserInfo();
            var settings = PortalController.Instance.GetCurrentPortalSettings();
            var photoProperty = targetUser.Profile.GetProperty("Photo");
            if (photoProperty != null)
            {
                isVisible = user.UserID == targetUser.UserID;
                if (!isVisible)
                {
                    switch (photoProperty.ProfileVisibility.VisibilityMode)
                    {
                        case UserVisibilityMode.AllUsers:
                            isVisible = true;
                            break;
                        case UserVisibilityMode.MembersOnly:
                            isVisible = user.UserID > 0;
                            break;
                        case UserVisibilityMode.AdminOnly:
                            isVisible = user.IsInRole(settings.AdministratorRoleName);
                            break;
                        case UserVisibilityMode.FriendsAndGroups:
                            break;
                    }
                }

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
