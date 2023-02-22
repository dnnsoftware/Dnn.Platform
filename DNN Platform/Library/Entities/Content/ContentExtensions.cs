// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Content.Taxonomy;

    [Obsolete("Moving ContentExtensions to the DotNetNuke.Entities.Content namespace was an error. Please use DotNetNuke.Entities.Content.Common.ContentExtensions. Scheduled removal in v10.0.0.")]
    public static class ContentExtensions
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
}
