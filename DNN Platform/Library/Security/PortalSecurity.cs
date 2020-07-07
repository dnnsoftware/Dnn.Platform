// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// ReSharper disable MemberCanBeMadeStatic.Global
namespace DotNetNuke.Security
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Security;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Security.Cookies;
    using DotNetNuke.Services.Cryptography;

    public class PortalSecurity
    {
        private const string RoleFriendPrefix = "FRIEND:";
        private const string RoleFollowerPrefix = "FOLLOWER:";
        private const string RoleOwnerPrefix = "OWNER:";

        private const string BadStatementExpression = ";|--|\bcreate\b|\bdrop\b|\bselect\b|\binsert\b|\bdelete\b|\bupdate\b|\bunion\b|sp_|xp_|\bexec\b|\bexecute\b|/\\*.*\\*/|\bdeclare\b|\bwaitfor\b|%|&";

        private const RegexOptions RxOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;
        public static readonly PortalSecurity Instance = new PortalSecurity();

        private static readonly DateTime OldExpiryTime = new DateTime(1999, 1, 1);

        private static readonly Regex StripTagsRegex = new Regex("<[^<>]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex BadStatementRegex = new Regex(BadStatementExpression, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex[] RxListStrings = new[]
        {
            new Regex("<script[^>]*>.*?</script[^><]*>", RxOptions),
            new Regex("<script", RxOptions),
            new Regex("<input[^>]*>.*?</input[^><]*>", RxOptions),
            new Regex("<object[^>]*>.*?</object[^><]*>", RxOptions),
            new Regex("<embed[^>]*>.*?</embed[^><]*>", RxOptions),
            new Regex("<applet[^>]*>.*?</applet[^><]*>", RxOptions),
            new Regex("<form[^>]*>.*?</form[^><]*>", RxOptions),
            new Regex("<option[^>]*>.*?</option[^><]*>", RxOptions),
            new Regex("<select[^>]*>.*?</select[^><]*>", RxOptions),
            new Regex("<source[^>]*>.*?</source[^><]*>", RxOptions),
            new Regex("<iframe[^>]*>.*?</iframe[^><]*>", RxOptions),
            new Regex("<iframe.*?<", RxOptions),
            new Regex("<iframe.*?", RxOptions),
            new Regex("<ilayer[^>]*>.*?</ilayer[^><]*>", RxOptions),
            new Regex("<form[^>]*>", RxOptions),
            new Regex("</form[^><]*>", RxOptions),
            new Regex("\bonerror\b", RxOptions),
            new Regex("\bonload\b", RxOptions),
            new Regex("\bonfocus\b", RxOptions),
            new Regex("\bonblur\b", RxOptions),
            new Regex("\bonclick\b", RxOptions),
            new Regex("\bondblclick\b", RxOptions),
            new Regex("\bonchange\b", RxOptions),
            new Regex("\bonselect\b", RxOptions),
            new Regex("\bonsubmit\b", RxOptions),
            new Regex("\bonreset\b", RxOptions),
            new Regex("\bonkeydown\b", RxOptions),
            new Regex("\bonkeyup\b", RxOptions),
            new Regex("\bonkeypress\b", RxOptions),
            new Regex("\bonmousedown\b", RxOptions),
            new Regex("\bonmousemove\b", RxOptions),
            new Regex("\bonmouseout\b", RxOptions),
            new Regex("\bonmouseover\b", RxOptions),
            new Regex("\bonmouseup\b", RxOptions),
            new Regex("\bonreadystatechange\b", RxOptions),
            new Regex("\bonfinish\b", RxOptions),
            new Regex("javascript:", RxOptions),
            new Regex("vbscript:", RxOptions),
            new Regex("unescape", RxOptions),
            new Regex("alert[\\s(&nbsp;)]*\\([\\s(&nbsp;)]*'?[\\s(&nbsp;)]*[\"(&quot;)]?", RxOptions),
            new Regex(@"eval*.\(", RxOptions),
        };

        private static readonly Regex DangerElementsRegex = new Regex(@"(<[^>]*?) on.*?\=(['""]*)[\s\S]*?(\2)( *)([^>]*?>)", RxOptions);
        private static readonly Regex DangerElementContentRegex = new Regex(@"on.*?\=(['""]*)[\s\S]*?(\1)( *)", RxOptions);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The FilterFlag enum determines which filters are applied by the InputFilter
        /// function.  The Flags attribute allows the user to include multiple
        /// enumerated values in a single variable by OR'ing the individual values
        /// together.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Flags]
        public enum FilterFlag
        {
            MultiLine = 1,
            NoMarkup = 2,
            NoScripting = 4,
            NoSQL = 8,
            NoAngleBrackets = 16,
            NoProfanity = 32,
        }

        /// <summary>
        /// Determines the configuration source for the remove and replace functions.
        /// </summary>
        public enum ConfigType
        {
            ListController,
            ExternalFile,
        }

        /// <summary>
        /// determines whether to use system (host) list, portal specific list, or combine both
        /// At present only supported by ConfigType.ListController.
        /// </summary>
        public enum FilterScope
        {
            SystemList,
            PortalList,
            SystemAndPortalList,
        }

        private enum RoleType
        {
            Security,
            Friend,
            Follower,
            Owner,
        }

        public static void ForceSecureConnection()
        {
            // get current url
            var url = HttpContext.Current.Request.Url.ToString();

            // if unsecure connection
            if (url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                // switch to secure connection
                url = "https://" + url.Substring("http://".Length);

                // append ssl parameter to querystring to indicate secure connection processing has already occurred
                if (url.IndexOf("?", StringComparison.Ordinal) == -1)
                {
                    url = url + "?ssl=1";
                }
                else
                {
                    url = url + "&ssl=1";
                }

                // redirect to secure connection
                HttpContext.Current.Response.Redirect(url, true);
            }
        }

        public static string GetCookieDomain(int portalId)
        {
            string cookieDomain = string.Empty;
            if (PortalController.IsMemberOfPortalGroup(portalId))
            {
                // set cookie domain for portal group
                var groupController = new PortalGroupController();
                var group = groupController.GetPortalGroups().SingleOrDefault(p => p.MasterPortalId == PortalController.GetEffectivePortalId(portalId));

                if (@group != null
                        && !string.IsNullOrEmpty(@group.AuthenticationDomain)
                        && PortalSettings.Current.PortalAlias.HTTPAlias.Contains(@group.AuthenticationDomain))
                {
                    cookieDomain = @group.AuthenticationDomain;
                }

                if (string.IsNullOrEmpty(cookieDomain))
                {
                    cookieDomain = FormsAuthentication.CookieDomain;
                }
            }
            else
            {
                // set cookie domain to be consistent with domain specification in web.config
                cookieDomain = FormsAuthentication.CookieDomain;
            }

            return cookieDomain;
        }

        public static bool IsDenied(string roles)
        {
            UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
            return IsDenied(objUserInfo, settings, roles);
        }

        public static bool IsDenied(UserInfo objUserInfo, PortalSettings settings, string roles)
        {
            // super user always has full access
            if (objUserInfo.IsSuperUser)
            {
                return false;
            }

            bool isDenied = false;

            if (roles != null)
            {
                // permissions strings are encoded with Deny permissions at the beginning and Grant permissions at the end for optimal performance
                foreach (string role in roles.Split(new[] { ';' }))
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        // Deny permission
                        if (role.StartsWith("!"))
                        {
                            // Portal Admin cannot be denied from his/her portal (so ignore deny permissions if user is portal admin)
                            if (settings != null && !(settings.PortalId == objUserInfo.PortalID && objUserInfo.IsInRole(settings.AdministratorRoleName)))
                            {
                                string denyRole = role.Replace("!", string.Empty);
                                if (denyRole == Globals.glbRoleAllUsersName || objUserInfo.IsInRole(denyRole))
                                {
                                    isDenied = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return isDenied;
        }

        public static bool IsInRole(string role)
        {
            if (!string.IsNullOrEmpty(role) && role == Globals.glbRoleUnauthUserName && !HttpContext.Current.Request.IsAuthenticated)
            {
                return true;
            }

            return IsInRoles(UserController.Instance.GetCurrentUserInfo(), PortalController.Instance.GetCurrentPortalSettings(), role);
        }

        public static bool IsInRoles(string roles)
        {
            UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
            return IsInRoles(objUserInfo, settings, roles);
        }

        public static bool IsInRoles(UserInfo objUserInfo, PortalSettings settings, string roles)
        {
            // super user always has full access
            bool isInRoles = objUserInfo.IsSuperUser;

            if (!isInRoles)
            {
                if (roles != null)
                {
                    foreach (string role in roles.Split(new[] { ';' }))
                    {
                        bool? roleAllowed;
                        ProcessRole(objUserInfo, settings, role, out roleAllowed);
                        if (roleAllowed.HasValue)
                        {
                            isInRoles = roleAllowed.Value;
                            break;
                        }
                    }
                }
            }

            return isInRoles;
        }

        public static bool IsFriend(int userId)
        {
            UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
            return IsInRoles(objUserInfo, settings, RoleFriendPrefix + userId);
        }

        public static bool IsFollower(int userId)
        {
            UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
            return IsInRoles(objUserInfo, settings, RoleFollowerPrefix + userId);
        }

        public static bool IsOwner(int userId)
        {
            UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
            return IsInRoles(objUserInfo, settings, RoleOwnerPrefix + userId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This function creates a random key.
        /// </summary>
        /// <param name="numBytes">This is the number of bytes for the key.</param>
        /// <returns>A random string.</returns>
        /// <remarks>
        /// This is a public function used for generating SHA1 keys.
        /// </remarks>
        public string CreateKey(int numBytes)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var buff = new byte[numBytes];
                rng.GetBytes(buff);
                return BytesToHexString(buff);
            }
        }

        public string Decrypt(string strKey, string strData)
        {
            return CryptographyProvider.Instance().DecryptParameter(strData, strKey);
        }

        public string DecryptString(string message, string passphrase)
        {
            return CryptographyProvider.Instance().DecryptString(message, passphrase);
        }

        public string Encrypt(string key, string data)
        {
            return CryptographyProvider.Instance().EncryptParameter(data, key);
        }

        public string EncryptString(string message, string passphrase)
        {
            return CryptographyProvider.Instance().EncryptString(message, passphrase);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This function applies security filtering to the UserInput string.
        /// </summary>
        /// <param name="userInput">This is the string to be filtered.</param>
        /// <param name="filterType">Flags which designate the filters to be applied.</param>
        /// <returns>Filtered UserInput.</returns>
        /// -----------------------------------------------------------------------------
        public string InputFilter(string userInput, FilterFlag filterType)
        {
            if (userInput == null)
            {
                return string.Empty;
            }

            var tempInput = userInput;
            if ((filterType & FilterFlag.NoAngleBrackets) == FilterFlag.NoAngleBrackets)
            {
                var removeAngleBrackets = Config.GetSetting("RemoveAngleBrackets") != null && bool.Parse(Config.GetSetting("RemoveAngleBrackets"));
                if (removeAngleBrackets)
                {
                    tempInput = FormatAngleBrackets(tempInput);
                }
            }

            if ((filterType & FilterFlag.NoSQL) == FilterFlag.NoSQL)
            {
                tempInput = FormatRemoveSQL(tempInput);
            }

            if ((filterType & FilterFlag.NoMarkup) == FilterFlag.NoMarkup && IncludesMarkup(tempInput))
            {
                tempInput = HttpUtility.HtmlEncode(tempInput);
            }

            if ((filterType & FilterFlag.NoScripting) == FilterFlag.NoScripting)
            {
                tempInput = this.FormatDisableScripting(tempInput);
            }

            if ((filterType & FilterFlag.MultiLine) == FilterFlag.MultiLine)
            {
                tempInput = FormatMultiLine(tempInput);
            }

            if ((filterType & FilterFlag.NoProfanity) == FilterFlag.NoProfanity)
            {
                tempInput = this.Replace(tempInput, ConfigType.ListController, "ProfanityFilter", FilterScope.SystemAndPortalList);
            }

            return tempInput;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Replaces profanity words with other words in the provided input string.
        /// </summary>
        /// <remarks>
        /// The correspondence between the words to search and the words to replace could be specified in two different places:
        /// 1) In an external file. (NOT IMPLEMENTED)
        /// 2) In System/Site lists.
        /// The name of the System List is "ProfanityFilter". The name of the list in each portal is composed using the following rule:
        /// "ProfanityFilter-" + PortalID.
        /// </remarks>
        /// <param name="inputString">The string to search the words in.</param>
        /// <param name="configType">The type of configuration.</param>
        /// <param name="configSource">The external file to search the words. Ignored when configType is ListController.</param>
        /// <param name="filterScope">When using ListController configType, this parameter indicates which list(s) to use.</param>
        /// <returns>The original text with the profanity words replaced.</returns>
        /// -----------------------------------------------------------------------------
        public string Replace(string inputString, ConfigType configType, string configSource, FilterScope filterScope)
        {
            switch (configType)
            {
                case ConfigType.ListController:
                    const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
                    const string listName = "ProfanityFilter";

                    var listController = new ListController();

                    PortalSettings settings;

                    IEnumerable<ListEntryInfo> listEntryHostInfos;
                    IEnumerable<ListEntryInfo> listEntryPortalInfos;

                    switch (filterScope)
                    {
                        case FilterScope.SystemList:
                            listEntryHostInfos = listController.GetListEntryInfoItems(listName, string.Empty, Null.NullInteger);
                            inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", removeItem.Value, options));
                            break;
                        case FilterScope.SystemAndPortalList:
                            settings = PortalController.Instance.GetCurrentPortalSettings();
                            listEntryHostInfos = listController.GetListEntryInfoItems(listName, string.Empty, Null.NullInteger);
                            listEntryPortalInfos = listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, string.Empty, settings.PortalId);
                            inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", removeItem.Value, options));
                            inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", removeItem.Value, options));
                            break;
                        case FilterScope.PortalList:
                            settings = PortalController.Instance.GetCurrentPortalSettings();
                            listEntryPortalInfos = listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, string.Empty, settings.PortalId);
                            inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", removeItem.Value, options));
                            break;
                    }

                    break;
                case ConfigType.ExternalFile:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException("configType");
            }

            return inputString;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Removes profanity words in the provided input string.
        /// </summary>
        /// <remarks>
        /// The words to search could be defined in two different places:
        /// 1) In an external file. (NOT IMPLEMENTED)
        /// 2) In System/Site lists.
        /// The name of the System List is "ProfanityFilter". The name of the list in each portal is composed using the following rule:
        /// "ProfanityFilter-" + PortalID.
        /// </remarks>
        /// <param name="inputString">The string to search the words in.</param>
        /// <param name="configType">The type of configuration.</param>
        /// <param name="configSource">The external file to search the words. Ignored when configType is ListController.</param>
        /// <param name="filterScope">When using ListController configType, this parameter indicates which list(s) to use.</param>
        /// <returns>The original text with the profanity words removed.</returns>
        /// -----------------------------------------------------------------------------
        public string Remove(string inputString, ConfigType configType, string configSource, FilterScope filterScope)
        {
            switch (configType)
            {
                case ConfigType.ListController:
                    const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
                    const string listName = "ProfanityFilter";

                    var listController = new ListController();

                    PortalSettings settings;

                    IEnumerable<ListEntryInfo> listEntryHostInfos;
                    IEnumerable<ListEntryInfo> listEntryPortalInfos;

                    switch (filterScope)
                    {
                        case FilterScope.SystemList:
                            listEntryHostInfos = listController.GetListEntryInfoItems(listName, string.Empty, Null.NullInteger);
                            inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", string.Empty, options));
                            break;
                        case FilterScope.SystemAndPortalList:
                            settings = PortalController.Instance.GetCurrentPortalSettings();
                            listEntryHostInfos = listController.GetListEntryInfoItems(listName, string.Empty, Null.NullInteger);
                            listEntryPortalInfos = listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, string.Empty, settings.PortalId);
                            inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", string.Empty, options));
                            inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", string.Empty, options));
                            break;
                        case FilterScope.PortalList:
                            settings = PortalController.Instance.GetCurrentPortalSettings();
                            listEntryPortalInfos = listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, string.Empty, settings.PortalId);
                            inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + Regex.Escape(removeItem.Text) + @"\b", string.Empty, options));
                            break;
                    }

                    break;
                case ConfigType.ExternalFile:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException("configType");
            }

            return inputString;
        }

        public void SignIn(UserInfo user, bool createPersistentCookie)
        {
            if (PortalController.IsMemberOfPortalGroup(user.PortalID) || createPersistentCookie)
            {
                // Create a custom auth cookie

                // first, create the authentication ticket
                var authenticationTicket = createPersistentCookie
                    ? new FormsAuthenticationTicket(user.Username, true, Config.GetPersistentCookieTimeout())
                    : new FormsAuthenticationTicket(user.Username, false, Config.GetAuthCookieTimeout());

                // encrypt it
                var encryptedAuthTicket = FormsAuthentication.Encrypt(authenticationTicket);

                // Create a new Cookie
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedAuthTicket)
                {
                    Expires = authenticationTicket.Expiration,
                    Domain = GetCookieDomain(user.PortalID),
                    Path = FormsAuthentication.FormsCookiePath,
                    Secure = FormsAuthentication.RequireSSL,
                };

                if (HttpContext.Current.Response.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    HttpContext.Current.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
                }

                HttpContext.Current.Response.Cookies.Set(authCookie);
                AuthCookieController.Instance.Update(authCookie.Value, authCookie.Expires.ToUniversalTime(), user.UserID);

                if (PortalController.IsMemberOfPortalGroup(user.PortalID))
                {
                    var domain = GetCookieDomain(user.PortalID);
                    var siteGroupCookie = new HttpCookie("SiteGroup", domain)
                    {
                        Expires = authenticationTicket.Expiration,
                        Domain = domain,
                        Path = FormsAuthentication.FormsCookiePath,
                        Secure = FormsAuthentication.RequireSSL,
                    };

                    HttpContext.Current.Response.Cookies.Set(siteGroupCookie);
                }
            }
            else
            {
                if (HttpContext.Current.Response.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    HttpContext.Current.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
                }

                FormsAuthentication.SetAuthCookie(user.Username, false);
                var authCookie = HttpContext.Current.Response.Cookies[FormsAuthentication.FormsCookieName];
                if (!string.IsNullOrEmpty(authCookie?.Value))
                {
                    var t = FormsAuthentication.Decrypt(authCookie.Value);
                    if (t != null)
                    {
                        AuthCookieController.Instance.Update(authCookie.Value, t.Expiration.ToUniversalTime(), user.UserID);
                    }
                }
            }

            if (user.IsSuperUser)
            {
                // save userinfo object in context to ensure Personalization is saved correctly
                HttpContext.Current.Items["UserInfo"] = user;
            }

            // Identity the Login is processed by system.
            HttpContext.Current.Items["DNN_UserSignIn"] = true;
        }

        public void SignOut()
        {
            InvalidateAspNetSession(HttpContext.Current);

            var currentAuthCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (currentAuthCookie != null)
            {
                // This will prevent next requests from being authenticated if using smae cookie
                AuthCookieController.Instance.Update(currentAuthCookie.Value, OldExpiryTime, Null.NullInteger);
            }

            // Log User Off from Cookie Authentication System
            var domainCookie = HttpContext.Current.Request.Cookies["SiteGroup"];
            if (domainCookie == null)
            {
                // Forms Authentication's Logout
                FormsAuthentication.SignOut();
            }
            else
            {
                // clear custom domain cookie
                var domain = domainCookie.Value;

                // Create a new Cookie
                var str = string.Empty;
                if (HttpContext.Current.Request.Browser["supportsEmptyStringInCookieValue"] == "false")
                {
                    str = "NoCookie";
                }

                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, str)
                {
                    Expires = OldExpiryTime,
                    Domain = domain,
                    Path = FormsAuthentication.FormsCookiePath,
                    Secure = FormsAuthentication.RequireSSL,
                };

                HttpContext.Current.Response.Cookies.Set(authCookie);

                var siteGroupCookie = new HttpCookie("SiteGroup", str)
                {
                    Expires = OldExpiryTime,
                    Domain = domain,
                    Path = FormsAuthentication.FormsCookiePath,
                    Secure = FormsAuthentication.RequireSSL,
                };

                HttpContext.Current.Response.Cookies.Set(siteGroupCookie);
            }

            // Remove current userinfo from context items
            HttpContext.Current.Items.Remove("UserInfo");

            // remove language cookie
            var httpCookie = HttpContext.Current.Response.Cookies["language"];
            if (httpCookie != null)
            {
                httpCookie.Value = string.Empty;
            }

            // remove authentication type cookie
            var cookie = HttpContext.Current.Response.Cookies["authentication"];
            if (cookie != null)
            {
                cookie.Value = string.Empty;
            }

            // expire cookies
            cookie = HttpContext.Current.Response.Cookies["portalaliasid"];
            if (cookie != null)
            {
                cookie.Value = null;
                cookie.Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/";
                cookie.Expires = DateTime.Now.AddYears(-30);
            }

            cookie = HttpContext.Current.Response.Cookies["portalroles"];
            if (cookie != null)
            {
                cookie.Value = null;
                cookie.Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/";
                cookie.Expires = DateTime.Now.AddYears(-30);
            }

            // clear any authentication provider tokens that match *UserToken convention e.g FacebookUserToken ,TwitterUserToken, LiveUserToken and GoogleUserToken
            var authCookies = HttpContext.Current.Request.Cookies.AllKeys;
            foreach (var authCookie in authCookies)
            {
                if (authCookie.EndsWith("UserToken"))
                {
                    var auth = HttpContext.Current.Response.Cookies[authCookie];
                    if (auth != null)
                    {
                        auth.Value = null;
                        auth.Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/";
                        auth.Expires = DateTime.Now.AddYears(-30);
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This function applies security filtering to the UserInput string, and reports
        /// whether the input string is valid.
        /// </summary>
        /// <param name="userInput">This is the string to be filtered.</param>
        /// <param name="filterType">Flags which designate the filters to be applied.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public bool ValidateInput(string userInput, FilterFlag filterType)
        {
            string filteredInput = this.InputFilter(userInput, filterType);

            return userInput == filteredInput;
        }

        /// <summary>
        /// This function loops through every portal that has set its own AllowedExtensionWhitelist
        /// and checks that there are no extensions there that are restriced by the host
        ///
        /// The only time we should call this is if the host allowed extensions list has changed.
        /// </summary>
        /// <param name="newMasterList">Comma separated list of extensions that govern all users on this installation.</param>
        public void CheckAllPortalFileExtensionWhitelists(string newMasterList)
        {
            var masterList = new FileExtensionWhitelist(newMasterList);
            var portalSettings = Data.DataProvider.Instance().GetPortalSettingsBySetting("AllowedExtensionsWhitelist", null);
            foreach (var portalId in portalSettings.Keys)
            {
                if (!string.IsNullOrEmpty(portalSettings[portalId]))
                {
                    var portalExts = new FileExtensionWhitelist(portalSettings[portalId]);
                    var newValue = portalExts.RestrictBy(masterList).ToStorageString();
                    if (newValue != portalSettings[portalId])
                    {
                        PortalController.UpdatePortalSetting(portalId, "AllowedExtensionsWhitelist", newValue, false);
                    }
                }
            }
        }

        private static void ProcessRole(UserInfo user, PortalSettings settings, string roleName, out bool? roleAllowed)
        {
            var roleType = GetRoleType(roleName);
            switch (roleType)
            {
                case RoleType.Friend:
                    ProcessFriendRole(user, roleName, out roleAllowed);
                    break;
                case RoleType.Follower:
                    ProcessFollowerRole(user, roleName, out roleAllowed);
                    break;
                case RoleType.Owner:
                    ProcessOwnerRole(user, roleName, out roleAllowed);
                    break;
                default:
                    ProcessSecurityRole(user, settings, roleName, out roleAllowed);
                    break;
            }
        }

        private static void ProcessFriendRole(UserInfo user, string roleName, out bool? roleAllowed)
        {
            roleAllowed = null;
            var targetUser = UserController.Instance.GetUserById(user.PortalID, GetEntityFromRoleName(roleName));
            var relationShip = RelationshipController.Instance.GetFriendRelationship(user, targetUser);
            if (relationShip != null && relationShip.Status == RelationshipStatus.Accepted)
            {
                roleAllowed = true;
            }
        }

        private static void ProcessFollowerRole(UserInfo user, string roleName, out bool? roleAllowed)
        {
            roleAllowed = null;
            var targetUser = UserController.Instance.GetUserById(user.PortalID, GetEntityFromRoleName(roleName));
            var relationShip = RelationshipController.Instance.GetFollowerRelationship(user, targetUser);
            if (relationShip != null && relationShip.Status == RelationshipStatus.Accepted)
            {
                roleAllowed = true;
            }
        }

        private static void ProcessOwnerRole(UserInfo user, string roleName, out bool? roleAllowed)
        {
            roleAllowed = null;
            var entityId = GetEntityFromRoleName(roleName);
            if (entityId == user.UserID)
            {
                roleAllowed = true;
            }
        }

        private static int GetEntityFromRoleName(string roleName)
        {
            var roleParts = roleName.Split(':');
            int result;
            if (roleParts.Length > 1 && int.TryParse(roleParts[1], out result))
            {
                return result;
            }

            return Null.NullInteger;
        }

        private static void ProcessSecurityRole(UserInfo user, PortalSettings settings, string roleName, out bool? roleAllowed)
        {
            roleAllowed = null;

            // permissions strings are encoded with Deny permissions at the beginning and Grant permissions at the end for optimal performance
            if (!string.IsNullOrEmpty(roleName))
            {
                // Deny permission
                if (roleName.StartsWith("!"))
                {
                    // Portal Admin cannot be denied from his/her portal (so ignore deny permissions if user is portal admin)
                    if (settings != null && !(settings.PortalId == user.PortalID && user.IsInRole(settings.AdministratorRoleName)))
                    {
                        string denyRole = roleName.Replace("!", string.Empty);
                        if (denyRole == Globals.glbRoleAllUsersName || user.IsInRole(denyRole))
                        {
                            roleAllowed = false;
                        }
                    }
                }
                else // Grant permission
                {
                    if (roleName == Globals.glbRoleAllUsersName || user.IsInRole(roleName))
                    {
                        roleAllowed = true;
                    }
                }
            }
        }

        private static RoleType GetRoleType(string roleName)
        {
            if (roleName.StartsWith(RoleFriendPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return RoleType.Friend;
            }

            if (roleName.StartsWith(RoleFollowerPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return RoleType.Follower;
            }

            if (roleName.StartsWith(RoleOwnerPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return RoleType.Owner;
            }

            return RoleType.Security;
        }

        private static string BytesToHexString(IEnumerable<byte> bytes)
        {
            var hexString = new StringBuilder();
            foreach (var b in bytes)
            {
                hexString.Append(string.Format("{0:X2}", b));
            }

            return hexString.ToString();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This function uses Regex search strings to remove HTML tags which are
        /// targeted in Cross-site scripting (XSS) attacks.  This function will evolve
        /// to provide more robust checking as additional holes are found.
        /// </summary>
        /// <param name="strInput">This is the string to be filtered.</param>
        /// <returns>Filtered UserInput.</returns>
        /// <remarks>
        /// This is a private function that is used internally by the FormatDisableScripting function.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private static string FilterStrings(string strInput)
        {
            // setup up list of search terms as items may be used twice
            var tempInput = strInput;
            if (string.IsNullOrEmpty(tempInput))
            {
                return tempInput;
            }

            const string replacement = " ";

            // remove the js event from html tags
            var tagMatches = DangerElementsRegex.Matches(tempInput);
            foreach (Match match in tagMatches)
            {
                var tagContent = match.Value;
                var cleanTagContent = DangerElementContentRegex.Replace(tagContent, string.Empty);
                tempInput = tempInput.Replace(tagContent, cleanTagContent);
            }

            // check if text contains encoded angle brackets, if it does it we decode it to check the plain text
            if (tempInput.Contains("&gt;") || tempInput.Contains("&lt;"))
            {
                // text is encoded, so decode and try again
                tempInput = HttpUtility.HtmlDecode(tempInput);
                tempInput = RxListStrings.Aggregate(tempInput, (current, s) => s.Replace(current, replacement));

                // Re-encode
                tempInput = HttpUtility.HtmlEncode(tempInput);
            }
            else
            {
                tempInput = RxListStrings.Aggregate(tempInput, (current, s) => s.Replace(current, replacement));
            }

            return tempInput;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This filter removes angle brackets i.e.
        /// </summary>
        /// <param name="strInput">This is the string to be filtered.</param>
        /// <returns>Filtered UserInput.</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private static string FormatAngleBrackets(string strInput)
        {
            var tempInput = new StringBuilder(strInput).Replace("<", string.Empty).Replace(">", string.Empty);
            return tempInput.ToString();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This filter removes CrLf characters and inserts br.
        /// </summary>
        /// <param name="strInput">This is the string to be filtered.</param>
        /// <returns>Filtered UserInput.</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private static string FormatMultiLine(string strInput)
        {
            const string lbreak = "<br />";
            var tempInput = new StringBuilder(strInput).Replace("\r\n", lbreak).Replace("\n", lbreak).Replace("\r", lbreak);
            return tempInput.ToString();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This function verifies raw SQL statements to prevent SQL injection attacks
        /// and replaces a similar function (PreventSQLInjection) from the Common.Globals.vb module.
        /// </summary>
        /// <param name="strSQL">This is the string to be filtered.</param>
        /// <returns>Filtered UserInput.</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private static string FormatRemoveSQL(string strSQL)
        {
            // Check for forbidden T-SQL commands. Use word boundaries to filter only real statements.
            return BadStatementRegex.Replace(strSQL, " ").Replace("'", "''");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This function determines if the Input string contains any markup.
        /// </summary>
        /// <param name="strInput">This is the string to be checked.</param>
        /// <returns>True if string contains Markup tag(s).</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private static bool IncludesMarkup(string strInput)
        {
            return StripTagsRegex.IsMatch(strInput);
        }

        private static void InvalidateAspNetSession(HttpContext context)
        {
            if (context.Session != null && !context.Session.IsNewSession)
            {
                // invalidate existing session so a new one is created
                context.Session.Clear();
                context.Session.Abandon();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This function uses Regex search strings to remove HTML tags which are
        /// targeted in Cross-site scripting (XSS) attacks.  This function will evolve
        /// to provide more robust checking as additional holes are found.
        /// </summary>
        /// <param name="strInput">This is the string to be filtered.</param>
        /// <returns>Filtered UserInput.</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private string FormatDisableScripting(string strInput)
        {
            return string.IsNullOrWhiteSpace(strInput)
                ? strInput
                : FilterStrings(strInput);
        }
    }
}
