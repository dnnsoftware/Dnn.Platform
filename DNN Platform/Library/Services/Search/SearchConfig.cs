#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Collections.Generic;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchConfig class provides a configuration class for Search
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Obsolete("Deprecated in DNN 7.1.  No longer used in the Search infrastructure.")]
    [Serializable]
    public class SearchConfig
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (SearchConfig));
		#region "Private Members"

        private readonly bool _SearchIncludeCommon;
        private readonly bool _SearchIncludeNumeric;
        private readonly int _SearchMaxWordlLength;
        private readonly int _SearchMinWordlLength;
		
		#endregion

		#region "Constructor(s)"

        public SearchConfig(int portalID)
            : this(PortalController.Instance.GetPortalSettings(portalID))
        {
        }

        public SearchConfig(Dictionary<string, string> settings)
        {
            _SearchIncludeCommon = GetSettingAsBoolean("SearchIncludeCommon", settings, Host.SearchIncludeCommon);
            _SearchIncludeNumeric = GetSettingAsBoolean("SearchIncludeNumeric", settings, Host.SearchIncludeNumeric);
            _SearchMaxWordlLength = GetSettingAsInteger("MaxSearchWordLength", settings, Host.SearchMaxWordlLength);
            _SearchMinWordlLength = GetSettingAsInteger("MinSearchWordLength", settings, Host.SearchMinWordlLength);
        }
		
		#endregion

		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to inlcude Common Words in the Search Index
        /// </summary>
        /// <remarks>Defaults to False</remarks>
        /// -----------------------------------------------------------------------------
        public bool SearchIncludeCommon
        {
            get
            {
                return _SearchIncludeCommon;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to inlcude Numbers in the Search Index
        /// </summary>
        /// <remarks>Defaults to False</remarks>
        /// -----------------------------------------------------------------------------
        public bool SearchIncludeNumeric
        {
            get
            {
                return _SearchIncludeNumeric;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum Search Word length to index
        /// </summary>
        /// <remarks>Defaults to 25</remarks>
        /// -----------------------------------------------------------------------------
        public int SearchMaxWordlLength
        {
            get
            {
                return _SearchMaxWordlLength;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum Search Word length to index
        /// </summary>
        /// <remarks>Defaults to 3</remarks>
        /// -----------------------------------------------------------------------------
        public int SearchMinWordlLength
        {
            get
            {
                return _SearchMinWordlLength;
            }
        }
		
		#endregion

		#region "Private Methods"


        private bool GetSettingAsBoolean(string key, Dictionary<string, string> settings, bool defaultValue)
        {
            bool retValue = Null.NullBoolean;
            try
            {
                string setting = Null.NullString;
                settings.TryGetValue(key, out setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = (setting.ToUpperInvariant().StartsWith("Y") || setting.ToUpperInvariant() == "TRUE");
                }
            }
            catch (Exception exc)
            {
                //we just want to trap the error as we may not be installed so there will be no Settings
                Logger.Error(exc);

            }
            return retValue;
        }

        private int GetSettingAsInteger(string key, Dictionary<string, string> settings, int defaultValue)
        {
            int retValue = Null.NullInteger;
            try
            {
                string setting = Null.NullString;
                settings.TryGetValue(key, out setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = Convert.ToInt32(setting);
                }
            }
            catch (Exception exc)
            {
                //we just want to trap the error as we may not be installed so there will be no Settings
                Logger.Error(exc);

            }
            return retValue;
        }
		
		#endregion
    }
}