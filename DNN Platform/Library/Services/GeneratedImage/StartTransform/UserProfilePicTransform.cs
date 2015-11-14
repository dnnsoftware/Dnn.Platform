using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
	public class UserProfilePicTransform : ImageTransform
    {
        #region Properties

        /// <summary>
		/// Sets the UserID of the profile pic
		/// </summary>
		public int UserID { get; set; }

        public override string UniqueString
		{
			get { return base.UniqueString + this.UserID.ToString(); }
		}

        #endregion 
       
        public UserProfilePicTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
		}

		public override Image ProcessImage(Image image)
		{
            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
            var user = UserController.Instance.GetUser(settings.PortalId, UserID);

            IFileInfo photoFile = null;

            Bitmap noAvatar = new Bitmap(Globals.ApplicationMapPath +  @"\images\no_avatar.gif");
		    if (user != null && TryGetPhotoFile(user, out photoFile))
		    {
		        if (!IsImageExtension(photoFile.Extension))
                    return noAvatar;

		        var folder = FolderManager.Instance.GetFolder(photoFile.FolderId);
                return new Bitmap(folder.PhysicalPath + photoFile.FileName);
		    }
		    return noAvatar;
		}

        //whether current user has permission to view target user's photo.
        private bool TryGetPhotoFile(UserInfo targetUser, out IFileInfo photoFile)
        {
            bool isVisible = false;
            photoFile = null;

            UserInfo user = UserController.Instance.GetCurrentUserInfo();
            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
            var photoProperty = targetUser.Profile.GetProperty("Photo");
            if (photoProperty != null)
            {
                isVisible = (user.UserID == targetUser.UserID);
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

        
        private bool IsImageExtension(string extension)
        {
            if (!extension.StartsWith("."))
            {
                extension = string.Format(".{0}", extension);
            }

            List<string> imageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", ".JPEG", ".ICO" };
            return imageExtensions.Contains(extension.ToUpper());
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
