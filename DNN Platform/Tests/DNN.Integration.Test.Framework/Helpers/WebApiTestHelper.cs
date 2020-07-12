// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNN.Integration.Test.Framework.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;

    using DNN.Integration.Test.Framework.Controllers;
    using NUnit.Framework;

    public static class WebApiTestHelper
    {
        private static readonly Dictionary<string, IWebApiConnector> CachedConnections = new Dictionary<string, IWebApiConnector>();
        private static readonly Random Rnd = new Random();
        private static readonly string HostGuid = HostSettingsHelper.GetHostSettingValue("GUID");
        private static IWebApiConnector _anonymousConnector;

        /// <summary>
        /// Returns a coonector to access the default site annonymously.
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions.</returns>
        public static IWebApiConnector GetAnnonymousConnector(string url = null)
        {
            url = url ?? AppConfigHelper.SiteUrl;
            return _anonymousConnector ??
                (_anonymousConnector = WebApiConnector.GetWebConnector(url, null));
        }

        public static IDictionary<string, string> GetRequestHeaders(string tabPath, string moduleName, int portalId = 0)
        {
            var tabId = DatabaseHelper.ExecuteScalar<int>($"SELECT * FROM {{objectQualifier}}Tabs WHERE TabPath = '{tabPath}' AND PortalId = {portalId}");
            var moduleId = DatabaseHelper.ExecuteScalar<int>(
                $@"
SELECT TOP 1 m.ModuleID FROM {{objectQualifier}}TabModules tm
	INNER JOIN {{objectQualifier}}modules m ON m.ModuleID = tm.ModuleID
	INNER JOIN {{objectQualifier}}ModuleDefinitions md ON md.ModuleDefID = m.ModuleDefID
WHERE tm.TabID = {tabId} AND md.FriendlyName = '{moduleName}'");

            return new Dictionary<string, string>
            {
                { "TabId", tabId.ToString() },
                { "ModuleId", moduleId.ToString() },
            };
        }

        public static IWebApiConnector PrepareNewUser(out int userId, out string username, out int fileId, int portalId = 0)
        {
            username = $"testuser{Rnd.Next(1000, 9999)}";
            var email = $"{username}@dnn.com";

            using (WebApiTestHelper.Register(username, AppConfigHelper.HostPassword, username, email))
            {
            }

            userId = DatabaseHelper.ExecuteScalar<int>($"SELECT UserId FROM {{objectQualifier}}Users WHERE Username = '{username}'");

            var connector = WebApiTestHelper.LoginHost();
            var url = $"/API/PersonaBar/Users/UpdateAuthorizeStatus?userId={userId}&authorized=true";
            connector.PostJson(url, new { });
            connector.Logout();

            var userConnector = WebApiTestHelper.LoginUser(username);
            userConnector.UploadUserFile("Files\\Test.png", true, userId);

            fileId = DatabaseHelper.ExecuteScalar<int>($"SELECT MAX(FileId) FROM {{objectQualifier}}Files WHERE FileName = 'Test.png' AND CreatedByUserID = {userId} AND PortalId = {portalId}");

            return userConnector;
        }

        /// <summary>
        /// Register a user by using the Registration form.
        /// </summary>
        /// <returns></returns>
        public static HttpWebResponse Register(string userName, string password, string displayName, string email,
            string url = null, bool encriptFieldsNames = true)
        {
            const string registerRelativeUrl = "/Register";
            const string registerFieldsPrefix = "dnn$ctr$Register$userForm";

            var postData = new Dictionary<string, object>
            {
                { registerFieldsPrefix + CodifyInputName("Username", "TextBox", encriptFieldsNames), userName },
                { registerFieldsPrefix + CodifyInputName("Password", "TextBox", encriptFieldsNames), password },
                { registerFieldsPrefix + CodifyInputName("PasswordConfirm", "TextBox", encriptFieldsNames), password },
                { registerFieldsPrefix + CodifyInputName("DisplayName", "TextBox", encriptFieldsNames), displayName },
                { registerFieldsPrefix + CodifyInputName("Email", "TextBox", encriptFieldsNames), email },
                { "__EVENTTARGET", "dnn$ctr$Register$registerButton" },
                { "__EVENTARGUMENT", string.Empty },
                { "__ASYNCPOST", "true" },
            };

            url = url ?? AppConfigHelper.SiteUrl;
            var excludedInputPrefixes = new List<string>();
            var connector = WebApiConnector.GetWebConnector(url, string.Empty);
            return connector.PostUserForm(registerRelativeUrl, postData, excludedInputPrefixes, false);
        }

        public static string GenerateSha256Hash(this string str)
        {
            return str.GenerateHash("SHA256");
        }

        /// <summary>Generate a MD5 hash of a string.
        /// </summary>
        /// <returns></returns>
        public static string GenerateMd5(this string str)
        {
            return str.GenerateHash("MD5");
        }

        /// <summary>
        /// Creates a Registered User and performs Login for that user in as well.
        /// Password used is same as that for Host. Existing user is used if it's already present.
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions.</returns>
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
        /// <returns>WebApiConnector object to perform more actions.</returns>
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
        /// <returns>IWebApiConnector object to perform more actions.</returns>
        public static IWebApiConnector LoginHost()
        {
            return LoginUser(AppConfigHelper.HostUserName);
        }

        /// <summary>
        /// Logs in an already registered user (regardless of the user's role).
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions.</returns>
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
            return 0;
        }

        /// <summary>
        /// Logs in as host and clears host cache.
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions.</returns>
        public static IWebApiConnector ClearHostCache()
        {
            var connector = LoginUser(AppConfigHelper.HostUserName);

            // connector.PostJson("API/internalservices/controlbar/ClearHostCache", null);
            connector.PostJson("API/PersonaBar/Server/ClearCache", null);
            return connector;
        }

        /// <summary>
        /// Logs in as host and recycle the application.
        /// </summary>
        public static void RecycleApplication()
        {
            var connector = LoginUser(AppConfigHelper.HostUserName);
            connector.PostJson("API/internalservices/controlbar/RecycleApplicationPool ", null);
        }

        /// <summary>
        /// Returns TabId and ModuleId headers for a Page to be passed in a Post WebApi call.
        /// </summary>
        /// <param name="tabPath">double-slashed prefixed page name, e.g. //Home or //Groups//GroupSpaces//Members.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetTabModuleHeaders(string tabPath)
        {
            var tabId = TabController.GetTabIdByPathName(tabPath);
            var moduleId = TabController.GetModuleIdInsidePage(tabId);

            return new Dictionary<string, string>
            {
                { "TabID", tabId.ToString("D") },
                { "ModuleID", moduleId.ToString("D") },
            };
        }

        /// <summary>
        /// Returns TabId and ModuleId headers for a Page and Module to be passed in a Post WebApi call.
        /// </summary>
        /// <param name="moduleFriendlyName">Module's friendly name, e.g. Wiki or Activity Stream.</param>
        /// <param name="tabPath">double-slashed prefixed page name, e.g. //Home or //Groups//GroupSpaces//Members.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetTabModuleHeaders(string moduleFriendlyName, string tabPath)
        {
            var tabId = TabController.GetTabIdByPathName(tabPath);
            var moduleId = TabController.GetModuleIdInsidePage(tabId, moduleFriendlyName);

            return new Dictionary<string, string>
            {
                { "TabID", tabId.ToString("D") },
                { "ModuleID", moduleId.ToString("D") },
            };
        }

        internal static void ClearCachedConnections()
        {
            _anonymousConnector = null;
            CachedConnections.Clear();
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
                    ? str.GenerateSha256Hash()
                    : str.GenerateMd5();
            }
            catch (Exception)
            {
                return str.GenerateMd5();
            }
        }

        private static string GenerateHash(this string str, string hashType)
        {
            var hasher = HashAlgorithm.Create(hashType);
            if (hasher == null)
            {
                throw new InvalidOperationException("No hashing type found by name " + hashType);
            }

            using (hasher)
            {
                // convert our string into byte array
                var byteArray = Encoding.UTF8.GetBytes(str);

                // get the hashed values created by our SHA1CryptoServiceProvider
                var hashedByteArray = hasher.ComputeHash(byteArray);

                // create a StringBuilder object
                var stringBuilder = new StringBuilder();

                // loop to each each byte
                foreach (var b in hashedByteArray)
                {
                    // append it to our StringBuilder
                    stringBuilder.Append(b.ToString("x2").ToLowerInvariant());
                }

                // return the hashed value
                return stringBuilder.ToString();
            }
        }
    }
}
