#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using Dnn.Modules.ResourceManager.Components.Models;

namespace Dnn.Modules.ResourceManager.Components
{
    public interface ILocalizationController
    {
        string CultureName { get; }

        long GetResxTimeStamp(string resourceFile, Localization localization);

        Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture, Localization localization);

        /// <summary>
        /// Returns a dictionary of localized key for a resource file for a given culture
        /// </summary>
        /// <param name="resourceFile">The relative file path of the main resource file, e.g. ~/DesktopModules/SocialLibrary/App_LocalResources/CmxResources.resx</param>
        /// <param name="culture">The culture for which this dictionay is requested</param>
        /// <remarks>This API does not fallback to any other DNN resource files.</remarks>
        Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture);
    }
}
