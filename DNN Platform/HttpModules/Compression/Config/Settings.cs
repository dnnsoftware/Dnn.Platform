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
                        settings._excludedPaths.Add(nav.Value.ToLower());
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
                if (relUrl.ToLower().Contains(path))
                {
                    match = true;
                    break;
                }
            }
            return match;
        }
    }
}
