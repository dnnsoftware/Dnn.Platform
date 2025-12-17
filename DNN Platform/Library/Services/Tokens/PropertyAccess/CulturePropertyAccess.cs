// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Globalization;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;

    public class CulturePropertyAccess : IPropertyAccess
    {
        /// <inheritdoc/>
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        /// <inheritdoc/>
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            CultureInfo ci = formatProvider;
            if (propertyName.Equals(CultureDropDownTypes.EnglishName.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ci.EnglishName), format);
            }

            if (propertyName.Equals(CultureDropDownTypes.Lcid.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return ci.LCID.ToString();
            }

            if (propertyName.Equals(CultureDropDownTypes.Name.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return PropertyAccess.FormatString(ci.Name, format);
            }

            if (propertyName.Equals(CultureDropDownTypes.NativeName.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ci.NativeName), format);
            }

            if (propertyName.Equals(CultureDropDownTypes.TwoLetterIsoCode.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return PropertyAccess.FormatString(ci.TwoLetterISOLanguageName, format);
            }

            if (propertyName.Equals(CultureDropDownTypes.ThreeLetterIsoCode.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return PropertyAccess.FormatString(ci.ThreeLetterISOLanguageName, format);
            }

            if (propertyName.Equals(CultureDropDownTypes.DisplayName.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return PropertyAccess.FormatString(ci.DisplayName, format);
            }

            if (propertyName.Equals("languagename", StringComparison.OrdinalIgnoreCase))
            {
                if (ci.IsNeutralCulture)
                {
                    return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ci.EnglishName), format);
                }
                else
                {
                    return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ci.Parent.EnglishName), format);
                }
            }

            if (propertyName.Equals("languagenativename", StringComparison.OrdinalIgnoreCase))
            {
                if (ci.IsNeutralCulture)
                {
                    return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ci.NativeName), format);
                }
                else
                {
                    return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ci.Parent.NativeName), format);
                }
            }

            if (propertyName.Equals("countryname", StringComparison.OrdinalIgnoreCase))
            {
                if (ci.IsNeutralCulture)
                {
                    // Neutral culture do not include region information
                    return string.Empty;
                }
                else
                {
                    RegionInfo country = new RegionInfo(new CultureInfo(ci.Name, false).LCID);
                    return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(country.EnglishName), format);
                }
            }

            if (propertyName.Equals("countrynativename", StringComparison.OrdinalIgnoreCase))
            {
                if (ci.IsNeutralCulture)
                {
                    // Neutral culture do not include region information
                    return string.Empty;
                }
                else
                {
                    RegionInfo country = new RegionInfo(new CultureInfo(ci.Name, false).LCID);
                    return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(country.NativeName), format);
                }
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
