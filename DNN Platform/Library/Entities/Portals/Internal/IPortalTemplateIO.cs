// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public interface IPortalTemplateIO
    {
        IEnumerable<string> EnumerateTemplates();

        IEnumerable<string> EnumerateLanguageFiles();

        string GetResourceFilePath(string templateFilePath);

        string GetLanguageFilePath(string templateFilePath, string cultureCode);

        TextReader OpenTextReader(string filePath);
    }
}
