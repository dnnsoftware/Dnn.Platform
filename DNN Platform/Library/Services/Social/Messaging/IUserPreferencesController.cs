using DotNetNuke.Entities.Users;

namespace DotNetNuke.Services.Social.Messaging
{
    /// <summary>
    /// This class is responsible to manage the Messaging User Preference
    /// </summary>
    public interface IUserPreferencesController
    {
        /// <summary>
        /// Set the User Messaging Preference
        /// </summary>
        /// <param name="userPreference">User Preference</param>
        void SetUserPreference(UserPreference userPreference);

        /// <summary>
        /// Get the User Messaging Preference
        /// </summary>
        /// <param name="userinfo">User info</param>
        /// <returns>User Messaging Preference</returns>
        UserPreference GetUserPreference(UserInfo userinfo);
    }
}
