// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    using System.Collections.Generic;

    public interface IExtensionUrlProviderSettingsControl
    {
        ExtensionUrlProviderInfo Provider { get; set; }

        void LoadSettings();

        /// <summary>
        /// Build the Settings Dictionary and return it to the caller to persist to the database.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> SaveSettings();
    }
}
