// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Utilities;

using System.Collections.Generic;
using System.Text.RegularExpressions;

using DNNConnect.CKEditorProvider.Objects;

/// <summary>Finds, and extract all Anchors from a Page.</summary>
public static class AnchorFinder
{
    /// <summary>Extract the Anchors.</summary>
    /// <param name="file">
    /// The file.
    /// </param>
    /// <returns>
    /// All Anchors on the Page.
    /// </returns>
    public static List<LinkItem> ListAll(string file)
    {
        var anchorList = new List<LinkItem>();

        MatchCollection m1 = Regex.Matches(file, @"(<a.*?>.*?</a>)", RegexOptions.IgnoreCase);

        foreach (Match m in m1)
        {
            string value = m.Groups[1].Value;
            LinkItem item = new LinkItem();

            Match m2 = Regex.Match(value, @"href=\""(.*?)\""", RegexOptions.Singleline);

            if (m2.Success)
            {
                item.Href = m2.Groups[1].Value;
            }

            Match m3 = Regex.Match(value, @"name=\""(.*?)\""", RegexOptions.Singleline);

            if (m3.Success)
            {
                item.Anchor = m3.Groups[1].Value;
            }

            item.Text = Regex.Replace(value, @"\s*<.*?>\s*", string.Empty, RegexOptions.Singleline);

            anchorList.Add(item);
        }

        return anchorList;
    }
}
