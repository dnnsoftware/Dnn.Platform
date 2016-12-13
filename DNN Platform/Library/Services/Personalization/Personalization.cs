#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System.Web;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;

#endregion

namespace DotNetNuke.Services.Personalization
{
    public class Personalization
    {
		#region Private Methods

        private static PersonalizationInfo LoadProfile()
        {
            HttpContext context = HttpContext.Current;

            //First try and load Personalization object from the Context
            var personalization = (PersonalizationInfo) context.Items["Personalization"];

            //If the Personalization object is nothing load it and store it in the context for future calls
            if (personalization == null)
            {
                var _portalSettings = (PortalSettings) context.Items["PortalSettings"];

                //load the user info object
                UserInfo UserInfo = UserController.Instance.GetCurrentUserInfo();

                //get the personalization object
                var personalizationController = new PersonalizationController();
                personalization = personalizationController.LoadProfile(UserInfo.UserID, _portalSettings.PortalId);

                //store it in the context
                context.Items.Add("Personalization", personalization);
            }
            return personalization;
        }
		
		#endregion

		#region Public Shared Methods

        /// <summary>
        /// load users profile and extract value base on naming container and key
        /// </summary>
        /// <param name="namingContainer">Container for related set of values</param>
        /// <param name="key">Individual profile key</param>
        /// <returns></returns>
        public static object GetProfile(string namingContainer, string key)
        {
            return GetProfile(LoadProfile(), namingContainer, key);
        }

        /// <summary>
        /// extract value base on naming container and key from PersonalizationInfo object
        /// </summary>
        /// <param name="personalization">Object containing user personalization info</param>
        /// <param name="namingContainer">Container for related set of values</param>
        /// <param name="key">Individual profile key</param>
        /// <returns></returns>
        public static object GetProfile(PersonalizationInfo personalization, string namingContainer, string key)
        {
            return personalization != null ? personalization.Profile[namingContainer + ":" + key] : "";
        }

        /// <summary>
        /// load users profile and extract secure value base on naming container and key
       /// </summary>
        /// <param name="namingContainer">Container for related set of values</param>
        /// <param name="key">Individual profile key</param>
       /// <returns></returns>
        public static object GetSecureProfile(string namingContainer, string key)
        {
            return GetSecureProfile(LoadProfile(), namingContainer, key);
        }

        /// <summary>
        /// extract value base on naming container and key from PersonalizationInfo object
        /// function will automatically decrypt value to plaintext
        /// </summary>
        /// <param name="personalization">Object containing user personalization info</param>
        /// <param name="namingContainer">Container for related set of values</param>
        /// <param name="key">Individual profile key</param>
        /// <returns></returns>
        public static object GetSecureProfile(PersonalizationInfo personalization, string namingContainer, string key)
        {
            if (personalization != null)
            {
                var ps = new PortalSecurity();
                return ps.DecryptString(personalization.Profile[namingContainer + ":" + key].ToString(), Config.GetDecryptionkey());
            }
            return "";
        }

        /// <summary>
        /// remove value from profile
        /// uses namingcontainer and key to locate approriate value
        /// </summary>
        /// <param name="namingContainer">Container for related set of values</param>
        /// <param name="key">Individual profile key</param>
        public static void RemoveProfile(string namingContainer, string key)
        {
            RemoveProfile(LoadProfile(), namingContainer, key);
        }

        /// <summary>
        /// remove value from users PersonalizationInfo object (if it exists)
        /// uses namingcontainer and key to locate approriate value
        /// </summary>
        /// <param name="personalization">Object containing user personalization info</param>
        /// <param name="namingContainer">Container for related set of values</param>
        /// <param name="key">Individual profile key</param>
        public static void RemoveProfile(PersonalizationInfo personalization, string namingContainer, string key)
        {
            if (personalization != null)
            {
                (personalization.Profile).Remove(namingContainer + ":" + key);
                personalization.IsModified = true;
            }
        }

        /// <summary>
        /// persist profile value -use naming container and key to orgainize
        /// </summary>
        /// <param name="namingContainer">Container for related set of values</param>
        /// <param name="key">Individual profile key</param>
        /// <param name="value">Individual profile value</param>
        public static void SetProfile(string namingContainer, string key, object value)
        {
            SetProfile(LoadProfile(), namingContainer, key, value);
        }

        /// <summary>
        /// persist value stored in PersonalizationInfo obhect - use naming container and key to organize
        /// </summary>
        /// <param name="personalization">Object containing user personalization info</param>
        /// <param name="namingContainer">Container for related set of values</param>
        /// <param name="key">Individual profile key</param>
        /// <param name="value">Individual profile value</param>
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
        /// function calls an overload which automatically encrypts the value
        /// </summary>
        /// <param name="namingContainer">Object containing user personalization info</param>
        /// <param name="key">Individual profile key</param>
        /// <param name="value">Individual profile value</param>
        public static void SetSecureProfile(string namingContainer, string key, object value)
        {
            SetSecureProfile(LoadProfile(), namingContainer, key, value);
        }

        /// <summary>
        /// persist profile value from PersonalizationInfo object, using naming container and key to organise 
        /// function will automatically encrypt the value to plaintext
        /// </summary>
        /// <param name="personalization">Object containing user personalization info</param>
        /// <param name="namingContainer">Container for related set of values</param>
        /// <param name="key">Individual profile key</param>
        /// <param name="value">Individual profile value</param>
        public static void SetSecureProfile(PersonalizationInfo personalization, string namingContainer, string key, object value)
        {
            if (personalization != null)
            {
                var ps = new PortalSecurity();
                personalization.Profile[namingContainer + ":" + key] = ps.EncryptString(value.ToString(), Config.GetDecryptionkey());
                personalization.IsModified = true;
            }
        }
		#endregion
    }
}