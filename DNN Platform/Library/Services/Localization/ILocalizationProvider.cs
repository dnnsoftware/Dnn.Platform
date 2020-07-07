// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Localization
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Portals;

    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface ILocalizationProvider
    {
        string GetString(string key, string resourceFileRoot);

        string GetString(string key, string resourceFileRoot, string language);

        string GetString(string key, string resourceFileRoot, string language, PortalSettings portalSettings);

        string GetString(string key, string resourceFileRoot, string language, PortalSettings portalSettings, bool disableShowMissingKeys);

        /// <summary>
        /// Saves a string to a resource file.
        /// </summary>
        /// <param name="key">The key to save (e.g. "MyWidget.Text").</param>
        /// <param name="value">The text value for the key.</param>
        /// <param name="resourceFileRoot">Relative path for the resource file root (e.g. "DesktopModules/Admin/Lists/App_LocalResources/ListEditor.ascx.resx").</param>
        /// <param name="language">The locale code in lang-region format (e.g. "fr-FR").</param>
        /// <param name="portalSettings">The current portal settings.</param>
        /// <param name="resourceType">Specifies whether to save as portal, host or system resource file.</param>
        /// <param name="addFile">if set to <c>true</c> a new file will be created if it is not found.</param>
        /// <param name="addKey">if set to <c>true</c> a new key will be created if not found.</param>
        /// <returns>If the value could be saved then true will be returned, otherwise false.</returns>
        /// <exception cref="System.Exception">Any file io error or similar will lead to exceptions.</exception>
        bool SaveString(string key, string value, string resourceFileRoot, string language, PortalSettings portalSettings, DotNetNuke.Services.Localization.LocalizationProvider.CustomizedLocale resourceType, bool addFile, bool addKey);

        /// <summary>
        /// Gets a compiled resource file for a specific language and portal. This takes the original resource file
        /// and overwrites it with any keys found in localized and overridden resource files according to .net and DNN rules.
        /// </summary>
        /// <param name="portalSettings">The portal settings for the requesting portal. Only used to retrieve PortalId and DefaultLanguage.</param>
        /// <param name="resourceFile">The resource file to be retrieved. Relative path from DNN's root starting with /.</param>
        /// <param name="locale">The requested locale. You should use the thread locale by default.</param>
        /// <returns>Dictionary of key value pairs where the keys are the localization keys and the values the localized texts.</returns>
        Dictionary<string, string> GetCompiledResourceFile(PortalSettings portalSettings, string resourceFile, string locale);
    }
}
