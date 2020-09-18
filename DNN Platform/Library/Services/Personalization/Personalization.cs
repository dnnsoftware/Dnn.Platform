// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Personalization
{
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;

    public class Personalization
    {
        /// <summary>
        /// load users profile and extract value base on naming container and key.
        /// </summary>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <returns></returns>
        public static object GetProfile(string namingContainer, string key)
        {
            return GetProfile(LoadProfile(), namingContainer, key);
        }

        /// <summary>
        /// extract value base on naming container and key from PersonalizationInfo object.
        /// </summary>
        /// <param name="personalization">Object containing user personalization info.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <returns></returns>
        public static object GetProfile(PersonalizationInfo personalization, string namingContainer, string key)
        {
            return personalization != null ? personalization.Profile[namingContainer + ":" + key] : string.Empty;
        }

        /// <summary>
        /// load users profile and extract secure value base on naming container and key.
        /// </summary>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <returns></returns>
        public static object GetSecureProfile(string namingContainer, string key)
        {
            return GetSecureProfile(LoadProfile(), namingContainer, key);
        }

        /// <summary>
        /// extract value base on naming container and key from PersonalizationInfo object
        /// function will automatically decrypt value to plaintext.
        /// </summary>
        /// <param name="personalization">Object containing user personalization info.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <returns></returns>
        public static object GetSecureProfile(PersonalizationInfo personalization, string namingContainer, string key)
        {
            if (personalization != null)
            {
                var ps = PortalSecurity.Instance;
                return ps.DecryptString(personalization.Profile[namingContainer + ":" + key].ToString(), Config.GetDecryptionkey());
            }

            return string.Empty;
        }

        /// <summary>
        /// remove value from profile
        /// uses namingcontainer and key to locate approriate value.
        /// </summary>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        public static void RemoveProfile(string namingContainer, string key)
        {
            RemoveProfile(LoadProfile(), namingContainer, key);
        }

        /// <summary>
        /// remove value from users PersonalizationInfo object (if it exists)
        /// uses namingcontainer and key to locate approriate value.
        /// </summary>
        /// <param name="personalization">Object containing user personalization info.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        public static void RemoveProfile(PersonalizationInfo personalization, string namingContainer, string key)
        {
            if (personalization != null)
            {
                personalization.Profile.Remove(namingContainer + ":" + key);
                personalization.IsModified = true;
            }
        }

        /// <summary>
        /// persist profile value -use naming container and key to orgainize.
        /// </summary>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <param name="value">Individual profile value.</param>
        public static void SetProfile(string namingContainer, string key, object value)
        {
            SetProfile(LoadProfile(), namingContainer, key, value);
        }

        /// <summary>
        /// persist value stored in PersonalizationInfo obhect - use naming container and key to organize.
        /// </summary>
        /// <param name="personalization">Object containing user personalization info.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <param name="value">Individual profile value.</param>
        public static void SetProfile(PersonalizationInfo personalization, string namingContainer, string key, object value)
        {
            if (personalization != null)
            {
                personalization.Profile[namingContainer + ":" + key] = value;
                personalization.IsModified = true;
            }
        }

        /// <summary>
        /// persist profile value -use naming container and key to orgainize
        /// function calls an overload which automatically encrypts the value.
        /// </summary>
        /// <param name="namingContainer">Object containing user personalization info.</param>
        /// <param name="key">Individual profile key.</param>
        /// <param name="value">Individual profile value.</param>
        public static void SetSecureProfile(string namingContainer, string key, object value)
        {
            SetSecureProfile(LoadProfile(), namingContainer, key, value);
        }

        /// <summary>
        /// persist profile value from PersonalizationInfo object, using naming container and key to organise
        /// function will automatically encrypt the value to plaintext.
        /// </summary>
        /// <param name="personalization">Object containing user personalization info.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <param name="value">Individual profile value.</param>
        public static void SetSecureProfile(PersonalizationInfo personalization, string namingContainer, string key, object value)
        {
            if (personalization != null)
            {
                var ps = PortalSecurity.Instance;
                personalization.Profile[namingContainer + ":" + key] = ps.EncryptString(value.ToString(), Config.GetDecryptionkey());
                personalization.IsModified = true;
            }
        }

        private static PersonalizationInfo LoadProfile()
        {
            HttpContext context = HttpContext.Current;

            // First try and load Personalization object from the Context
            var personalization = (PersonalizationInfo)context.Items["Personalization"];

            // If the Personalization object is nothing load it and store it in the context for future calls
            if (personalization == null)
            {
                var _portalSettings = (PortalSettings)context.Items["PortalSettings"];

                // load the user info object
                UserInfo UserInfo = UserController.Instance.GetCurrentUserInfo();

                // get the personalization object
                var personalizationController = new PersonalizationController();
                personalization = personalizationController.LoadProfile(UserInfo.UserID, _portalSettings.PortalId);

                // store it in the context
                context.Items.Add("Personalization", personalization);
            }

            return personalization;
        }
    }
}
