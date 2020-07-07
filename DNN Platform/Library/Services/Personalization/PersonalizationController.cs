// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Personalization
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Text;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Security;

    public class PersonalizationController
    {
        // default implementation relies on HTTPContext
        public void LoadProfile(HttpContext httpContext, int userId, int portalId)
        {
            this.LoadProfile(new HttpContextWrapper(httpContext), userId, portalId);
        }

        public void LoadProfile(HttpContextBase httpContext, int userId, int portalId)
        {
            if (HttpContext.Current.Items["Personalization"] == null)
            {
                httpContext.Items.Add("Personalization", this.LoadProfile(userId, portalId));
            }
        }

        // override allows for manipulation of PersonalizationInfo outside of HTTPContext
        public PersonalizationInfo LoadProfile(int userId, int portalId)
        {
            var personalization = new PersonalizationInfo { UserId = userId, PortalId = portalId, IsModified = false };
            string profileData = Null.NullString;
            if (userId > Null.NullInteger)
            {
                var cacheKey = string.Format(DataCache.UserPersonalizationCacheKey, portalId, userId);
                profileData = CBO.GetCachedObject<string>(
                     new CacheItemArgs(cacheKey, DataCache.UserPersonalizationCacheTimeout,
                     DataCache.UserPersonalizationCachePriority, portalId, userId), GetCachedUserPersonalizationCallback);
            }
            else
            {
                // Anon User - so try and use cookie.
                HttpContext context = HttpContext.Current;
                if (context != null && context.Request.Cookies["DNNPersonalization"] != null)
                {
                    profileData = DecryptData(context.Request.Cookies["DNNPersonalization"].Value);

                    if (string.IsNullOrEmpty(profileData))
                    {
                        var personalizationCookie = new HttpCookie("DNNPersonalization", string.Empty)
                        {
                            Expires = DateTime.Now.AddDays(-1),
                            Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                        };
                        context.Response.Cookies.Add(personalizationCookie);
                    }
                }
            }

            personalization.Profile = string.IsNullOrEmpty(profileData)
                ? new Hashtable() : Globals.DeserializeHashTableXml(profileData);
            return personalization;
        }

        public void SaveProfile(PersonalizationInfo personalization)
        {
            this.SaveProfile(personalization, personalization.UserId, personalization.PortalId);
        }

        // default implementation relies on HTTPContext
        public void SaveProfile(HttpContext httpContext, int userId, int portalId)
        {
            var objPersonalization = (PersonalizationInfo)httpContext.Items["Personalization"];
            this.SaveProfile(objPersonalization, userId, portalId);
        }

        // override allows for manipulation of PersonalizationInfo outside of HTTPContext
        public void SaveProfile(PersonalizationInfo personalization, int userId, int portalId)
        {
            if (personalization != null && personalization.IsModified)
            {
                var profileData = Globals.SerializeHashTableXml(personalization.Profile);
                if (userId > Null.NullInteger)
                {
                    DataProvider.Instance().UpdateProfile(userId, portalId, profileData);

                    // remove then re-add the updated one
                    var cacheKey = string.Format(DataCache.UserPersonalizationCacheKey, portalId, userId);
                    DataCache.RemoveCache(cacheKey);
                    CBO.GetCachedObject<string>(
                        new CacheItemArgs(
                        cacheKey,
                        DataCache.UserPersonalizationCacheTimeout, DataCache.UserPersonalizationCachePriority), _ => profileData);
                }
                else
                {
                    // Anon User - so try and use cookie.
                    var context = HttpContext.Current;
                    if (context != null)
                    {
                        var personalizationCookie = new HttpCookie("DNNPersonalization", EncryptData(profileData))
                        {
                            Expires = DateTime.Now.AddDays(30),
                            Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                        };
                        context.Response.Cookies.Add(personalizationCookie);
                    }
                }
            }
        }

        private static object GetCachedUserPersonalizationCallback(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            var userId = (int)cacheItemArgs.ParamList[1];
            var returnValue = Null.NullString; // Default is no profile
            IDataReader dr = null;
            try
            {
                dr = DataProvider.Instance().GetProfile(userId, portalId);
                if (dr.Read())
                {
                    returnValue = dr["ProfileData"].ToString();
                }
                else // does not exist
                {
                    DataProvider.Instance().AddProfile(userId, portalId);
                }
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return returnValue;
        }

        private static string EncryptData(string profileData)
        {
            return PortalSecurity.Instance.Encrypt(ValidationUtils.GetDecryptionKey(), profileData);
        }

        private static string DecryptData(string profileData)
        {
            return PortalSecurity.Instance.Decrypt(ValidationUtils.GetDecryptionKey(), profileData);
        }
    }
}
