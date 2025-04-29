// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens;

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
/// <c>[object:property]</c> or <c>[object:property|format]</c> or <c>[custom:no]</c> within a string
/// with the appropriate current property/custom values.
/// Example for Newsletter: 'Dear [user:Displayname],' ==> 'Dear Superuser Account,'
/// Supported Token Sources: User, Host, Portal, Tab, Module, Membership, Profile,
///                          Row, Date, Ticks, ArrayList (Custom), IDictionary.
/// </summary>
public class TokenReplace : BaseCustomTokenReplace
{
    /// <summary>Initializes a new instance of the <see cref="TokenReplace"/> class for default context.</summary>
    public TokenReplace()
        : this(Scope.DefaultSettings, null, null, null, Null.NullInteger)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="TokenReplace"/> class for default context and the current module. </summary>
    /// <param name="moduleID">ID of the current module.</param>
    public TokenReplace(int moduleID)
        : this(Scope.DefaultSettings, null, null, null, moduleID)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="TokenReplace"/> class for custom context.</summary>
    /// <param name="accessLevel">Security level granted by the calling object.</param>
    public TokenReplace(Scope accessLevel)
        : this(accessLevel, null, null, null, Null.NullInteger)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="TokenReplace"/> class for custom context.</summary>
    /// <param name="accessLevel">Security level granted by the calling object.</param>
    /// <param name="moduleID">ID of the current module.</param>
    public TokenReplace(Scope accessLevel, int moduleID)
        : this(accessLevel, null, null, null, moduleID)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="TokenReplace"/> class for custom context.</summary>
    /// <param name="accessLevel">Security level granted by the calling object.</param>
    /// <param name="language">Locale to be used for formatting etc.</param>
    /// <param name="portalSettings">PortalSettings to be used.</param>
    /// <param name="user">user, for which the properties shall be returned.</param>
    public TokenReplace(Scope accessLevel, string language, PortalSettings portalSettings, UserInfo user)
        : this(accessLevel, language, portalSettings, user, Null.NullInteger)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="TokenReplace"/> class for custom context.</summary>
    /// <param name="accessLevel">Security level granted by the calling object.</param>
    /// <param name="language">Locale to be used for formatting etc.</param>
    /// <param name="portalSettings">PortalSettings to be used.</param>
    /// <param name="user">user, for which the properties shall be returned.</param>
    /// <param name="moduleID">ID of the current module.</param>
    public TokenReplace(Scope accessLevel, string language, PortalSettings portalSettings, UserInfo user, int moduleID)
    {
        this.CurrentAccessLevel = accessLevel;

        if (accessLevel != Scope.NoSettings)
        {
            this.DeterminePortal(portalSettings);
            this.DetermineUser(user);
            this.DetermineLanguage(language);
            this.DetermineModule(moduleID);
        }

        this.AddPropertySource("date", new DateTimePropertyAccess());
        this.AddPropertySource("datetime", new DateTimePropertyAccess());
        this.AddPropertySource("ticks", new TicksPropertyAccess());
        this.AddPropertySource("culture", new CulturePropertyAccess());
    }

    /// <summary>Gets or sets the current ModuleID to be used for 'User:' token replacement.</summary>
    /// <value>ModuleID (Integer).</value>
    public int ModuleId
    {
        get => this.TokenContext.Module?.ModuleID ?? Null.NullInteger;
        set => this.TokenContext.Module = this.GetModule(value);
    }

    /// <summary>Gets or sets the module settings object to use for 'Module:' token replacement.</summary>
    public ModuleInfo ModuleInfo
    {
        get => this.TokenContext.Module;
        set => this.TokenContext.Module = value;
    }

    /// <summary>Gets or sets the portal settings object to use for 'Portal:' token replacement.</summary>
    /// <value>PortalSettings object.</value>
    public PortalSettings PortalSettings
    {
        get => this.TokenContext.Portal;
        set => this.TokenContext.Portal = value;
    }

    /// <summary>Gets or sets the user object to use for 'User:' token replacement.</summary>
    /// <value>UserInfo object.</value>
    public UserInfo User
    {
        get => this.TokenContext.User;
        set => this.TokenContext.User = value;
    }

    /// <summary>Replaces tokens in <paramref cref="sourceText"/> with the property values.</summary>
    /// <param name="sourceText">String with <c>[Object:Property]</c> tokens.</param>
    /// <returns>string containing replaced values.</returns>
    public string ReplaceEnvironmentTokens(string sourceText)
    {
        return this.ReplaceTokens(sourceText);
    }

    /// <summary>Replaces tokens in <paramref cref="sourceText"/> with the property values.</summary>
    /// <param name="sourceText">String with <c>[Object:Property]</c> tokens.</param>
    /// <param name="row">The data row to use as the source.</param>
    /// <returns>string containing replaced values.</returns>
    public string ReplaceEnvironmentTokens(string sourceText, DataRow row)
    {
        var rowProperties = new DataRowPropertyAccess(row);
        this.AddPropertySource("field", rowProperties);
        this.AddPropertySource("row", rowProperties);
        return this.ReplaceTokens(sourceText);
    }

    /// <summary>Replaces tokens in <paramref cref="sourceText"/> with the property values.</summary>
    /// <param name="sourceText">String with <c>[Object:Property]</c> tokens.</param>
    /// <param name="custom">A list of custom properties.</param>
    /// <param name="customCaption">The prefix for the custom properties.</param>
    /// <returns>string containing replaced values.</returns>
    public string ReplaceEnvironmentTokens(string sourceText, ArrayList custom, string customCaption)
    {
        this.AddPropertySource(customCaption.ToLowerInvariant(), new ArrayListPropertyAccess(custom));
        return this.ReplaceTokens(sourceText);
    }

    /// <summary>Replaces tokens in <paramref cref="sourceText"/> with the property values.</summary>
    /// <param name="sourceText">String with <c>[Object:Property]</c> tokens.</param>
    /// <param name="custom">NameValueList for replacing <c>[custom:name]</c> tokens, where <c>custom</c> is specified in next param and <c>name</c> is either the key or the index number in the string.</param>
    /// <param name="customCaption">Token name to be used inside token <c>[custom:name]</c>.</param>
    /// <returns>string containing replaced values.</returns>
    public string ReplaceEnvironmentTokens(string sourceText, IDictionary custom, string customCaption)
    {
        this.AddPropertySource(customCaption.ToLowerInvariant(), new DictionaryPropertyAccess(custom));
        return this.ReplaceTokens(sourceText);
    }

    /// <summary>Replaces tokens in <paramref cref="sourceText"/> with the property values.</summary>
    /// <param name="sourceText">String with <c>[Object:Property]</c> tokens.</param>
    /// <param name="custom">NameValueList for replacing <c>[custom:name]</c> tokens, where <c>custom</c> is specified in next param and <c>name</c> is either the key or the index number in the string.</param>
    /// <param name="customCaptions">Token names to be used inside token <c>[custom:name]</c>, where 'custom' is one of the values in the string array. </param>
    /// <returns>string containing replaced values.</returns>
    public string ReplaceEnvironmentTokens(string sourceText, IDictionary custom, string[] customCaptions)
    {
        foreach (var customCaption in customCaptions)
        {
            this.AddPropertySource(customCaption.ToLowerInvariant(), new DictionaryPropertyAccess(custom));
        }

        return this.ReplaceTokens(sourceText);
    }

    /// <summary>Replaces tokens in <paramref cref="sourceText"/> with the property values.</summary>
    /// <param name="sourceText">String with <c>[Object:Property]</c> tokens.</param>
    /// <param name="custom">NameValueList for replacing <c>[custom:name]</c> tokens, where <c>custom</c> is specified in next param and <c>name</c> is either the key or the index number in the string.</param>
    /// <param name="customCaption">Token name to be used inside token <c>[custom:name]</c>.</param>
    /// <param name="row">DataRow, from which field values shall be used for replacement.</param>
    /// <returns>string containing replaced values.</returns>
    public string ReplaceEnvironmentTokens(string sourceText, ArrayList custom, string customCaption, DataRow row)
    {
        var rowProperties = new DataRowPropertyAccess(row);
        this.AddPropertySource("field", rowProperties);
        this.AddPropertySource("row", rowProperties);
        this.AddPropertySource(customCaption.ToLowerInvariant(), new ArrayListPropertyAccess(custom));
        return this.ReplaceTokens(sourceText);
    }

    /// <summary>Replaces tokens in <paramref cref="sourceText"/> with the property values, skipping environment objects.</summary>
    /// <param name="sourceText">String with <c>[Object:Property]</c> tokens.</param>
    /// <returns>string containing replaced values.</returns>
    protected override string ReplaceTokens(string sourceText)
    {
        this.InitializePropertySources();
        return base.ReplaceTokens(sourceText);
    }

    /// <summary>setup context by creating appropriate objects.</summary>
    /// <remarks>security is not the purpose of the initialization, this is in the responsibility of each property access class.</remarks>
    private void InitializePropertySources()
    {
        // Cleanup, by default "" is returned for these objects and any property
        IPropertyAccess defaultPropertyAccess = new EmptyPropertyAccess();
        this.AddPropertySource("portal", defaultPropertyAccess);
        this.AddPropertySource("tab", defaultPropertyAccess);
        this.AddPropertySource("host", defaultPropertyAccess);
        this.AddPropertySource("module", defaultPropertyAccess);
        this.AddPropertySource("user", defaultPropertyAccess);
        this.AddPropertySource("membership", defaultPropertyAccess);
        this.AddPropertySource("profile", defaultPropertyAccess);

        // initialization
        if (this.CurrentAccessLevel >= Scope.Configuration)
        {
            if (this.PortalSettings != null)
            {
                this.AddPropertySource("portal", this.PortalSettings);
                this.AddPropertySource("tab", this.PortalSettings.ActiveTab);
            }

            this.AddPropertySource("host", new HostPropertyAccess());
            if (this.ModuleInfo != null)
            {
                this.AddPropertySource("module", this.ModuleInfo);
            }
        }

        if (this.CurrentAccessLevel >= Scope.DefaultSettings && !(this.User == null || this.User.UserID == -1))
        {
            this.AddPropertySource("user", this.User);
            this.AddPropertySource("membership", new MembershipPropertyAccess(this.User));
            this.AddPropertySource("profile", new ProfilePropertyAccess(this.User));
        }
    }

    /// <summary>Load the module for the Module Token Provider.</summary>
    /// <param name="moduleId">The module ID.</param>
    /// <returns>The populated ModuleInfo or <see langword="null"/>.</returns>
    /// <remarks>
    /// This method is called by the Setter of ModuleId.
    /// Because of this, it may NOT access ModuleId itself (which will still be -1) but use moduleId (lower case).
    /// </remarks>
    private ModuleInfo GetModule(int moduleId)
    {
        if (moduleId == this.TokenContext.Module?.ModuleID)
        {
            return this.TokenContext.Module;
        }

        if (moduleId <= 0)
        {
            return null;
        }

        var tab = this.TokenContext.Tab ?? this.PortalSettings?.ActiveTab;
        if (tab != null && tab.TabID > 0)
        {
            return ModuleController.Instance.GetModule(moduleId, tab.TabID, false);
        }

        return ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
    }

    private void DeterminePortal(PortalSettings portalSettings)
    {
        this.PortalSettings = portalSettings ?? PortalController.Instance.GetCurrentPortalSettings();
    }

    private void DetermineUser(UserInfo user)
    {
        this.AccessingUser = HttpContext.Current != null ? (UserInfo)HttpContext.Current.Items["UserInfo"] : new UserInfo();
        this.User = user ?? this.AccessingUser;
    }

    private void DetermineLanguage(string language)
    {
        this.Language = string.IsNullOrEmpty(language) ? new Localization.Localization().CurrentUICulture : language;
    }

    private void DetermineModule(int moduleID)
    {
        if (moduleID != Null.NullInteger)
        {
            this.ModuleId = moduleID;
        }
    }
}
