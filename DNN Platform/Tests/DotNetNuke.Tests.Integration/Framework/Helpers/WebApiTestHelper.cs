// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017, DNN Corp.
// All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using DotNetNuke.Tests.Integration.Framework.Controllers;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.Framework.Helpers
{
    public static class WebApiTestHelper
    {
        private static IWebApiConnector _anonymousConnector;
        private static readonly Dictionary<string, IWebApiConnector> CachedConnections = new Dictionary<string, IWebApiConnector>();

        private static readonly string HostGuid = HostSettingsHelper.GetHostSettingValue("GUID");

        internal static void ClearCachedConnections()
        {
            _anonymousConnector = null;
            CachedConnections.Clear();
        }

        /// <summary>
        /// Returns a coonector to access the default site annonymously
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions</returns>
        public static IWebApiConnector GetAnnonymousConnector(string url = null)
        {
            url = url ?? AppConfigHelper.SiteUrl;
            return _anonymousConnector ??
                (_anonymousConnector = WebApiConnector.GetWebConnector(url, null));
        }

        /// <summary>
        /// Register a user by using the Registration form
        /// </summary>
        public static HttpWebResponse Register(string userName, string password, string displayName, string email,
            string url = null, bool encriptFieldsNames = true)
        {
            const string registerRelativeUrl = "/Register";
            const string registerFieldsPrefix = "dnn$ctr$Register$userForm";

            var postData = new Dictionary<string, object>
            {
                {registerFieldsPrefix + CodifyInputName("Username", "TextBox", encriptFieldsNames), userName }, 
                {registerFieldsPrefix + CodifyInputName("Password", "TextBox", encriptFieldsNames), password},
                {registerFieldsPrefix + CodifyInputName("PasswordConfirm", "TextBox", encriptFieldsNames), password},
                {registerFieldsPrefix + CodifyInputName("DisplayName", "TextBox", encriptFieldsNames), displayName},
                {registerFieldsPrefix + CodifyInputName("Email", "TextBox", encriptFieldsNames), email},
                {"__EVENTTARGET", "dnn$ctr$Register$registerButton"},
                {"__EVENTARGUMENT", ""},
                {"__ASYNCPOST", "true"}
            };

            url = url ?? AppConfigHelper.SiteUrl;
            var excludedInputPrefixes = new List<string>();
            var connector = WebApiConnector.GetWebConnector(url, "");
            return connector.PostUserForm(registerRelativeUrl, postData, excludedInputPrefixes, false);
        }

        private static string CodifyInputName(string originalId, string sufix, bool encriptName)
        {
            var transformedId = encriptName ? CodifyString(originalId) : originalId;
            return "$" + transformedId + "$" + transformedId + "_" + sufix;
        }

        private static string CodifyString(string originalString)
        {
            return GenerateHash(HostGuid.Substring(0, 7) + originalString + DateTime.Now.Day);
        }

        private static string GenerateHash(string str)
        {
            try
            {
                return CryptoConfig.AllowOnlyFipsAlgorithms
                    ? GenerateSha256Hash(str)
                    : GenerateMd5(str);
            }
            catch (Exception)
            {
                return GenerateMd5(str);
            }
        }

        private static string GenerateSha256Hash(string str)
        {
            using (var hasher = new SHA256CryptoServiceProvider())
            {
                var byteArray = hasher.ComputeHash(Encoding.Unicode.GetBytes(str));
                return byteArray.Aggregate("", (current, b) => current + b.ToString("x2"));
            }
        }
        
        private static string GenerateMd5(string str)
        {
            using (var hasher = new MD5CryptoServiceProvider())
            {
                var byteArray = hasher.ComputeHash(Encoding.Unicode.GetBytes(str));
                return byteArray.Aggregate("", (current, b) => current + b.ToString("x2"));
            }
        }

        /// <summary>
        /// Creates a Registered User and performs Login for that user in as well. 
        /// Password used is same as that for Host. Existing user is used if it's already present.
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions</returns>
        public static IWebApiConnector LoginRegisteredUser(string firstName = IntegrationConstants.RuFirstName, string lastName = IntegrationConstants.RuLastName, string url = null)
        {
            IWebApiConnector connector;
            url = url ?? AppConfigHelper.SiteUrl;
            var username = firstName + "." + lastName;
            var key = string.Join("_", url, username);
            if (!CachedConnections.TryGetValue(key, out connector) || 
                connector.LoggedInAtTime < DateTime.Now.AddMinutes(-19.5))
            {
                var portalId = GetPortalFromUrl(url);
                UserController.CreateRegisteredUser(firstName, lastName, portalId);
                connector = WebApiConnector.GetWebConnector(url, username);
                CachedConnections[key] = connector;
            }

            if (!connector.IsLoggedIn)
            {
                var loggedIn = connector.Login(AppConfigHelper.HostPassword);
                Assert.IsTrue(loggedIn);
            }

            return connector;
        }

        /// <summary>
        /// Creates an Administrator and performs Login for that user in as well. 
        /// Password used is same as that for Host. Existing user is used if it's already present.
        /// </summary>
        /// <returns>WebApiConnector object to perform more actions</returns>
        public static IWebApiConnector LoginAdministrator(string firstName = IntegrationConstants.AdminFirstName, string lastName = IntegrationConstants.AdminLastName, string url = null)
        {
            IWebApiConnector connector;
            url = url ?? AppConfigHelper.SiteUrl;
            var username = firstName + "." + lastName;
            var key = string.Join("_", url, username);
            if (!CachedConnections.TryGetValue(key, out connector))
            {
                var portalId = GetPortalFromUrl(url);
                UserController.CreateAdministratorUser(firstName, lastName, portalId);
                connector = WebApiConnector.GetWebConnector(url, username);
                CachedConnections.Add(key, connector);
            }

            if (!connector.IsLoggedIn)
            {
                var loggedIn = connector.Login(AppConfigHelper.HostPassword);
                Assert.IsTrue(loggedIn);
            }

            return connector;
        }

        /// <summary>
        /// Logs in Host.
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions</returns>
        public static IWebApiConnector LoginHost()
        {
            return LoginUser(AppConfigHelper.HostUserName);
        }

        /// <summary>
        /// Logs in an already registered user (regardless of the user's role)
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions</returns>
        public static IWebApiConnector LoginUser(string username, string url = null)
        {
            IWebApiConnector connector;
            url = url ?? AppConfigHelper.SiteUrl;
            var key = string.Join("_", url, username);
            if (!CachedConnections.TryGetValue(key, out connector))
            {
                connector = WebApiConnector.GetWebConnector(url, username);
                CachedConnections.Add(key, connector);
            }

            if (!connector.IsLoggedIn)
            {
                var loggedIn = connector.Login(AppConfigHelper.HostPassword);
                Assert.IsTrue(loggedIn);
            }

            return connector;
        }

        public static int GetPortalFromUrl(string url)
        {
            //TODO
            return 0;
        }

        /// <summary>
        /// Logs in as host and clears host cache
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions</returns>
        public static IWebApiConnector ClearHostCache()
        {
            var connector = LoginUser(AppConfigHelper.HostUserName);
            connector.PostJson("API/internalservices/controlbar/ClearHostCache", null);
            return connector;
        }

        /// <summary>
        /// Logs in as host and recycle the application
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions</returns>
        public static void RecycleApplication()
        {
            var connector = LoginUser(AppConfigHelper.HostUserName);
            connector.PostJson("API/internalservices/controlbar/RecycleApplicationPool ", null);
        }

        /// <summary>
        /// Returns TabId and ModuleId headers for a Page to be passed in a Post WebApi call
        /// </summary>
        /// <param name="tabPath">double-slashed prefixed page name, e.g. //Home or //Groups//GroupSpaces//Members</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetTabModuleHeaders(string tabPath)
        {
            var tabId = TabController.GetTabIdByPathName(tabPath);
            var moduleId = TabController.GetModuleIdInsidePage(tabId);

            return new Dictionary<string, string>
            {
                {"TabID", tabId.ToString("D")},
                {"ModuleID", moduleId.ToString("D")},
            };
        }

        /// <summary>
        /// Returns TabId and ModuleId headers for a Page and Module to be passed in a Post WebApi call
        /// </summary>
        /// <param name="moduleFriendlyName">Module's friendly name, e.g. Wiki or Activity Stream</param>
        /// <param name="tabPath">double-slashed prefixed page name, e.g. //Home or //Groups//GroupSpaces//Members</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetTabModuleHeaders(string moduleFriendlyName, string tabPath)
        {
            var tabId = TabController.GetTabIdByPathName(tabPath);
            var moduleId = TabController.GetModuleIdInsidePage(tabId, moduleFriendlyName);

            return new Dictionary<string, string>
            {
                {"TabID", tabId.ToString("D")},
                {"ModuleID", moduleId.ToString("D")},
            };
        }
    }
}
