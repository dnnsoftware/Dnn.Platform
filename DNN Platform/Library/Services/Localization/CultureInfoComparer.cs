// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization
{
    using System;
    using System.Collections;
    using System.Globalization;

    public class CultureInfoComparer : IComparer
    {
        private readonly string compare;

        /// <summary>Initializes a new instance of the <see cref="CultureInfoComparer"/> class.</summary>
        /// <param name="compareBy"><c>"ENGLISH"</c> to compare by <see cref="CultureInfo.EnglishName"/>, <c>"NATIVE"</c> to compare by <see cref="CultureInfo.NativeName"/>, or anything else to compare by <see cref="CultureInfo.Name"/>.</param>
        public CultureInfoComparer(string compareBy)
        {
            this.compare = compareBy;
        }

        /// <inheritdoc/>
        public int Compare(object x, object y)
        {
            return this.compare.ToUpperInvariant() switch
            {
                "ENGLISH" => string.Compare(((CultureInfo)x).EnglishName, ((CultureInfo)y).EnglishName, StringComparison.Ordinal),
                "NATIVE" => string.Compare(((CultureInfo)x).NativeName, ((CultureInfo)y).NativeName, StringComparison.Ordinal),
                _ => string.Compare(((CultureInfo)x).Name, ((CultureInfo)y).Name, StringComparison.Ordinal),
            };
        }
    }
}
