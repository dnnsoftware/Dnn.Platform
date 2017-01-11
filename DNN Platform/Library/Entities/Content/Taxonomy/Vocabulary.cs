#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections.Generic;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;

#endregion

namespace DotNetNuke.Entities.Content.Taxonomy
{
	/// <summary>
	/// Class of Vocabulary.
	/// </summary>
	/// <seealso cref="TermController"/>
    [Serializable]
    public class Vocabulary : BaseEntityInfo, IHydratable
    {
        private static readonly PortalSecurity Security = new PortalSecurity();

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

        #region "Constructors"

        public Vocabulary() : this(Null.NullString, Null.NullString, VocabularyType.Simple)
        {
        }

        public Vocabulary(string name) : this(name, Null.NullString, VocabularyType.Simple)
        {
        }

        public Vocabulary(string name, string description) : this(name, description, VocabularyType.Simple)
        {
        }

        public Vocabulary(VocabularyType type) : this(Null.NullString, Null.NullString, type)
        {
        }

        public Vocabulary(string name, string description, VocabularyType type)
        {
            Description = description;
            Name = name;
            Type = type;

            ScopeId = Null.NullInteger;
            ScopeTypeId = Null.NullInteger;
            VocabularyId = Null.NullInteger;
            Weight = 0;
        }

        #endregion

        #region "Public Properties"

        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = Security.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
            }
        }

        public bool IsHeirarchical
        {
            get
            {
                return (Type == VocabularyType.Hierarchy);
            }
        }

        public bool IsSystem
        {
            get
            {
                return _IsSystem;
            }
            set
            {
                _IsSystem = value;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (HtmlUtils.ContainsEntity(value))
                    value = System.Net.WebUtility.HtmlDecode(value);
                _Name = Security.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
            }
        }

        public int ScopeId
        {
            get
            {
                return _ScopeId;
            }
            set
            {
                _ScopeId = value;
            }
        }

        public ScopeType ScopeType
        {
            get
            {
                if (_ScopeType == null)
                {
                    _ScopeType = this.GetScopeType(_ScopeTypeId);
                }

                return _ScopeType;
            }
        }

        public int ScopeTypeId
        {
            get
            {
                return _ScopeTypeId;
            }
            set
            {
                _ScopeTypeId = value;
            }
        }

        public List<Term> Terms
        {
            get
            {
                if (_Terms == null)
                {
                    _Terms = this.GetTerms(_VocabularyId);
                }
                return _Terms;
            }
        }

        public VocabularyType Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
            }
        }

        public int VocabularyId
        {
            get
            {
                return _VocabularyId;
            }
            set
            {
                _VocabularyId = value;
            }
        }

        public int Weight
        {
            get
            {
                return _Weight;
            }
            set
            {
                _Weight = value;
            }
        }

        #endregion

        #region "IHydratable Implementation"

        public virtual void Fill(IDataReader dr)
        {
            VocabularyId = Null.SetNullInteger(dr["VocabularyID"]);
            switch (Convert.ToInt16(dr["VocabularyTypeID"]))
            {
                case 1:
                    Type = VocabularyType.Simple;
                    break;
                case 2:
                    Type = VocabularyType.Hierarchy;
                    break;
            }
            IsSystem = Null.SetNullBoolean(dr["IsSystem"]);
            Name = Null.SetNullString(dr["Name"]);
            Description = Null.SetNullString(dr["Description"]);
            ScopeId = Null.SetNullInteger(dr["ScopeID"]);
            ScopeTypeId = Null.SetNullInteger(dr["ScopeTypeID"]);
            Weight = Null.SetNullInteger(dr["Weight"]);

            //Fill base class properties
            FillInternal(dr);
        }

        public virtual int KeyID
        {
            get
            {
                return VocabularyId;
            }
            set
            {
                VocabularyId = value;
            }
        }

        #endregion
    }
}