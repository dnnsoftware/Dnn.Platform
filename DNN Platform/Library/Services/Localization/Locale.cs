// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization
{
    using System;
    using System.Data;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;

    /// <summary>
    ///   <para>The Locale class is a custom business object that represents a locale, which is the language and country combination.</para>
    /// </summary>
    [Serializable]
    public class Locale : BaseEntityInfo, IHydratable
    {
        public Locale()
        {
            this.PortalId = Null.NullInteger;
            this.LanguageId = Null.NullInteger;
            this.IsPublished = Null.NullBoolean;
        }

        public CultureInfo Culture
        {
            get
            {
                return CultureInfo.GetCultureInfo(this.Code);
            }
        }

        public string EnglishName
        {
            get
            {
                string _Name = Null.NullString;
                if (this.Culture != null)
                {
                    _Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.Culture.EnglishName);
                }

                return _Name;
            }
        }

        public Locale FallBackLocale
        {
            get
            {
                Locale _FallbackLocale = null;
                if (!string.IsNullOrEmpty(this.Fallback))
                {
                    _FallbackLocale = LocaleController.Instance.GetLocale(this.PortalId, this.Fallback);
                }

                return _FallbackLocale;
            }
        }

        public string NativeName
        {
            get
            {
                string _Name = Null.NullString;
                if (this.Culture != null)
                {
                    _Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.Culture.NativeName);
                }

                return _Name;
            }
        }

        public string Code { get; set; }

        public string Fallback { get; set; }

        public bool IsPublished { get; set; }

        public int LanguageId { get; set; }

        public int PortalId { get; set; }

        public string Text { get; set; }

        public int KeyID
        {
            get
            {
                return this.LanguageId;
            }

            set
            {
                this.LanguageId = value;
            }
        }

        public void Fill(IDataReader dr)
        {
            this.LanguageId = Null.SetNullInteger(dr["LanguageID"]);
            this.Code = Null.SetNullString(dr["CultureCode"]);
            this.Text = Null.SetNullString(dr["CultureName"]);
            this.Fallback = Null.SetNullString(dr["FallbackCulture"]);

            // These fields may not be populated (for Host level locales)
            DataTable schemaTable = dr.GetSchemaTable();
            bool hasColumns = schemaTable.Select("ColumnName = 'IsPublished' Or ColumnName = 'PortalID'").Length == 2;

            if (hasColumns)
            {
                this.IsPublished = Null.SetNullBoolean(dr["IsPublished"]);
                this.PortalId = Null.SetNullInteger(dr["PortalID"]);
            }

            // Call the base classes fill method to populate base class proeprties
            this.FillInternal(dr);
        }
    }
}
