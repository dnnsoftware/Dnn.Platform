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

using System.Collections;
using System.Data;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Services.Tokens
{
    /// <summary>
    /// The TokenReplace class provides the option to replace tokens formatted
    /// [object:property] or [object:property|format] or [custom:no] within a string
    /// with the appropriate current property/custom values.
    /// Example for Newsletter: 'Dear [user:Displayname],' ==> 'Dear Superuser Account,'
    /// Supported Token Sources: User, Host, Portal, Tab, Module, Membership, Profile,
    ///                          Row, Date, Ticks, ArrayList (Custom), IDictionary
    /// </summary>
    /// <remarks></remarks>
    public class TokenReplace : BaseCustomTokenReplace
    {
        private ModuleInfo _moduleInfo;

        /// <summary>
        /// creates a new TokenReplace object for default context
        /// </summary>
        public TokenReplace() : this(Scope.DefaultSettings, null, null, null, Null.NullInteger)
        {
        }

        /// <summary>
        /// creates a new TokenReplace object for default context and the current module
        /// </summary>
        /// <param name="moduleID">ID of the current module</param>
        public TokenReplace(int moduleID) : this(Scope.DefaultSettings, null, null, null, moduleID)
        {
        }

        /// <summary>
        /// creates a new TokenReplace object for custom context
        /// </summary>
        /// <param name="accessLevel">Security level granted by the calling object</param>
        public TokenReplace(Scope accessLevel) : this(accessLevel, null, null, null, Null.NullInteger)
        {
        }

        /// <summary>
        /// creates a new TokenReplace object for custom context
        /// </summary>
        /// <param name="accessLevel">Security level granted by the calling object</param>
        /// <param name="moduleID">ID of the current module</param>
        public TokenReplace(Scope accessLevel, int moduleID) : this(accessLevel, null, null, null, moduleID)
        {
        }

        /// <summary>
        /// creates a new TokenReplace object for custom context
        /// </summary>
        /// <param name="accessLevel">Security level granted by the calling object</param>
        /// <param name="language">Locale to be used for formatting etc.</param>
        /// <param name="portalSettings">PortalSettings to be used</param>
        /// <param name="user">user, for which the properties shall be returned</param>
        public TokenReplace(Scope accessLevel, string language, PortalSettings portalSettings, UserInfo user) : this(accessLevel, language, portalSettings, user, Null.NullInteger)
        {
        }

        /// <summary>
        /// creates a new TokenReplace object for custom context
        /// </summary>
        /// <param name="accessLevel">Security level granted by the calling object</param>
        /// <param name="language">Locale to be used for formatting etc.</param>
        /// <param name="portalSettings">PortalSettings to be used</param>
        /// <param name="user">user, for which the properties shall be returned</param>
        /// <param name="moduleID">ID of the current module</param>
        public TokenReplace(Scope accessLevel, string language, PortalSettings portalSettings, UserInfo user, int moduleID)
        {
            ModuleId = int.MinValue;
            CurrentAccessLevel = accessLevel;
            if (accessLevel != Scope.NoSettings)
            {
                if (portalSettings == null)
                {
                    if (HttpContext.Current != null)
                    {
                        PortalSettings = PortalController.Instance.GetCurrentPortalSettings();
                    }
                }
                else
                {
                    PortalSettings = portalSettings;
                }
                if (user == null)
                {
                    if (HttpContext.Current != null)
                    {
                        User = (UserInfo) HttpContext.Current.Items["UserInfo"];
                    }
                    else
                    {
                        User = new UserInfo();
                    }
                    AccessingUser = User;
                }
                else
                {
                    User = user;
                    if (HttpContext.Current != null)
                    {
                        AccessingUser = (UserInfo) HttpContext.Current.Items["UserInfo"];
                    }
                    else
                    {
                        AccessingUser = new UserInfo();
                    }
                }
                Language = string.IsNullOrEmpty(language) ? new Localization.Localization().CurrentUICulture : language;
                if (moduleID != Null.NullInteger)
                {
                    ModuleId = moduleID;
                }
            }
            PropertySource["date"] = new DateTimePropertyAccess();
            PropertySource["datetime"] = new DateTimePropertyAccess();
            PropertySource["ticks"] = new TicksPropertyAccess();
            PropertySource["culture"] = new CulturePropertyAccess();
        }
		
        /// <summary>
        /// Gets/sets the current ModuleID to be used for 'User:' token replacement
        /// </summary>
        /// <value>ModuleID (Integer)</value>
        public int ModuleId { get; set; }

        /// <summary>
        /// Gets/sets the module settings object to use for 'Module:' token replacement
        /// </summary>
        public ModuleInfo ModuleInfo
        {
            get
            {
                if (ModuleId > int.MinValue && (_moduleInfo == null || _moduleInfo.ModuleID != ModuleId))
                {
                    if (PortalSettings != null && PortalSettings.ActiveTab != null)
                    {
                        _moduleInfo = ModuleController.Instance.GetModule(ModuleId, PortalSettings.ActiveTab.TabID, false);
                    }
                    else
                    {
                        _moduleInfo = ModuleController.Instance.GetModule(ModuleId, Null.NullInteger, true);
                    }
                }
                return _moduleInfo;
            }
            set
            {
                _moduleInfo = value;
            }
        }

        /// <summary>
        /// Gets/sets the portal settings object to use for 'Portal:' token replacement
        /// </summary>
        /// <value>PortalSettings oject</value>
        public PortalSettings PortalSettings { get; set; }

        /// <summary>
        /// Gets/sets the user object to use for 'User:' token replacement
        /// </summary>
        /// <value>UserInfo oject</value>
        public UserInfo User { get; set; }

        /// <summary>
        /// setup context by creating appropriate objects
        /// </summary>
        /// <remarks >
        /// security is not the purpose of the initialization, this is in the responsibility of each property access class
        /// </remarks>
        private void InitializePropertySources()
        {
            //Cleanup, by default "" is returned for these objects and any property
            IPropertyAccess defaultPropertyAccess = new EmptyPropertyAccess();
            PropertySource["portal"] = defaultPropertyAccess;
            PropertySource["tab"] = defaultPropertyAccess;
            PropertySource["host"] = defaultPropertyAccess;
            PropertySource["module"] = defaultPropertyAccess;
            PropertySource["user"] = defaultPropertyAccess;
            PropertySource["membership"] = defaultPropertyAccess;
            PropertySource["profile"] = defaultPropertyAccess;

            //initialization
            if (CurrentAccessLevel >= Scope.Configuration)
            {
                if (PortalSettings != null)
                {
                    PropertySource["portal"] = PortalSettings;
                    PropertySource["tab"] = PortalSettings.ActiveTab;
                }
                PropertySource["host"] = new HostPropertyAccess();
                if (ModuleInfo != null)
                {
                    PropertySource["module"] = ModuleInfo;
                }
            }
            if (CurrentAccessLevel >= Scope.DefaultSettings && !(User == null || User.UserID == -1))
            {
                PropertySource["user"] = User;
                PropertySource["membership"] = new MembershipPropertyAccess(User);
                PropertySource["profile"] = new ProfilePropertyAccess(User);
            }
        }

		/// <summary>
        /// Replaces tokens in sourceText parameter with the property values
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens</param>
        /// <returns>string containing replaced values</returns>
        public string ReplaceEnvironmentTokens(string sourceText)
        {
            return ReplaceTokens(sourceText);
        }

        /// <summary>
        /// Replaces tokens in sourceText parameter with the property values
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens</param>
        /// <param name="row"></param>
        /// <returns>string containing replaced values</returns>
        public string ReplaceEnvironmentTokens(string sourceText, DataRow row)
        {
            var rowProperties = new DataRowPropertyAccess(row);
            PropertySource["field"] = rowProperties;
            PropertySource["row"] = rowProperties;
            return ReplaceTokens(sourceText);
        }

        /// <summary>
        /// Replaces tokens in sourceText parameter with the property values
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens</param>
        /// <param name="custom"></param>
        /// <param name="customCaption"></param>
        /// <returns>string containing replaced values</returns>
        public string ReplaceEnvironmentTokens(string sourceText, ArrayList custom, string customCaption)
        {
            PropertySource[customCaption.ToLower()] = new ArrayListPropertyAccess(custom);
            return ReplaceTokens(sourceText);
        }

        /// <summary>
        /// Replaces tokens in sourceText parameter with the property values
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens</param>
        /// <param name="custom">NameValueList for replacing [custom:name] tokens, where 'custom' is specified in next param and name is either thekey or the index number in the string </param>
        /// <param name="customCaption">Token name to be used inside token  [custom:name]</param>
        /// <returns>string containing replaced values</returns>
        public string ReplaceEnvironmentTokens(string sourceText, IDictionary custom, string customCaption)
        {
            PropertySource[customCaption.ToLower()] = new DictionaryPropertyAccess(custom);
            return ReplaceTokens(sourceText);
        }

        /// <summary> 
        /// Replaces tokens in sourceText parameter with the property values 
        /// </summary> 
        /// <param name="sourceText">String with [Object:Property] tokens</param> 
        /// <param name="custom">NameValueList for replacing [custom:name] tokens, where 'custom' is specified in next param and name is either thekey or the index number in the string </param> 
        /// <param name="customCaptions">Token names to be used inside token [custom:name], where 'custom' is one of the values in the string array </param> 
        /// <returns>string containing replaced values</returns> 
        public string ReplaceEnvironmentTokens(string sourceText, IDictionary custom, string[] customCaptions)
        {
            foreach (var customCaption in customCaptions)
            {
                PropertySource[customCaption.ToLower()] = new DictionaryPropertyAccess(custom);    
            }           
            return ReplaceTokens(sourceText);
        }
        
        /// <summary>
        /// Replaces tokens in sourceText parameter with the property values
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens</param>
        /// <param name="custom">NameValueList for replacing [custom:name] tokens, where 'custom' is specified in next param and name is either thekey or the index number in the string </param>
        /// <param name="customCaption">Token name to be used inside token  [custom:name]</param>
        /// <param name="row">DataRow, from which field values shall be used for replacement</param>
        /// <returns>string containing replaced values</returns>
        public string ReplaceEnvironmentTokens(string sourceText, ArrayList custom, string customCaption, DataRow row)
        {
            var rowProperties = new DataRowPropertyAccess(row);
            PropertySource["field"] = rowProperties;
            PropertySource["row"] = rowProperties;
            PropertySource[customCaption.ToLower()] = new ArrayListPropertyAccess(custom);
            return ReplaceTokens(sourceText);
        }

        /// <summary>
        /// Replaces tokens in sourceText parameter with the property values, skipping environment objects
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens</param>
        /// <returns>string containing replaced values</returns>
        protected override string ReplaceTokens(string sourceText)
        {
            InitializePropertySources();
            return base.ReplaceTokens(sourceText);
        }
    }
}
