// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;

    /// <summary>
    /// Extension methods for IMvcModuleControl interface.
    /// </summary>
    public static class MvcModuleControlExtensions
    {

        /// <summary>
        /// Gets a localized string for the module control.
        /// </summary>
        /// <param name="moduleControl">The module control instance.</param>
        /// <param name="key">The resource key.</param>
        /// <returns>The localized string.</returns>
        public static string LocalizeString(this IMvcModuleControl moduleControl, string key)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException(nameof(moduleControl));
            }

            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return Localization.GetString(key, moduleControl.LocalResourceFile);
        }

        /// <summary>
        /// Gets a localized string formatted for safe JavaScript usage.
        /// </summary>
        /// <param name="moduleControl">The module control instance.</param>
        /// <param name="key">The resource key.</param>
        /// <returns>The JavaScript-safe localized string.</returns>
        public static string LocalizeSafeJsString(this IMvcModuleControl moduleControl, string key)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException(nameof(moduleControl));
            }

            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return Localization.GetSafeJSString(key, moduleControl.LocalResourceFile);
        }

        /// <summary>
        /// Gets an edit URL for the module with specific parameters.
        /// </summary>
        /// <param name="moduleControl">The module control instance.</param>
        /// <param name="keyName">The parameter key name.</param>
        /// <param name="keyValue">The parameter key value.</param>
        /// <param name="controlKey">The control key for the edit page.</param>
        /// <returns>The edit URL with parameters.</returns>
        public static string EditUrl(this IMvcModuleControl moduleControl)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException(nameof(moduleControl));
            }
            return EditUrl(moduleControl, "Edit", string.Empty, string.Empty);
        }

        /// <summary>
        /// Gets an edit URL for the module with specific parameters.
        /// </summary>
        /// <param name="moduleControl">The module control instance.</param>
        /// <param name="keyName">The parameter key name.</param>
        /// <param name="keyValue">The parameter key value.</param>
        /// <param name="controlKey">The control key for the edit page.</param>
        /// <returns>The edit URL with parameters.</returns>
        public static string EditUrl(this IMvcModuleControl moduleControl, string controlKey)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException(nameof(moduleControl));
            }
            return EditUrl(moduleControl, controlKey, string.Empty, string.Empty);
        }

        /// <summary>
        /// Gets an edit URL for the module with specific parameters.
        /// </summary>
        /// <param name="moduleControl">The module control instance.</param>
        /// <param name="keyName">The parameter key name.</param>
        /// <param name="keyValue">The parameter key value.</param>
        /// <param name="controlKey">The control key for the edit page.</param>
        /// <returns>The edit URL with parameters.</returns>
        public static string EditUrl(this IMvcModuleControl moduleControl, string keyName, string keyValue)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException(nameof(moduleControl));
            }
            return EditUrl(moduleControl, keyName, keyValue, string.Empty);
        }

        /// <summary>
        /// Gets an edit URL for the module with specific parameters.
        /// </summary>
        /// <param name="moduleControl">The module control instance.</param>
        /// <param name="keyName">The parameter key name.</param>
        /// <param name="keyValue">The parameter key value.</param>
        /// <param name="controlKey">The control key for the edit page.</param>
        /// <returns>The edit URL with parameters.</returns>
        public static string EditUrl(this IMvcModuleControl moduleControl, string keyName, string keyValue, string controlKey)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException(nameof(moduleControl));
            }
            var parameters = new string[] { };
            return EditUrl(moduleControl, keyName, keyValue, controlKey, parameters);
        }

        /// <summary>
        /// Gets an edit URL for the module with specific parameters.
        /// </summary>
        /// <param name="moduleControl">The module control instance.</param>
        /// <param name="keyName">The parameter key name.</param>
        /// <param name="keyValue">The parameter key value.</param>
        /// <param name="controlKey">The control key for the edit page.</param>
        /// <returns>The edit URL with parameters.</returns>
        public static string EditUrl(this IMvcModuleControl moduleControl, string keyName, string keyValue, string controlKey, params string[] additionalParameters)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException(nameof(moduleControl));
            }
            var parameters = GetParameters(moduleControl, controlKey, additionalParameters);
            return moduleControl.ModuleContext.EditUrl(keyName, keyValue, controlKey, parameters);
        }


        /// <summary>
        /// Gets an edit URL for the module with specific parameters.
        /// </summary>
        /// <param name="moduleControl">The module control instance.</param>
        /// <param name="keyName">The parameter key name.</param>
        /// <param name="keyValue">The parameter key value.</param>
        /// <param name="controlKey">The control key for the edit page.</param>
        /// <returns>The edit URL with parameters.</returns>
        public static string EditUrl(this IMvcModuleControl moduleControl, int tabID, string controlKey, bool pageRedirect, params string[] additionalParameters)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException(nameof(moduleControl));
            }
            var parameters = GetParameters(moduleControl, controlKey, additionalParameters);
            return moduleControl.ModuleContext.NavigateUrl(tabID, controlKey, pageRedirect, parameters);
        }

        /// <summary>
        /// Gets a module setting value with type conversion.
        /// </summary>
        /// <typeparam name="T">The type to convert the setting to.</typeparam>
        /// <param name="moduleControl">The module control instance.</param>
        /// <param name="settingName">The setting name.</param>
        /// <param name="defaultValue">The default value if setting is not found or conversion fails.</param>
        /// <returns>The setting value converted to the specified type.</returns>
        public static T GetModuleSetting<T>(this IMvcModuleControl moduleControl, string settingName, T defaultValue = default)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException(nameof(moduleControl));
            }

            if (string.IsNullOrEmpty(settingName))
            {
                return defaultValue;
            }

            var settings = moduleControl.ModuleContext.Settings;
            if (settings == null || !settings.ContainsKey(settingName))
            {
                return defaultValue;
            }

            try
            {
                var settingValue = settings[settingName]?.ToString();
                if (string.IsNullOrEmpty(settingValue))
                {
                    return defaultValue;
                }

                return (T)Convert.ChangeType(settingValue, typeof(T));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Checks if the current user is in edit mode for the module.
        /// </summary>
        /// <param name="moduleControl">The module control instance.</param>
        /// <returns>True if in edit mode; otherwise, false.</returns>
        public static bool EditMode(this IMvcModuleControl moduleControl)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException(nameof(moduleControl));
            }

            return moduleControl.ModuleContext.EditMode;
        }

        /// <summary>
        /// Checks if the module is editable by the current user.
        /// </summary>
        /// <param name="moduleControl">The module control instance.</param>
        /// <returns>True if editable; otherwise, false.</returns>
        public static bool IsEditable(this IMvcModuleControl moduleControl)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException(nameof(moduleControl));
            }

            return moduleControl.ModuleContext.IsEditable;
        }

        private static string[] GetParameters(IMvcModuleControl moduleControl, string controlKey, string[] additionalParameters)
        {
            if (moduleControl.ModuleContext.Configuration.ModuleDefinition.ModuleControls.ContainsKey(controlKey))
            {
                var editModuleControl = moduleControl.ModuleContext.Configuration.ModuleDefinition.ModuleControls[controlKey];
                if (!string.IsNullOrEmpty(editModuleControl.MvcControlClass))
                {
                    var parameters = new string[1 + additionalParameters.Length];
                    parameters[0] = "mvcpage=yes";
                    Array.Copy(additionalParameters, 0, parameters, 1, additionalParameters.Length);
                    return parameters;
                }
            }

            return additionalParameters;
        }
    }
}
