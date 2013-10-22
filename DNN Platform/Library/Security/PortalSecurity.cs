#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;

using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Personalization;

#endregion

namespace DotNetNuke.Security
{
    public class PortalSecurity
    {
        #region FilterFlag enum

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// The FilterFlag enum determines which filters are applied by the InputFilter
        /// function.  The Flags attribute allows the user to include multiple
        /// enumerated values in a single variable by OR'ing the individual values
        /// together.
        /// </summary>
        ///-----------------------------------------------------------------------------
        [Flags]
        public enum FilterFlag
        {
            MultiLine = 1,
            NoMarkup = 2,
            NoScripting = 4,
            NoSQL = 8,
            NoAngleBrackets = 16,
            NoProfanity =32
        }

        /// <summary>
        /// Determines the configuration source for the remove and replace functions
        /// </summary>
        public enum ConfigType
        {
            ListController,
            ExternalFile
        }

        /// <summary>
        /// determines whether to use system (host) list, portal specific list, or combine both
        /// At present only supported by ConfigType.ListController
        /// </summary>
        public enum FilterScope
        {
            SystemList,
            PortalList,
            SystemAndPortalList
        }

        #endregion
		
		#region Private Methods

        private string BytesToHexString(byte[] bytes)
        {
            var hexString = new StringBuilder(64);
            int counter;
            for (counter = 0; counter <= bytes.Length - 1; counter++)
            {
                hexString.Append(String.Format("{0:X2}", bytes[counter]));
            }
            return hexString.ToString();
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function uses Regex search strings to remove HTML tags which are
        /// targeted in Cross-site scripting (XSS) attacks.  This function will evolve
        /// to provide more robust checking as additional holes are found.
        /// </summary>
        /// <param name="strInput">This is the string to be filtered</param>
        /// <returns>Filtered UserInput</returns>
        /// <remarks>
        /// This is a private function that is used internally by the FormatDisableScripting function
        /// </remarks>
        /// <history>
        ///     [cathal]        3/06/2007   Created
        /// </history>
        ///-----------------------------------------------------------------------------
        private string FilterStrings(string strInput)
        {
			//setup up list of search terms as items may be used twice
            string TempInput = strInput;
            var listStrings = new List<string>
                                  {
                                      "<script[^>]*>.*?</script[^><]*>",
                                      "<script",
                                      "<input[^>]*>.*?</input[^><]*>",
                                      "<object[^>]*>.*?</object[^><]*>",
                                      "<embed[^>]*>.*?</embed[^><]*>",
                                      "<applet[^>]*>.*?</applet[^><]*>",
                                      "<form[^>]*>.*?</form[^><]*>",
                                      "<option[^>]*>.*?</option[^><]*>",
                                      "<select[^>]*>.*?</select[^><]*>",
                                      "<iframe[^>]*>.*?</iframe[^><]*>",
                                      "<iframe.*?<",
                                      "<iframe.*?",
                                      "<ilayer[^>]*>.*?</ilayer[^><]*>",
                                      "<form[^>]*>",
                                      "</form[^><]*>",
                                      "onerror",
                                      "onmouseover",
                                      "javascript:",
                                      "vbscript:",
                                      "unescape",
                                      "alert[\\s(&nbsp;)]*\\([\\s(&nbsp;)]*'?[\\s(&nbsp;)]*[\"(&quot;)]?"
                                  };

            const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
            const string replacement = " ";

            //check if text contains encoded angle brackets, if it does it we decode it to check the plain text
            if (TempInput.Contains("&gt;") && TempInput.Contains("&lt;"))
            {
				//text is encoded, so decode and try again
                TempInput = HttpContext.Current.Server.HtmlDecode(TempInput);
                TempInput = listStrings.Aggregate(TempInput, (current, s) => Regex.Replace(current, s, replacement, options));

                //Re-encode
                TempInput = HttpContext.Current.Server.HtmlEncode(TempInput);
            }
            else
            {
                TempInput = listStrings.Aggregate(TempInput, (current, s) => Regex.Replace(current, s, replacement, options));
            }
            return TempInput;
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function uses Regex search strings to remove HTML tags which are
        /// targeted in Cross-site scripting (XSS) attacks.  This function will evolve
        /// to provide more robust checking as additional holes are found.
        /// </summary>
        /// <param name="strInput">This is the string to be filtered</param>
        /// <returns>Filtered UserInput</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function
        /// </remarks>
        ///-----------------------------------------------------------------------------
        private string FormatDisableScripting(string strInput)
        {
            string TempInput = strInput;
            TempInput = FilterStrings(TempInput);
            return TempInput;
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This filter removes angle brackets i.e.
        /// </summary>
        /// <param name="strInput">This is the string to be filtered</param>
        /// <returns>Filtered UserInput</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function
        /// </remarks>
        /// <history>
        /// 	[Cathal] 	6/1/2006	Created to fufill client request
        /// </history>
        ///-----------------------------------------------------------------------------
        private string FormatAngleBrackets(string strInput)
        {
            string TempInput = strInput.Replace("<", "");
            TempInput = TempInput.Replace(">", "");
            return TempInput;
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This filter removes CrLf characters and inserts br
        /// </summary>
        /// <param name="strInput">This is the string to be filtered</param>
        /// <returns>Filtered UserInput</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function
        /// </remarks>
        ///-----------------------------------------------------------------------------
        private string FormatMultiLine(string strInput)
        {
            string tempInput = strInput.Replace(Environment.NewLine, "<br />").Replace("\r\n", "<br />").Replace("\n", "<br />").Replace("\r", "<br />");
            return (tempInput);
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function verifies raw SQL statements to prevent SQL injection attacks
        /// and replaces a similar function (PreventSQLInjection) from the Common.Globals.vb module
        /// </summary>
        /// <param name="strSQL">This is the string to be filtered</param>
        /// <returns>Filtered UserInput</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function
        /// </remarks>
        ///-----------------------------------------------------------------------------
        private string FormatRemoveSQL(string strSQL)
        {
            const string BadStatementExpression = ";|--|create|drop|select|insert|delete|update|union|sp_|xp_|exec|/\\*.*\\*/|declare|waitfor|%|&";
            return Regex.Replace(strSQL, BadStatementExpression, " ", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace("'", "''");
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function determines if the Input string contains any markup.
        /// </summary>
        /// <param name="strInput">This is the string to be checked</param>
        /// <returns>True if string contains Markup tag(s)</returns>
        /// <remarks>
        /// This is a private function that is used internally by the InputFilter function
        /// </remarks>
        ///-----------------------------------------------------------------------------
        private bool IncludesMarkup(string strInput)
        {
            const RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
            const string pattern = "<[^<>]*>";
            return Regex.IsMatch(strInput, pattern, options);
        }
		
		#endregion
		
		#region Public Methods

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function creates a random key
        /// </summary>
        /// <param name="numBytes">This is the number of bytes for the key</param>
        /// <returns>A random string</returns>
        /// <remarks>
        /// This is a public function used for generating SHA1 keys
        /// </remarks>
        /// <history>
        /// </history>
        ///-----------------------------------------------------------------------------
        public string CreateKey(int numBytes)
        {
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[numBytes];
            rng.GetBytes(buff);
            return BytesToHexString(buff);
        }

        public string Decrypt(string strKey, string strData)
        {
            if (String.IsNullOrEmpty(strData))
            {
                return "";
            }
            string strValue = "";
            if (!String.IsNullOrEmpty(strKey))
            {
				//convert key to 16 characters for simplicity
                if (strKey.Length < 16)
                {
                    strKey = strKey + "XXXXXXXXXXXXXXXX".Substring(0, 16 - strKey.Length);
                }
                else
                {
                    strKey = strKey.Substring(0, 16);
                }
				
                //create encryption keys
                byte[] byteKey = Encoding.UTF8.GetBytes(strKey.Substring(0, 8));
                byte[] byteVector = Encoding.UTF8.GetBytes(strKey.Substring(strKey.Length - 8, 8));

                //convert data to byte array and Base64 decode
                var byteData = new byte[strData.Length];
                try
                {
                    byteData = Convert.FromBase64String(strData);
                }
                catch //invalid length
                {
                    strValue = strData;
                }
                if (String.IsNullOrEmpty(strValue))
                {
                    try
                    {
						//decrypt
                        var objDES = new DESCryptoServiceProvider();
                        var objMemoryStream = new MemoryStream();
                        var objCryptoStream = new CryptoStream(objMemoryStream, objDES.CreateDecryptor(byteKey, byteVector), CryptoStreamMode.Write);
                        objCryptoStream.Write(byteData, 0, byteData.Length);
                        objCryptoStream.FlushFinalBlock();

                        //convert to string
                        Encoding objEncoding = Encoding.UTF8;
                        strValue = objEncoding.GetString(objMemoryStream.ToArray());
                    }
                    catch //decryption error
                    {
                        strValue = "";
                    }
                }
            }
            else
            {
                strValue = strData;
            }
            return strValue;
        }

        public string DecryptString(string message, string passphrase)
        {
            byte[] results;
            var utf8 = new UTF8Encoding();

            //hash the passphrase using MD5 to create 128bit byte array
            var hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(utf8.GetBytes(passphrase));

            var tdesAlgorithm = new TripleDESCryptoServiceProvider {Key = tdesKey, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7};


            byte[] dataToDecrypt = Convert.FromBase64String(message);
            try
            {
                ICryptoTransform decryptor = tdesAlgorithm.CreateDecryptor();
                results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information 
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            return utf8.GetString(results);
        }

        public string Encrypt(string key, string data)
        {
            string value;
            if (!String.IsNullOrEmpty(key))
            {
                //convert key to 16 characters for simplicity
                if (key.Length < 16)
                {
                    key = key + "XXXXXXXXXXXXXXXX".Substring(0, 16 - key.Length);
                }
                else
                {
                    key = key.Substring(0, 16);
                }
				
                //create encryption keys
                byte[] byteKey = Encoding.UTF8.GetBytes(key.Substring(0, 8));
                byte[] byteVector = Encoding.UTF8.GetBytes(key.Substring(key.Length - 8, 8));

                //convert data to byte array
                byte[] byteData = Encoding.UTF8.GetBytes(data);

                //encrypt 
                var objDES = new DESCryptoServiceProvider();
                var objMemoryStream = new MemoryStream();
                var objCryptoStream = new CryptoStream(objMemoryStream, objDES.CreateEncryptor(byteKey, byteVector), CryptoStreamMode.Write);
                objCryptoStream.Write(byteData, 0, byteData.Length);
                objCryptoStream.FlushFinalBlock();

                //convert to string and Base64 encode
                value = Convert.ToBase64String(objMemoryStream.ToArray());
            }
            else
            {
                value = data;
            }
            return value;
        }

        public string EncryptString(string message, string passphrase)
        {
            byte[] results;
            var utf8 = new UTF8Encoding();

            //hash the passphrase using MD5 to create 128bit byte array
            var hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(utf8.GetBytes(passphrase));

            var tdesAlgorithm = new TripleDESCryptoServiceProvider {Key = tdesKey, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7};


            byte[] dataToEncrypt = utf8.GetBytes(message);

            try
            {
                ICryptoTransform encryptor = tdesAlgorithm.CreateEncryptor();
                results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information 
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            //Return the encrypted string as a base64 encoded string 
            return Convert.ToBase64String(results);
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function applies security filtering to the UserInput string.
        /// </summary>
        /// <param name="userInput">This is the string to be filtered</param>
        /// <param name="filterType">Flags which designate the filters to be applied</param>
        /// <returns>Filtered UserInput</returns>
        ///-----------------------------------------------------------------------------
        public string InputFilter(string userInput, FilterFlag filterType)
        {
            if (userInput == null)
            {
                return "";
            }
            var tempInput = userInput;
            if ((filterType & FilterFlag.NoAngleBrackets) == FilterFlag.NoAngleBrackets)
            {
                var removeAngleBrackets = Config.GetSetting("RemoveAngleBrackets") != null && Boolean.Parse(Config.GetSetting("RemoveAngleBrackets"));
                if (removeAngleBrackets)
                {
                    tempInput = FormatAngleBrackets(tempInput);
                }
            }
            if ((filterType & FilterFlag.NoSQL) == FilterFlag.NoSQL)
            {
                tempInput = FormatRemoveSQL(tempInput);
            }
            else
            {
                if ((filterType & FilterFlag.NoMarkup) == FilterFlag.NoMarkup && IncludesMarkup(tempInput))
                {
                    tempInput = HttpUtility.HtmlEncode(tempInput);
                }
                if ((filterType & FilterFlag.NoScripting) == FilterFlag.NoScripting)
                {
                    tempInput = FormatDisableScripting(tempInput);
                }
                if ((filterType & FilterFlag.MultiLine) == FilterFlag.MultiLine)
                {
                    tempInput = FormatMultiLine(tempInput);
                }
            }
            if ((filterType & FilterFlag.NoProfanity) == FilterFlag.NoProfanity)
            {
                tempInput = Replace(tempInput, ConfigType.ListController, "ProfanityFilter", FilterScope.SystemAndPortalList);
            }
            return tempInput;
        }

        ///-----------------------------------------------------------------------------
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
        ///-----------------------------------------------------------------------------
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
                            listEntryHostInfos = listController.GetListEntryInfoItems(listName, "", Null.NullInteger);
                            inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + removeItem.Text + @"\b", removeItem.Value, options));
                            break;
                        case FilterScope.SystemAndPortalList:
                            settings = PortalController.GetCurrentPortalSettings();
                            listEntryHostInfos = listController.GetListEntryInfoItems(listName, "", Null.NullInteger);
                            listEntryPortalInfos = listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, "", settings.PortalId);
                            inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + removeItem.Text + @"\b", removeItem.Value, options));
                            inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + removeItem.Text + @"\b", removeItem.Value, options));
                            break;
                        case FilterScope.PortalList:
                            settings = PortalController.GetCurrentPortalSettings();
                            listEntryPortalInfos = listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, "", settings.PortalId);
                            inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + removeItem.Text + @"\b", removeItem.Value, options));
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

        ///-----------------------------------------------------------------------------
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
        ///-----------------------------------------------------------------------------
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
                            listEntryHostInfos = listController.GetListEntryInfoItems(listName, "", Null.NullInteger);
                            inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + removeItem.Text + @"\b", string.Empty, options));
                            break;
                        case FilterScope.SystemAndPortalList:
                            settings = PortalController.GetCurrentPortalSettings();
                            listEntryHostInfos = listController.GetListEntryInfoItems(listName, "", Null.NullInteger);
                            listEntryPortalInfos = listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, "", settings.PortalId);
                            inputString = listEntryHostInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + removeItem.Text + @"\b", string.Empty, options));
                            inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + removeItem.Text + @"\b", string.Empty, options));
                            break;
                        case FilterScope.PortalList:
                            settings = PortalController.GetCurrentPortalSettings();
                            listEntryPortalInfos = listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, "", settings.PortalId);
                            inputString = listEntryPortalInfos.Aggregate(inputString, (current, removeItem) => Regex.Replace(current, @"\b" + removeItem.Text + @"\b", string.Empty, options));        
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
                //Create a custom auth cookie

                //first, create the authentication ticket     
                FormsAuthenticationTicket authenticationTicket = createPersistentCookie
                                                                     ? new FormsAuthenticationTicket(user.Username, true, Config.GetPersistentCookieTimeout())
                                                                     : new FormsAuthenticationTicket(user.Username, false, Config.GetAuthCookieTimeout());

                //encrypt it     
                var encryptedAuthTicket = FormsAuthentication.Encrypt(authenticationTicket);

                //Create a new Cookie
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedAuthTicket)
                                        {
                                            Expires = authenticationTicket.Expiration,
                                            Domain = GetCookieDomain(user.PortalID),
                                            Path = FormsAuthentication.FormsCookiePath,
                                            Secure = FormsAuthentication.RequireSSL
                                        };

                HttpContext.Current.Response.Cookies.Set(authCookie);


                if (PortalController.IsMemberOfPortalGroup(user.PortalID))
                {
                    var domain = GetCookieDomain(user.PortalID);
                    var siteGroupCookie = new HttpCookie("SiteGroup", domain)
                    {
                        Expires = authenticationTicket.Expiration,
                        Domain = domain,
                        Path = FormsAuthentication.FormsCookiePath,
                        Secure = FormsAuthentication.RequireSSL
                    };

                    HttpContext.Current.Response.Cookies.Set(siteGroupCookie);
                }
            }
            else
            {
                FormsAuthentication.SetAuthCookie(user.Username, false);
            }
            if (user.IsSuperUser)
            {
                //save userinfo object in context to ensure Personalization is saved correctly
                HttpContext.Current.Items["UserInfo"] = user;

                HostController.Instance.Update(String.Format("GettingStarted_Display_{0}", user.UserID), "true");
            }
        }

        public void SignOut()
        {
			//Log User Off from Cookie Authentication System
            var domainCookie = HttpContext.Current.Request.Cookies["SiteGroup"];
            if (domainCookie == null)
            {
                //Forms Authentication's Logout
                FormsAuthentication.SignOut();
            }
            else
            {
                //clear custom domain cookie
                var domain = domainCookie.Value;

                //Create a new Cookie
                string str = String.Empty;
                if (HttpContext.Current.Request.Browser["supportsEmptyStringInCookieValue"] == "false")
                {
                    str = "NoCookie";
                }

                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, str)
                                    {
                                        Expires = new DateTime(1999, 1, 1),
                                        Domain = domain,
                                        Path = FormsAuthentication.FormsCookiePath,
                                        Secure = FormsAuthentication.RequireSSL
                    
                                    };

                HttpContext.Current.Response.Cookies.Set(authCookie);

                var siteGroupCookie = new HttpCookie("SiteGroup", str)
                                    {
                                        Expires = new DateTime(1999, 1, 1),
                                        Domain = domain,
                                        Path = FormsAuthentication.FormsCookiePath,
                                        Secure = FormsAuthentication.RequireSSL
                                    };

                HttpContext.Current.Response.Cookies.Set(siteGroupCookie);
            }

			//Remove current userinfo from context items
			HttpContext.Current.Items.Remove("UserInfo");

            //remove language cookie
            var httpCookie = HttpContext.Current.Response.Cookies["language"];
            if (httpCookie != null)
            {
                httpCookie.Value = "";
            }

            //remove authentication type cookie
            var cookie = HttpContext.Current.Response.Cookies["authentication"];
            if (cookie != null)
            {
                cookie.Value = "";
            }

            //expire cookies
            var httpCookie1 = HttpContext.Current.Response.Cookies["portalaliasid"];
            if (httpCookie1 != null)
            {
                httpCookie1.Value = null;
                httpCookie1.Path = "/";
                httpCookie1.Expires = DateTime.Now.AddYears(-30);
            }

            var cookie1 = HttpContext.Current.Response.Cookies["portalroles"];
            if (cookie1 != null)
            {
                cookie1.Value = null;
                cookie1.Path = "/";
                cookie1.Expires = DateTime.Now.AddYears(-30);
            }

            //clear any authentication provider tokens that match *UserToken convention e.g FacebookUserToken ,TwitterUserToken, LiveUserToken and GoogleUserToken
            var authCookies = HttpContext.Current.Request.Cookies.AllKeys;
            foreach (var authCookie in authCookies)
            {
                if (authCookie.EndsWith("UserToken"))
                {
                    var auth = HttpContext.Current.Response.Cookies[authCookie];
                    if (auth != null)
                    {
                        auth.Value = null;
                        auth.Path = "/";
                        auth.Expires = DateTime.Now.AddYears(-30);
                    }
                }
            }
           
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// This function applies security filtering to the UserInput string, and reports
        /// whether the input string is valid.
        /// </summary>
        /// <param name="userInput">This is the string to be filtered</param>
        /// <param name="filterType">Flags which designate the filters to be applied</param>
        /// <returns></returns>
        ///-----------------------------------------------------------------------------
        public bool ValidateInput(string userInput, FilterFlag filterType)
        {
            string filteredInput = InputFilter(userInput, filterType);

            return (userInput == filteredInput);
        }
		
		#endregion
		
		#region Public Shared/Static Methods

        public static void ForceSecureConnection()
        {
			//get current url
            string URL = HttpContext.Current.Request.Url.ToString();
			//if unsecure connection
            if (URL.StartsWith("http://"))
            {
				//switch to secure connection
                URL = URL.Replace("http://", "https://");
                //append ssl parameter to querystring to indicate secure connection processing has already occurred
                if (URL.IndexOf("?", StringComparison.Ordinal) == -1)
                {
                    URL = URL + "?ssl=1";
                }
                else
                {
                    URL = URL + "&ssl=1";
                }
                //redirect to secure connection
                HttpContext.Current.Response.Redirect(URL, true);
            }
        }

        public static string GetCookieDomain(int portalId)
        {
            string cookieDomain = String.Empty;
            if (PortalController.IsMemberOfPortalGroup(portalId))
            {
                //set cookie domain for portal group
                var groupController = new PortalGroupController();
                var group = groupController.GetPortalGroups().SingleOrDefault(p => p.MasterPortalId == PortalController.GetEffectivePortalId(portalId));

				if (@group != null 
						&& !string.IsNullOrEmpty(@group.AuthenticationDomain)
						&& PortalSettings.Current.PortalAlias.HTTPAlias.Contains(@group.AuthenticationDomain))
                {
                    cookieDomain = @group.AuthenticationDomain;
                }

                if (String.IsNullOrEmpty(cookieDomain))
                {
                    cookieDomain = FormsAuthentication.CookieDomain;
                }
            }
            else
            {
                //set cookie domain to be consistent with domain specification in web.config
                cookieDomain = FormsAuthentication.CookieDomain;
            }


            return cookieDomain;
        }

        public static bool IsInRole(string role)
        {
            UserInfo objUserInfo = UserController.GetCurrentUserInfo();
            HttpContext context = HttpContext.Current;
            if (!String.IsNullOrEmpty(role) && ((context.Request.IsAuthenticated == false && role == Globals.glbRoleUnauthUserName)))
            {
                return true;
            }
            return objUserInfo.IsInRole(role);
        }

        public static bool IsInRoles(string roles)
        {
            UserInfo objUserInfo = UserController.GetCurrentUserInfo();
            //Portal Admin cannot be denied from his/her portal (so ignore deny permissions if user is portal admin)
            PortalSettings settings = PortalController.GetCurrentPortalSettings();
            return IsInRoles(objUserInfo, settings, roles);            
        }
        
        public static bool IsInRoles(UserInfo objUserInfo, PortalSettings settings, string roles)
        {
            //super user always has full access
            bool blnIsInRoles = objUserInfo.IsSuperUser;

            if (!blnIsInRoles)
            {
                if (roles != null)
                {
                    //permissions strings are encoded with Deny permissions at the beginning and Grant permissions at the end for optimal performance
                    foreach (string role in roles.Split(new[] { ';' }))
                    {
                        if (!String.IsNullOrEmpty(role))
                        {
                            //Deny permission
                            if (role.StartsWith("!"))
                            {
                                //Portal Admin cannot be denied from his/her portal (so ignore deny permissions if user is portal admin)
                                if (!(settings.PortalId == objUserInfo.PortalID && settings.AdministratorId == objUserInfo.UserID))
                                {
                                    string denyRole = role.Replace("!", "");
                                    if (denyRole == Globals.glbRoleAllUsersName || objUserInfo.IsInRole(denyRole))
                                    {
                                        break;
                                    }
                                }
                            }
                            else //Grant permission
                            {
                                if (role == Globals.glbRoleAllUsersName || objUserInfo.IsInRole(role))
                                {
                                    blnIsInRoles = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return blnIsInRoles;
        }
		
		#endregion
		
		#region Obsoleted Methods, retained for Binary Compatability

        [Obsolete("Deprecated in DNN 6.2 - roles cookie is no longer used)")]
        public static void ClearRoles()
        {
            var httpCookie = HttpContext.Current.Response.Cookies["portalroles"];
            if (httpCookie != null)
            {
                httpCookie.Value = null;
                httpCookie.Path = "/";
                httpCookie.Expires = DateTime.Now.AddYears(-30);
            }
        }

        [Obsolete("Deprecated in DNN 5.0.  Please use HasModuleAccess(SecurityAccessLevel.Edit, PortalSettings, ModuleInfo, Username)")]
        public static bool HasEditPermissions(int ModuleId)
        {
            return
                ModulePermissionController.HasModulePermission(
                    new ModulePermissionCollection(CBO.FillCollection(DataProvider.Instance().GetModulePermissionsByModuleID(ModuleId, -1), typeof (ModulePermissionInfo))), "EDIT");
        }

        [Obsolete("Deprecated in DNN 5.0.  Please use HasModuleAccess(SecurityAccessLevel.Edit, PortalSettings, ModuleInfo)")]
        public static bool HasEditPermissions(ModulePermissionCollection objModulePermissions)
        {
            return ModulePermissionController.HasModulePermission(objModulePermissions, "EDIT");
        }

        [Obsolete("Deprecated in DNN 5.0.  Please use HasModuleAccess(SecurityAccessLevel.Edit, PortalSettings, ModuleInfo)")]
        public static bool HasEditPermissions(int ModuleId, int Tabid)
        {
            return ModulePermissionController.HasModulePermission(ModulePermissionController.GetModulePermissions(ModuleId, Tabid), "EDIT");
        }

        [Obsolete("Deprecated in DNN 5.1.  Please use ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, PortalSettings, ModuleInfo)")]
        public static bool HasNecessaryPermission(SecurityAccessLevel AccessLevel, PortalSettings PortalSettings, ModuleInfo ModuleConfiguration, string UserName)
        {
            return ModulePermissionController.HasModuleAccess(AccessLevel, "EDIT", ModuleConfiguration);
        }

        [Obsolete("Deprecated in DNN 5.1.  Please use ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, PortalSettings, ModuleInfo)")]
        public static bool HasNecessaryPermission(SecurityAccessLevel AccessLevel, PortalSettings PortalSettings, ModuleInfo ModuleConfiguration, UserInfo User)
        {
            return ModulePermissionController.HasModuleAccess(AccessLevel, "EDIT", ModuleConfiguration);
        }

        [Obsolete("Deprecated in DNN 5.1.  Please use ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, PortalSettings, ModuleInfo)")]
        public static bool HasNecessaryPermission(SecurityAccessLevel AccessLevel, PortalSettings PortalSettings, ModuleInfo ModuleConfiguration)
        {
            return ModulePermissionController.HasModuleAccess(AccessLevel, "EDIT", ModuleConfiguration);
        }

        [Obsolete("Deprecated in DNN 5.1.  Please use TabPermissionController.CanAdminPage")]
        public static bool IsPageAdmin()
        {
            return TabPermissionController.CanAdminPage();
        }

        [Obsolete("Deprecated in DNN 4.3. This function has been replaced by UserController.UserLogin")]
        public int UserLogin(string Username, string Password, int PortalID, string PortalName, string IP, bool CreatePersistentCookie)
        {
            UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
            int UserId = -1;
            UserInfo objUser = UserController.UserLogin(PortalID, Username, Password, "", PortalName, IP, ref loginStatus, CreatePersistentCookie);
            if (loginStatus == UserLoginStatus.LOGIN_SUCCESS || loginStatus == UserLoginStatus.LOGIN_SUPERUSER)
            {
                UserId = objUser.UserID;
            }
            return UserId;
        }
		
		#endregion
    }
}
