// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Services.Cache;

#endregion

namespace DotNetNuke.HttpModules.Compression
{
    /// <summary>
    /// This class encapsulates the settings for an HttpCompressionModule
    /// </summary>
    [Serializable]
    public class Settings
    {
        private readonly StringCollection _excludedPaths;
        private Algorithms _preferredAlgorithm;

        private Settings()
        {
            _preferredAlgorithm = Algorithms.None;
            _excludedPaths = new StringCollection();
        }

        /// <summary>
        /// The default settings.  Deflate + normal.
        /// </summary>
        public static Settings Default
        {
            get
            {
                return new Settings();
            }
        }

        /// <summary>
        /// The preferred algorithm to use for compression
        /// </summary>
        public Algorithms PreferredAlgorithm
        {
            get
            {
                return _preferredAlgorithm;
            }
        }

        /// <summary>
        /// Get the current settings from the xml config file
        /// </summary>
        public static Settings GetSettings()
        {
            var settings = (Settings) DataCache.GetCache("CompressionConfig");
            if (settings == null)
            {
                settings = Default;
                //Place this in a try/catch as during install the host settings will not exist
                try
                {
                    settings._preferredAlgorithm = (Algorithms) Host.HttpCompressionAlgorithm;
                }
                catch (Exception e)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(e);
                }

                string filePath = Common.Utilities.Config.GetPathToFile(Common.Utilities.Config.ConfigFileType.Compression);

                //Create a FileStream for the Config file
                using (var fileReader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var doc = new XPathDocument(fileReader);
                    foreach (XPathNavigator nav in doc.CreateNavigator().Select("compression/excludedPaths/path"))
                    {
                        settings._excludedPaths.Add(nav.Value.ToLowerInvariant());
                    }
                }
                if ((File.Exists(filePath)))
                {
                    //Set back into Cache
                    DataCache.SetCache("CompressionConfig", settings, new DNNCacheDependency(filePath));
                }
            }
            return settings;
        }

        /// <summary>
        /// Looks for a given path in the list of paths excluded from compression
        /// </summary>
        /// <param name="relUrl">the relative url to check</param>
        /// <returns>true if excluded, false if not</returns>
        public bool IsExcludedPath(string relUrl)
        {
            bool match = false;
            foreach (string path in _excludedPaths)
            {
                if (relUrl.ToLowerInvariant().Contains(path))
                {
                    match = true;
                    break;
                }
            }
            return match;
        }
    }
}
