// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Settings
{
    using System;

    /// <summary>
    /// When applied to a property this attribute persists the value of the property in the DNN PortalSettings referenced by the PortalId within the context.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PortalSettingAttribute : ParameterAttributeBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether the setting changes with (portal) language. If set to true, the parameter will be
        /// stored and retrieved based on the current thread locale. If false, it will be stored without a locale.
        /// </summary>
        public bool LocaleSensitive { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the settings should be stored securely. This encrypts the value of the parameter.
        /// </summary>
        public bool IsSecure { get; set; } = false;
    }
}
