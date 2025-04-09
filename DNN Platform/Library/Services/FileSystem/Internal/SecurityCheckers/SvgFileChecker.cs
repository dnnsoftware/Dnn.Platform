// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal.SecurityCheckers;

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

public class SvgFileChecker : IFileSecurityChecker
{
    /// <inheritdoc/>
    public bool Validate(Stream fileContent)
    {
        try
        {
            var doc = XDocument.Load(fileContent);
            return doc.Descendants().All(e => e.Name.LocalName != "script" && e.Attributes().All(a => !a.Name.LocalName.StartsWith("on")));
        }
        catch (Exception)
        {
            // when there have exception occur, just return false as not validated, no need log the error.
            return false;
        }
    }
}
