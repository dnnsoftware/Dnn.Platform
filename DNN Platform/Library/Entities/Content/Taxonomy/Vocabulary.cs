// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;

    /// <summary>
    /// Class of Vocabulary.
    /// </summary>
    /// <seealso cref="TermController"/>
    [Serializable]
    public class Vocabulary : BaseEntityInfo, IHydratable
    {
        private static readonly PortalSecurity Security = PortalSecurity.Instance;

        private string _Description;
        private bool _IsSystem;
        private string _Name;
        private int _ScopeId;
        private ScopeType _ScopeType;
        private int _ScopeTypeId;
        private List<Term> _Terms;
        private VocabularyType _Type;
        private int _VocabularyId;

        private int _Weight;

        public Vocabulary()
            : this(Null.NullString, Null.NullString, VocabularyType.Simple)
        {
        }

        public Vocabulary(string name)
            : this(name, Null.NullString, VocabularyType.Simple)
        {
        }

        public Vocabulary(string name, string description)
            : this(name, description, VocabularyType.Simple)
        {
        }

        public Vocabulary(VocabularyType type)
            : this(Null.NullString, Null.NullString, type)
        {
        }

        public Vocabulary(string name, string description, VocabularyType type)
        {
            this.Description = description;
            this.Name = name;
            this.Type = type;

            this.ScopeId = Null.NullInteger;
            this.ScopeTypeId = Null.NullInteger;
            this.VocabularyId = Null.NullInteger;
            this.Weight = 0;
        }

        public bool IsHeirarchical
        {
            get
            {
                return this.Type == VocabularyType.Hierarchy;
            }
        }

        public ScopeType ScopeType
        {
            get
            {
                if (this._ScopeType == null)
                {
                    this._ScopeType = this.GetScopeType(this._ScopeTypeId);
                }

                return this._ScopeType;
            }
        }

        public List<Term> Terms
        {
            get
            {
                if (this._Terms == null)
                {
                    this._Terms = this.GetTerms(this._VocabularyId);
                }

                return this._Terms;
            }
        }

        public string Description
        {
            get
            {
                return this._Description;
            }

            set
            {
                this._Description = Security.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
            }
        }

        public bool IsSystem
        {
            get
            {
                return this._IsSystem;
            }

            set
            {
                this._IsSystem = value;
            }
        }

        public string Name
        {
            get
            {
                return this._Name;
            }

            set
            {
                if (HtmlUtils.ContainsEntity(value))
                {
                    value = System.Net.WebUtility.HtmlDecode(value);
                }

                this._Name = Security.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
            }
        }

        public int ScopeId
        {
            get
            {
                return this._ScopeId;
            }

            set
            {
                this._ScopeId = value;
            }
        }

        public int ScopeTypeId
        {
            get
            {
                return this._ScopeTypeId;
            }

            set
            {
                this._ScopeTypeId = value;
            }
        }

        public VocabularyType Type
        {
            get
            {
                return this._Type;
            }

            set
            {
                this._Type = value;
            }
        }

        public int VocabularyId
        {
            get
            {
                return this._VocabularyId;
            }

            set
            {
                this._VocabularyId = value;
            }
        }

        public int Weight
        {
            get
            {
                return this._Weight;
            }

            set
            {
                this._Weight = value;
            }
        }

        public virtual int KeyID
        {
            get
            {
                return this.VocabularyId;
            }

            set
            {
                this.VocabularyId = value;
            }
        }

        public virtual void Fill(IDataReader dr)
        {
            this.VocabularyId = Null.SetNullInteger(dr["VocabularyID"]);
            switch (Convert.ToInt16(dr["VocabularyTypeID"]))
            {
                case 1:
                    this.Type = VocabularyType.Simple;
                    break;
                case 2:
                    this.Type = VocabularyType.Hierarchy;
                    break;
            }

            this.IsSystem = Null.SetNullBoolean(dr["IsSystem"]);
            this.Name = Null.SetNullString(dr["Name"]);
            this.Description = Null.SetNullString(dr["Description"]);
            this.ScopeId = Null.SetNullInteger(dr["ScopeID"]);
            this.ScopeTypeId = Null.SetNullInteger(dr["ScopeTypeID"]);
            this.Weight = Null.SetNullInteger(dr["Weight"]);

            // Fill base class properties
            this.FillInternal(dr);
        }
    }
}
