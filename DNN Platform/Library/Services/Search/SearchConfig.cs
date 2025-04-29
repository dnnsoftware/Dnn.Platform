// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search;

using System;
using System.Collections.Generic;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Internal.SourceGenerators;

/// <summary>The SearchConfig class provides a configuration class for Search.</summary>
[DnnDeprecated(7, 1, 0, "No longer used in the Search infrastructure", RemovalVersion = 10)]
[Serializable]
public partial class SearchConfig
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchConfig));
    private readonly bool searchIncludeCommon;
    private readonly bool searchIncludeNumeric;
    private readonly int searchMaxWordlLength;
    private readonly int searchMinWordlLength;

    /// <summary>Initializes a new instance of the <see cref="SearchConfig"/> class.</summary>
    /// <param name="portalID"></param>
    public SearchConfig(int portalID)
        : this(PortalController.Instance.GetPortalSettings(portalID))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SearchConfig"/> class.</summary>
    /// <param name="settings"></param>
    public SearchConfig(Dictionary<string, string> settings)
    {
        this.searchIncludeCommon = this.GetSettingAsBoolean("SearchIncludeCommon", settings, Host.SearchIncludeCommon);
        this.searchIncludeNumeric = this.GetSettingAsBoolean("SearchIncludeNumeric", settings, Host.SearchIncludeNumeric);
        this.searchMaxWordlLength = this.GetSettingAsInteger("MaxSearchWordLength", settings, Host.SearchMaxWordlLength);
        this.searchMinWordlLength = this.GetSettingAsInteger("MinSearchWordLength", settings, Host.SearchMinWordlLength);
    }

    /// <summary>Gets a value indicating whether to include Common Words in the Search Index.</summary>
    /// <remarks>Defaults to False.</remarks>
    public bool SearchIncludeCommon
    {
        get
        {
            return this.searchIncludeCommon;
        }
    }

    /// <summary>Gets a value indicating whether to include Numbers in the Search Index.</summary>
    /// <remarks>Defaults to False.</remarks>
    public bool SearchIncludeNumeric
    {
        get
        {
            return this.searchIncludeNumeric;
        }
    }

    /// <summary>Gets the maximum Search Word length to index.</summary>
    /// <remarks>Defaults to 25.</remarks>
    public int SearchMaxWordlLength
    {
        get
        {
            return this.searchMaxWordlLength;
        }
    }

    /// <summary>Gets the maximum Search Word length to index.</summary>
    /// <remarks>Defaults to 3.</remarks>
    public int SearchMinWordlLength
    {
        get
        {
            return this.searchMinWordlLength;
        }
    }

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
                retValue = setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
            }
        }
        catch (Exception exc)
        {
            // we just want to trap the error as we may not be installed so there will be no Settings
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
            // we just want to trap the error as we may not be installed so there will be no Settings
            Logger.Error(exc);
        }

        return retValue;
    }
}
