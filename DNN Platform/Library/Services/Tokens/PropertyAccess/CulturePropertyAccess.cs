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
            if (propertyName.Equals(nameof(CultureDropDownTypes.EnglishName), StringComparison.OrdinalIgnoreCase))
            {
                return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formatProvider.EnglishName), format);
            }

            if (propertyName.Equals(nameof(CultureDropDownTypes.Lcid), StringComparison.OrdinalIgnoreCase))
            {
                return formatProvider.LCID.ToString(formatProvider);
            }

            if (propertyName.Equals(nameof(CultureDropDownTypes.Name), StringComparison.OrdinalIgnoreCase))
            {
                return PropertyAccess.FormatString(formatProvider.Name, format);
            }

            if (propertyName.Equals(nameof(CultureDropDownTypes.NativeName), StringComparison.OrdinalIgnoreCase))
            {
                return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formatProvider.NativeName), format);
            }

            if (propertyName.Equals(nameof(CultureDropDownTypes.TwoLetterIsoCode), StringComparison.OrdinalIgnoreCase))
            {
                return PropertyAccess.FormatString(formatProvider.TwoLetterISOLanguageName, format);
            }

            if (propertyName.Equals(nameof(CultureDropDownTypes.ThreeLetterIsoCode), StringComparison.OrdinalIgnoreCase))
            {
                return PropertyAccess.FormatString(formatProvider.ThreeLetterISOLanguageName, format);
            }

            if (propertyName.Equals(nameof(CultureDropDownTypes.DisplayName), StringComparison.OrdinalIgnoreCase))
            {
                return PropertyAccess.FormatString(formatProvider.DisplayName, format);
            }

            if (propertyName.Equals("languagename", StringComparison.OrdinalIgnoreCase))
            {
                if (formatProvider.IsNeutralCulture)
                {
                    return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formatProvider.EnglishName), format);
                }
                else
                {
                    return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formatProvider.Parent.EnglishName), format);
                }
            }

            if (propertyName.Equals("languagenativename", StringComparison.OrdinalIgnoreCase))
            {
                if (formatProvider.IsNeutralCulture)
                {
                    return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formatProvider.NativeName), format);
                }
                else
                {
                    return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formatProvider.Parent.NativeName), format);
                }
            }

            if (propertyName.Equals("countryname", StringComparison.OrdinalIgnoreCase))
            {
                if (formatProvider.IsNeutralCulture)
                {
                    // Neutral culture do not include region information
                    return string.Empty;
                }
                else
                {
                    RegionInfo country = new RegionInfo(new CultureInfo(formatProvider.Name, false).LCID);
                    return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(country.EnglishName), format);
                }
            }

            if (propertyName.Equals("countrynativename", StringComparison.OrdinalIgnoreCase))
            {
                if (formatProvider.IsNeutralCulture)
                {
                    // Neutral culture do not include region information
                    return string.Empty;
                }
                else
                {
                    RegionInfo country = new RegionInfo(new CultureInfo(formatProvider.Name, false).LCID);
                    return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(country.NativeName), format);
                }
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
