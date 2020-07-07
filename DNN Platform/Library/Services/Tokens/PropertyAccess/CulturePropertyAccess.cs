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
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo AccessingUser, Scope AccessLevel, ref bool PropertyNotFound)
        {
            CultureInfo ci = formatProvider;
            if (propertyName.Equals(CultureDropDownTypes.EnglishName.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ci.EnglishName), format);
            }

            if (propertyName.Equals(CultureDropDownTypes.Lcid.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return ci.LCID.ToString();
            }

            if (propertyName.Equals(CultureDropDownTypes.Name.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return PropertyAccess.FormatString(ci.Name, format);
            }

            if (propertyName.Equals(CultureDropDownTypes.NativeName.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return PropertyAccess.FormatString(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ci.NativeName), format);
            }

            if (propertyName.Equals(CultureDropDownTypes.TwoLetterIsoCode.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return PropertyAccess.FormatString(ci.TwoLetterISOLanguageName, format);
            }

            if (propertyName.Equals(CultureDropDownTypes.ThreeLetterIsoCode.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return PropertyAccess.FormatString(ci.ThreeLetterISOLanguageName, format);
            }

            if (propertyName.Equals(CultureDropDownTypes.DisplayName.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return PropertyAccess.FormatString(ci.DisplayName, format);
            }

            if (propertyName.Equals("languagename", StringComparison.InvariantCultureIgnoreCase))
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

            if (propertyName.Equals("languagenativename", StringComparison.InvariantCultureIgnoreCase))
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

            if (propertyName.Equals("countryname", StringComparison.InvariantCultureIgnoreCase))
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

            if (propertyName.Equals("countrynativename", StringComparison.InvariantCultureIgnoreCase))
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

            PropertyNotFound = true;
            return string.Empty;
        }
    }
}
