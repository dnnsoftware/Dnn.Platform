// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System;
    using System.Globalization;

    using DotNetNuke.Services.Localization;

    /// <summary>Utility class to support simple localization.</summary>
    internal class Localizer : ILocalizer
    {
        /// <summary>The relative path to the resources file for localization.</summary>
        public static readonly string ResourceFile =
            "~/DesktopModules/TelerikRemoval/App_LocalResources/View.ascx.resx";

        private readonly ILocalizationProvider localizationProvider;

        /// <summary>Initializes a new instance of the <see cref="Localizer"/> class.</summary>
        /// <param name="localizationProvider">An instance of <see cref="ILocalizationProvider"/>.</param>
        public Localizer(ILocalizationProvider localizationProvider)
        {
            this.localizationProvider = localizationProvider ??
                throw new ArgumentNullException(nameof(localizationProvider));
        }

        /// <inheritdoc/>
        public string Localize(string key)
        {
            return this.localizationProvider.GetString(key, ResourceFile);
        }

        /// <inheritdoc/>
        public string LocalizeFormat(string formatKey, params object[] args)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                this.Localize(formatKey),
                args);
        }
    }
}
