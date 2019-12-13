#region Usings

using System;
using System.Data;
using System.Globalization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Localization
{
    /// <summary>
    ///   <para>The Locale class is a custom business object that represents a locale, which is the language and country combination.</para>
    /// </summary>
    [Serializable]
    public class Locale : BaseEntityInfo, IHydratable
    {
        public Locale()
        {
            PortalId = Null.NullInteger;
            LanguageId = Null.NullInteger;
            IsPublished = Null.NullBoolean;
        }

        #region Public Properties

        public string Code { get; set; }

        public CultureInfo Culture
        {
            get
            {
                return CultureInfo.GetCultureInfo(Code);
            }
        }

        public string EnglishName
        {
            get
            {
                string _Name = Null.NullString;
                if (Culture != null)
                {
                    _Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Culture.EnglishName);
                }
                return _Name;
            }
        }

        public string Fallback { get; set; }

        public Locale FallBackLocale
        {
            get
            {
                Locale _FallbackLocale = null;
                if (!string.IsNullOrEmpty(Fallback))
                {
                    _FallbackLocale = LocaleController.Instance.GetLocale(PortalId, Fallback);
                }
                return _FallbackLocale;
            }
        }

        public bool IsPublished { get; set; }

        public int LanguageId { get; set; }

        public string NativeName
        {
            get
            {
                string _Name = Null.NullString;
                if (Culture != null)
                {
                    _Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Culture.NativeName);
                }
                return _Name;
            }
        }

        public int PortalId { get; set; }

        public string Text { get; set; }

        #endregion

        #region IHydratable Implementation

        public void Fill(IDataReader dr)
        {
            LanguageId = Null.SetNullInteger(dr["LanguageID"]);
            Code = Null.SetNullString(dr["CultureCode"]);
            Text = Null.SetNullString(dr["CultureName"]);
            Fallback = Null.SetNullString(dr["FallbackCulture"]);

            //These fields may not be populated (for Host level locales)
            DataTable schemaTable = dr.GetSchemaTable();
            bool hasColumns = schemaTable.Select("ColumnName = 'IsPublished' Or ColumnName = 'PortalID'").Length == 2;
            
            if(hasColumns)
            {
                IsPublished = Null.SetNullBoolean(dr["IsPublished"]);
                PortalId = Null.SetNullInteger(dr["PortalID"]);
            }
            
            //Call the base classes fill method to populate base class proeprties
            base.FillInternal(dr);
        }

        public int KeyID
        {
            get
            {
                return LanguageId;
            }
            set
            {
                LanguageId = value;
            }
        }

        #endregion

	}
}
