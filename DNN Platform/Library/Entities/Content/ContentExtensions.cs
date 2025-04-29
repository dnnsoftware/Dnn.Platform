// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content;

using System.Collections.Generic;

using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(7, 0, 0, "Moving ContentExtensions to the DotNetNuke.Entities.Content namespace was an error. Please use DotNetNuke.Entities.Content.Common.ContentExtensions.", RemovalVersion = 10)]
public static partial class ContentExtensions
{
    // only forwarding public methods that existed as of 6.1.0
    // calls to internal methods will be fixed in the source
    public static string ToDelimittedString(this List<Term> terms, string delimiter)
    {
        return Common.ContentExtensions.ToDelimittedString(terms, delimiter);
    }

    public static string ToDelimittedString(this List<Term> terms, string format, string delimiter)
    {
        return Common.ContentExtensions.ToDelimittedString(terms, format, delimiter);
    }
}
