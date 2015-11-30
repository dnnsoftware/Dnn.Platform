using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Services.Journal;
using System.Globalization;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Modules.Journal.Components
{
    public class ProfilePicPropertyAccess: IPropertyAccess
    {
        private int userId;
        public int Size { get; set; } = 32;

        public ProfilePicPropertyAccess(int UserId)
        {
            this.userId = UserId;
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {

            if (propertyName.ToLowerInvariant() == "relativeurl")
            {
                int size;
                if (int.TryParse(format, out size)) {
                    Size = size;
                }
                var settings = PortalController.Instance.GetCurrentPortalSettings();
                var userInfo = UserController.GetUserById(settings.PortalId, userId);
                var url = string.Format(Globals.UserProfilePicRelativeUrl(false), userId, Size, Size);

                if (userInfo.Profile != null)
                {
                    var photoProperty = userInfo.Profile.GetProperty("Photo");

                    int photoFileId;
                    if (photoProperty != null && int.TryParse(photoProperty.PropertyValue, out photoFileId))
                    {
                        var photoFile = FileManager.Instance.GetFile(photoFileId);
                        if (photoFile != null)
                        {
                            return url + "&cdv=" + photoFile.LastModificationTime.Ticks;
                        }
                    }

                    else
                    {
                        return url;
                    }
                }
            }
            propertyNotFound = true;
            return string.Empty;
        }

    }
}