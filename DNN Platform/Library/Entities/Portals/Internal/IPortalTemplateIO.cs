// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.IO;

namespace DotNetNuke.Entities.Portals.Internal
{
    public interface IPortalTemplateIO
    {
        IEnumerable<string> EnumerateTemplates();
        IEnumerable<string> EnumerateLanguageFiles();
        string GetResourceFilePath(string templateFilePath);
        string GetLanguageFilePath(string templateFilePath, string cultureCode);
        TextReader OpenTextReader(string filePath);
    }
}
