// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Portals.Templates;

/// <summary>Informational class for a Portal Template consisting of a template file and (potentially) a resources file that correspond to the given culture code.</summary>
public interface IPortalTemplateInfo
{
    /// <summary>Gets the full path to the resource file for the given culture, if it exists.</summary>
    string ResourceFilePath { get; }

    /// <summary>Gets the name of the template (this comes either from the resource file or it's the filename without the .template extension).</summary>
    string Name { get; }

    /// <summary>Gets the culture code for this template/resource file combination.</summary>
    string CultureCode { get; }

    /// <summary>Gets the full path to the template file.</summary>
    string TemplateFilePath { get; }

    /// <summary>Gets the full path to the resource file without the .resources extension.</summary>
    string LanguageFilePath { get; }

    /// <summary>Gets the description for the template. This is taken either from the resources file or parsed from the template itself.</summary>
    string Description { get; }
}
