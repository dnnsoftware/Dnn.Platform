// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens
{
    using System.Collections;
    using System.Data;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    /// <summary>
    /// The TokenReplace class provides the option to replace tokens formatted
    /// [object:property] or [object:property|format] or [custom:no] within a string
    /// with the appropriate current property/custom values.
    /// Example for Newsletter: 'Dear [user:Displayname],' ==> 'Dear Superuser Account,'
    /// Supported Token Sources: User, Host, Portal, Tab, Module, Membership, Profile,
    ///                          Row, Date, Ticks, ArrayList (Custom), IDictionary.
    /// </summary>
    /// <remarks></remarks>
    public class TokenReplace : BaseCustomTokenReplace
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenReplace"/> class.
        /// creates a new TokenReplace object for default context.
        /// </summary>
        public TokenReplace()
            : this(Scope.DefaultSettings, null, null, null, Null.NullInteger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenReplace"/> class.
        /// creates a new TokenReplace object for default context and the current module.
        /// </summary>
        /// <param name="moduleID">ID of the current module.</param>
        public TokenReplace(int moduleID)
            : this(Scope.DefaultSettings, null, null, null, moduleID)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenReplace"/> class.
        /// creates a new TokenReplace object for custom context.
        /// </summary>
        /// <param name="accessLevel">Security level granted by the calling object.</param>
        public TokenReplace(Scope accessLevel)
            : this(accessLevel, null, null, null, Null.NullInteger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenReplace"/> class.
        /// creates a new TokenReplace object for custom context.
        /// </summary>
        /// <param name="accessLevel">Security level granted by the calling object.</param>
        /// <param name="moduleID">ID of the current module.</param>
        public TokenReplace(Scope accessLevel, int moduleID)
            : this(accessLevel, null, null, null, moduleID)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenReplace"/> class.
        /// creates a new TokenReplace object for custom context.
        /// </summary>
        /// <param name="accessLevel">Security level granted by the calling object.</param>
        /// <param name="language">Locale to be used for formatting etc.</param>
        /// <param name="portalSettings">PortalSettings to be used.</param>
        /// <param name="user">user, for which the properties shall be returned.</param>
        public TokenReplace(Scope accessLevel, string language, PortalSettings portalSettings, UserInfo user)
            : this(accessLevel, language, portalSettings, user, Null.NullInteger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenReplace"/> class.
        /// creates a new TokenReplace object for custom context.
        /// </summary>
        /// <param name="accessLevel">Security level granted by the calling object.</param>
        /// <param name="language">Locale to be used for formatting etc.</param>
        /// <param name="portalSettings">PortalSettings to be used.</param>
        /// <param name="user">user, for which the properties shall be returned.</param>
        /// <param name="moduleID">ID of the current module.</param>
        public TokenReplace(Scope accessLevel, string language, PortalSettings portalSettings, UserInfo user, int moduleID)
        {
            this.CurrentAccessLevel = accessLevel;

            if (accessLevel != Scope.NoSettings) {
                DeterminePortal(portalSettings);
                DetermineUser(user);
                DetermineLanguage(language);
                DetermineModule(moduleID);
            }

            this.PropertySource["date"] = new DateTimePropertyAccess();
            this.PropertySource["datetime"] = new DateTimePropertyAccess();
            this.PropertySource["ticks"] = new TicksPropertyAccess();
            this.PropertySource["culture"] = new CulturePropertyAccess();
        }

        private void DeterminePortal(PortalSettings portalSettings)
        {
            PortalSettings = portalSettings ?? PortalController.Instance.GetCurrentPortalSettings();
        }

        private void DetermineUser(UserInfo user)
        {
            AccessingUser = HttpContext.Current != null ? (UserInfo)HttpContext.Current.Items["UserInfo"] : new UserInfo();
            User = user ?? AccessingUser;
        }

        private void DetermineLanguage(string language)
        {
            Language = string.IsNullOrEmpty(language) ? new Localization.Localization().CurrentUICulture : language;
        }

        private void DetermineModule(int moduleID)
        {
            if (moduleID != Null.NullInteger)
                ModuleId = moduleID;
        }

        /// <summary>
        /// Gets or sets /sets the current ModuleID to be used for 'User:' token replacement.
        /// </summary>
        /// <value>ModuleID (Integer).</value>
        public int ModuleId {
            get => TokenContext.Module?.ModuleID ?? Null.NullInteger;
            set => TokenContext.Module = GetModule(value);
        }

        private ModuleInfo GetModule(int moduleId)
        {
            if (moduleId == TokenContext.Module?.ModuleID)
                return TokenContext.Module;

            if (moduleId <= 0)
                return null;

            var tab = TokenContext.Tab ?? PortalSettings?.ActiveTab;
            if (tab != null && tab.TabID > 0)
                return ModuleController.Instance.GetModule(ModuleId, tab.TabID, false);

            return ModuleController.Instance.GetModule(ModuleId, Null.NullInteger, true);
        }

        /// <summary>
        /// Gets or sets the module settings object to use for 'Module:' token replacement.
        /// </summary>
        public ModuleInfo ModuleInfo
        {
            get => TokenContext.Module;
            set => TokenContext.Module = value;
        }

        /// <summary>
        /// Gets or sets /sets the portal settings object to use for 'Portal:' token replacement.
        /// </summary>
        /// <value>PortalSettings oject.</value>
        public PortalSettings PortalSettings {
            get => TokenContext.Portal;
            set => TokenContext.Portal = value;
        }

        /// <summary>
        /// Gets or sets /sets the user object to use for 'User:' token replacement.
        /// </summary>
        /// <value>UserInfo oject.</value>
        public UserInfo User {
            get => TokenContext.User;
            set => TokenContext.User = value;
        }

        /// <summary>
        /// Replaces tokens in sourceText parameter with the property values.
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens.</param>
        /// <returns>string containing replaced values.</returns>
        public string ReplaceEnvironmentTokens(string sourceText)
        {
            return this.ReplaceTokens(sourceText);
        }

        /// <summary>
        /// Replaces tokens in sourceText parameter with the property values.
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens.</param>
        /// <param name="row"></param>
        /// <returns>string containing replaced values.</returns>
        public string ReplaceEnvironmentTokens(string sourceText, DataRow row)
        {
            var rowProperties = new DataRowPropertyAccess(row);
            this.PropertySource["field"] = rowProperties;
            this.PropertySource["row"] = rowProperties;
            return this.ReplaceTokens(sourceText);
        }

        /// <summary>
        /// Replaces tokens in sourceText parameter with the property values.
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens.</param>
        /// <param name="custom"></param>
        /// <param name="customCaption"></param>
        /// <returns>string containing replaced values.</returns>
        public string ReplaceEnvironmentTokens(string sourceText, ArrayList custom, string customCaption)
        {
            this.PropertySource[customCaption.ToLowerInvariant()] = new ArrayListPropertyAccess(custom);
            return this.ReplaceTokens(sourceText);
        }

        /// <summary>
        /// Replaces tokens in sourceText parameter with the property values.
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens.</param>
        /// <param name="custom">NameValueList for replacing [custom:name] tokens, where 'custom' is specified in next param and name is either thekey or the index number in the string. </param>
        /// <param name="customCaption">Token name to be used inside token  [custom:name].</param>
        /// <returns>string containing replaced values.</returns>
        public string ReplaceEnvironmentTokens(string sourceText, IDictionary custom, string customCaption)
        {
            this.PropertySource[customCaption.ToLowerInvariant()] = new DictionaryPropertyAccess(custom);
            return this.ReplaceTokens(sourceText);
        }

        /// <summary>
        /// Replaces tokens in sourceText parameter with the property values.
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens.</param>
        /// <param name="custom">NameValueList for replacing [custom:name] tokens, where 'custom' is specified in next param and name is either thekey or the index number in the string. </param>
        /// <param name="customCaptions">Token names to be used inside token [custom:name], where 'custom' is one of the values in the string array. </param>
        /// <returns>string containing replaced values.</returns>
        public string ReplaceEnvironmentTokens(string sourceText, IDictionary custom, string[] customCaptions)
        {
            foreach (var customCaption in customCaptions)
            {
                this.PropertySource[customCaption.ToLowerInvariant()] = new DictionaryPropertyAccess(custom);
            }

            return this.ReplaceTokens(sourceText);
        }

        /// <summary>
        /// Replaces tokens in sourceText parameter with the property values.
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens.</param>
        /// <param name="custom">NameValueList for replacing [custom:name] tokens, where 'custom' is specified in next param and name is either thekey or the index number in the string. </param>
        /// <param name="customCaption">Token name to be used inside token  [custom:name].</param>
        /// <param name="row">DataRow, from which field values shall be used for replacement.</param>
        /// <returns>string containing replaced values.</returns>
        public string ReplaceEnvironmentTokens(string sourceText, ArrayList custom, string customCaption, DataRow row)
        {
            var rowProperties = new DataRowPropertyAccess(row);
            this.PropertySource["field"] = rowProperties;
            this.PropertySource["row"] = rowProperties;
            this.PropertySource[customCaption.ToLowerInvariant()] = new ArrayListPropertyAccess(custom);
            return this.ReplaceTokens(sourceText);
        }

        /// <summary>
        /// Replaces tokens in sourceText parameter with the property values, skipping environment objects.
        /// </summary>
        /// <param name="sourceText">String with [Object:Property] tokens.</param>
        /// <returns>string containing replaced values.</returns>
        protected override string ReplaceTokens(string sourceText)
        {
            this.InitializePropertySources();
            return base.ReplaceTokens(sourceText);
        }

        /// <summary>
        /// setup context by creating appropriate objects.
        /// </summary>
        /// <remarks >
        /// security is not the purpose of the initialization, this is in the responsibility of each property access class.
        /// </remarks>
        private void InitializePropertySources()
        {
            // Cleanup, by default "" is returned for these objects and any property
            IPropertyAccess defaultPropertyAccess = new EmptyPropertyAccess();
            this.PropertySource["portal"] = defaultPropertyAccess;
            this.PropertySource["tab"] = defaultPropertyAccess;
            this.PropertySource["host"] = defaultPropertyAccess;
            this.PropertySource["module"] = defaultPropertyAccess;
            this.PropertySource["user"] = defaultPropertyAccess;
            this.PropertySource["membership"] = defaultPropertyAccess;
            this.PropertySource["profile"] = defaultPropertyAccess;

            // initialization
            if (this.CurrentAccessLevel >= Scope.Configuration)
            {
                if (this.PortalSettings != null)
                {
                    this.PropertySource["portal"] = this.PortalSettings;
                    this.PropertySource["tab"] = this.PortalSettings.ActiveTab;
                }

                this.PropertySource["host"] = new HostPropertyAccess();
                if (this.ModuleInfo != null)
                {
                    this.PropertySource["module"] = this.ModuleInfo;
                }
            }

            if (this.CurrentAccessLevel >= Scope.DefaultSettings && !(this.User == null || this.User.UserID == -1))
            {
                this.PropertySource["user"] = this.User;
                this.PropertySource["membership"] = new MembershipPropertyAccess(this.User);
                this.PropertySource["profile"] = new ProfilePropertyAccess(this.User);
            }
        }
    }
}
