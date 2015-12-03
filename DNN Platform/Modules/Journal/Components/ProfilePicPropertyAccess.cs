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
        private readonly int _userId;

        public int Size { get; set; } = 32;

        public ProfilePicPropertyAccess(int userId)
        {
            _userId = userId;
        }

        public CacheLevel Cacheability => CacheLevel.notCacheable;

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {

            if (propertyName.ToLowerInvariant() == "relativeurl")
            {
                int size;
                if (int.TryParse(format, out size)) {
                    Size = size;
                }
                var settings = PortalController.Instance.GetCurrentPortalSettings();
                var userInfo = UserController.GetUserById(settings.PortalId, _userId);
                var url = string.Format(Globals.UserProfilePicRelativeUrl(false), _userId, Size, Size);

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