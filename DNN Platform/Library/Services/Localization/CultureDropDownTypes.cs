// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization;

using System;

/// <summary>
/// <para>CultureDropDownTypes allows the user to specify which culture name is displayed in the drop down list that is filled
/// by using one of the helper methods.</para>
/// </summary>
[Serializable]
public enum CultureDropDownTypes
{
    /// <summary>Displays the culture name in the format "&lt;languagefull&gt; (&lt;country/regionfull&gt;) in the .NET Framework language.</summary>
    DisplayName = 0,

    /// <summary>Displays the culture name in the format "&lt;languagefull&gt; (&lt;country/regionfull&gt;) in English.</summary>
    EnglishName = 1,

    /// <summary>Displays the culture identifier.</summary>
    Lcid = 2,

    /// <summary>Displays the culture name in the format "&lt;languagecode2&gt; (&lt;country/regioncode2&gt;).</summary>
    Name = 3,

    /// <summary>Displays the culture name in the format "&lt;languagefull&gt; (&lt;country/regionfull&gt;) in the language that the culture is set to display.</summary>
    NativeName = 4,

    /// <summary>Displays the IS0 639-1 two letter code.</summary>
    TwoLetterIsoCode = 5,

    /// <summary>Displays the ISO 629-2 three letter code "&lt;languagefull&gt; (&lt;country/regionfull&gt;).</summary>
    ThreeLetterIsoCode = 6,
}
