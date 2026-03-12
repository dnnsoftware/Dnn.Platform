// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Personalization
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Security;

    using Microsoft.Extensions.DependencyInjection;

    public class PersonalizationController
    {
        private const string PersonalizationCookieName = "DNNPersonalization";
        private const string AlgorithmCookieName = "DNNPersonalizationAlg";
        private readonly IHostSettings hostSettings;
        private readonly ICryptographyProvider cryptographyProvider;

        /// <summary>Initializes a new instance of the <see cref="PersonalizationController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public PersonalizationController()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PersonalizationController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public PersonalizationController(IHostSettings hostSettings)
            : this(null, null)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

        /// <summary>Initializes a new instance of the <see cref="PersonalizationController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        public PersonalizationController(IHostSettings hostSettings, ICryptographyProvider cryptographyProvider)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.cryptographyProvider = cryptographyProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
        }

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
            var personalization = new PersonalizationInfo { UserId = userId, PortalId = portalId, IsModified = false, };
            string profileData = Null.NullString;
            if (userId > Null.NullInteger)
            {
                var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.UserPersonalizationCacheKey, portalId, userId);
                profileData = CBO.GetCachedObject<string>(
                    this.hostSettings,
                    new CacheItemArgs(
                        cacheKey,
                        DataCache.UserPersonalizationCacheTimeout,
                        DataCache.UserPersonalizationCachePriority,
                        portalId,
                        userId),
                    GetCachedUserPersonalizationCallback);
            }
            else
            {
                // Anon User - so try and use cookie.
                HttpContext context = HttpContext.Current;
                if (context?.Request.Cookies[PersonalizationCookieName] != null)
                {
                    var algorithm = context.Request.Cookies[AlgorithmCookieName]?.Value;
                    if (string.IsNullOrWhiteSpace(algorithm))
                    {
                        algorithm = HashAlgorithmName.SHA1.Name;
                    }

                    profileData = this.DecryptData(new HashAlgorithmName(algorithm), context.Request.Cookies[PersonalizationCookieName].Value);

                    if (string.IsNullOrEmpty(profileData))
                    {
                        var personalizationCookie = new HttpCookie(PersonalizationCookieName, string.Empty)
                        {
                            Expires = DateTime.Now.AddDays(-1),
                            Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                        };
                        context.Response.Cookies.Add(personalizationCookie);
                    }
                }
            }

            personalization.Profile = string.IsNullOrEmpty(profileData)
                ? new Hashtable() : XmlUtils.DeSerializeHashtable(profileData, "profile");
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
            if (personalization is { IsModified: true, })
            {
                var profileData = XmlUtils.SerializeDictionary(personalization.Profile, "profile");
                if (userId > Null.NullInteger)
                {
                    DataProvider.Instance().UpdateProfile(userId, portalId, profileData);

                    // remove then re-add the updated one
                    var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.UserPersonalizationCacheKey, portalId, userId);
                    DataCache.RemoveCache(cacheKey);
                    CBO.GetCachedObject<string>(
                        this.hostSettings,
                        new CacheItemArgs(
                            cacheKey,
                            DataCache.UserPersonalizationCacheTimeout,
                            DataCache.UserPersonalizationCachePriority),
                        _ => profileData);
                }
                else
                {
                    // Anon User - so try and use cookie.
                    var context = HttpContext.Current;
                    if (context != null)
                    {
                        var personalizationCookie = new HttpCookie(PersonalizationCookieName, this.EncryptData(HashAlgorithmName.SHA512, profileData))
                        {
                            Expires = DateTime.Now.AddDays(30),
                            Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                        };
                        context.Response.Cookies.Add(personalizationCookie);
                        var algorithmCookie = new HttpCookie(AlgorithmCookieName, HashAlgorithmName.SHA512.Name)
                        {
                            Expires = DateTime.Now.AddDays(30),
                            Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                        };
                        context.Response.Cookies.Add(algorithmCookie);
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
                else
                {
                    // does not exist
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

        private string EncryptData(HashAlgorithmName hashAlgorithm, string profileData)
        {
            return this.cryptographyProvider.EncryptParameter(profileData, ValidationUtils.GetDecryptionKey(this.hostSettings, hashAlgorithm)).EncryptedMessage;
        }

        private string DecryptData(HashAlgorithmName hashAlgorithm, string profileData)
        {
            return this.cryptographyProvider.DecryptParameter(profileData, ValidationUtils.GetDecryptionKey(this.hostSettings, hashAlgorithm), this.cryptographyProvider.EncryptParameterAlgorithmName);
        }
    }
}
