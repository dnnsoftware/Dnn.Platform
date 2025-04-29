// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization;

using System.Collections;
using System.Globalization;

public class CultureInfoComparer : IComparer
{
    private readonly string compare;

    /// <summary>Initializes a new instance of the <see cref="CultureInfoComparer"/> class.</summary>
    /// <param name="compareBy"></param>
    public CultureInfoComparer(string compareBy)
    {
        this.compare = compareBy;
    }

    /// <inheritdoc/>
    public int Compare(object x, object y)
    {
        switch (this.compare.ToUpperInvariant())
        {
            case "ENGLISH":
                return ((CultureInfo)x).EnglishName.CompareTo(((CultureInfo)y).EnglishName);
            case "NATIVE":
                return ((CultureInfo)x).NativeName.CompareTo(((CultureInfo)y).NativeName);
            default:
                return ((CultureInfo)x).Name.CompareTo(((CultureInfo)y).Name);
        }
    }
}
