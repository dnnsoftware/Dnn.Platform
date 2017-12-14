using System.Globalization;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using DotNetNuke.Common;

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
                return UserController.Instance.GetUserProfilePictureUrl(_userId, Size, Size);
            }
            propertyNotFound = true;
            return string.Empty;
        }
    }
}