// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Compression
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Cache;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>This class encapsulates the settings for an HttpCompressionModule.</summary>
    [Serializable]
    public class Settings
    {
        private readonly List<string> excludedPaths = new();

        /// <summary>Gets the default settings.  Deflate + normal.</summary>
        public static Settings Default => new();

        /// <summary>Gets the preferred algorithm to use for compression.</summary>
        public Algorithms PreferredAlgorithm { get; private set; } = Algorithms.None;

        /// <summary>Get the current settings from the xml config file.</summary>
        /// <returns>A <see cref="Settings"/> instance.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public static Settings GetSettings()
            => GetSettings(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>());

        /// <summary>Get the current settings from the XML config file.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <returns>A <see cref="Settings"/> instance.</returns>
        public static Settings GetSettings(IHostSettings hostSettings, IApplicationStatusInfo appStatus)
        {
            var settings = (Settings)DataCache.GetCache("CompressionConfig");
            if (settings != null)
            {
                return settings;
            }

            settings = Default;

            // Place this in a try/catch as during install the host settings will not exist
            try
            {
                settings.PreferredAlgorithm = (Algorithms)hostSettings.HttpCompressionAlgorithm;
            }
            catch (Exception e)
            {
                if (appStatus.Status != UpgradeStatus.Install)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(e);
                }
            }

            string filePath = Config.GetPathToFile(appStatus, Config.ConfigFileType.Compression);

            // Create a FileStream for the Config file
            using (var fileReader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var doc = new XPathDocument(fileReader);
                foreach (XPathNavigator nav in doc.CreateNavigator().Select("compression/excludedPaths/path"))
                {
                    settings.excludedPaths.Add(nav.Value.ToLowerInvariant());
                }
            }

            if (File.Exists(filePath))
            {
                // Set back into Cache
                DataCache.SetCache("CompressionConfig", settings, new DNNCacheDependency(filePath));
            }

            return settings;
        }

        /// <summary>Looks for a given path in the list of paths excluded from compression.</summary>
        /// <param name="relUrl">the relative url to check.</param>
        /// <returns>true if excluded, false if not.</returns>
        public bool IsExcludedPath(string relUrl)
        {
            return this.excludedPaths.Any(path => relUrl.ToLowerInvariant().Contains(path));
        }
    }
}
