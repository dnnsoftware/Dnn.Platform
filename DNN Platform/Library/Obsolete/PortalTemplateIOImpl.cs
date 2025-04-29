// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Internal;

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Internal.SourceGenerators;

[EditorBrowsable(EditorBrowsableState.Never)]
[DnnDeprecated(7, 3, 0, "Use PortalTemplateIO", RemovalVersion = 10)]
public partial class PortalTemplateIOImpl : IPortalTemplateIO
{
    /// <inheritdoc/>
    public IEnumerable<string> EnumerateTemplates()
    {
        string path = Globals.HostMapPath;
        if (Directory.Exists(path))
        {
            return Directory.GetFiles(path, "*.template").Where(x => Path.GetFileNameWithoutExtension(x) != "admin");
        }

        return new string[0];
    }

    /// <inheritdoc/>
    public IEnumerable<string> EnumerateLanguageFiles()
    {
        string path = Globals.HostMapPath;
        if (Directory.Exists(path))
        {
            return Directory.GetFiles(path, "*.template.??-??.resx");
        }

        return new string[0];
    }

    /// <inheritdoc/>
    public string GetResourceFilePath(string templateFilePath)
    {
        return CheckFilePath(templateFilePath + ".resources");
    }

    /// <inheritdoc/>
    public string GetLanguageFilePath(string templateFilePath, string cultureCode)
    {
        return CheckFilePath(string.Format("{0}.{1}.resx", templateFilePath, cultureCode));
    }

    /// <inheritdoc/>
    public TextReader OpenTextReader(string filePath)
    {
        return new StreamReader(File.Open(filePath, FileMode.Open));
    }

    /// <inheritdoc/>
    public (string, List<string>) GetTemplateLanguages(string templateFilePath)
    {
        var defaultLanguage = string.Empty;
        var locales = new List<string>();
        var templateXml = new XmlDocument() { XmlResolver = null };
        templateXml.Load(templateFilePath);
        var node = templateXml.SelectSingleNode("//settings/defaultlanguage");
        if (node != null)
        {
            defaultLanguage = node.InnerText;
        }

        node = templateXml.SelectSingleNode("//locales");
        if (node != null)
        {
            foreach (XmlNode lnode in node.SelectNodes("//locale"))
            {
                locales.Add(lnode.InnerText);
            }
        }

        return (defaultLanguage, locales);
    }

    private static string CheckFilePath(string path)
    {
        if (File.Exists(path))
        {
            return path;
        }

        return string.Empty;
    }
}
