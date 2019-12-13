﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace Dnn.EditBar.UI.Controllers
{
    internal interface ILocalizationController
    {
        string CultureName { get; }

        /// <summary>
        /// Returns a dictionary of localized key for a resource file for a given culture
        /// </summary>
        /// <param name="resourceFile">The relative file path of the main resource file, e.g. ~/DesktopModules/SocialLibrary/App_LocalResources/CmxResources.resx</param>
        /// <param name="culture">The culture for which this dictionay is requested</param>
        /// <remarks>This API does not fallback to any other DNN resource files.</remarks>
        Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture);
    }
}
