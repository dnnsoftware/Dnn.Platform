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
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;

#endregion

namespace DotNetNuke.Entities.Content.Taxonomy
{
	/// <summary>
	/// Major class of Taxonomy.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Taxonomy is defined as “the practice and science of classification” – Wikipedia,
	/// while Folksonomy is defined as “collaborative tagging” – Wikipedia. 
	/// Usually, taxonomy refers to the practice of using hierarchical categories applied to the content by a “content editor”, 
	/// while folksonomy refers to the practice of free-form tagging of content by users. 
	/// In DotNetNuke, while we expose both of these at the presentation layer, in the API and Data Layer they are implemented 
	/// using a common data structure.
	/// </para>
	/// <para>
	/// There are a number of advantages of using a special System Vocabulary for storing user entered tags.
	/// One is that both taxonomy terms and folksonomy tags are treated in the API and Data Layer in the same way.
	/// This means that we only have to manage one relationship between content and terms rather than two separate relationships.
	/// The second benefit of treating tags in this way is that an admin can “manage” the tags – ie remove any offensive or inappropriate tags, 
	/// or correct spellings of tags, by using the Taxonomy Manager UI.
	/// </para>
	/// </remarks>
	/// <example>
	/// <code lang="C#">
	/// internal static List&lt;Term&gt; GetTerms(this Vocabulary voc, int vocabularyId)
	/// {
	///     ITermController ctl = Util.GetTermController();
	///     return ctl.GetTermsByVocabulary(vocabularyId).ToList();
	/// }
	/// </code>
	/// </example>
    [Serializable]
    public class Term : BaseEntityInfo, IHydratable
    {
        private static readonly PortalSecurity Security = new PortalSecurity();

        private List<Term> _childTerms;
        private string _description;
        private int _left;
        private string _name;
        private int? _parentTermId;
        private int _right;
        private readonly List<string> _synonyms = new List<String>();
        private int _termId;
        private Vocabulary _vocabulary;
        private int _vocabularyId;
        private int _weight;

        #region "Constructors"

        public Term() : this(Null.NullString, Null.NullString, Null.NullInteger)
        {
        }

        public Term(int vocabularyId) : this(Null.NullString, Null.NullString, vocabularyId)
        {
        }

        public Term(string name) : this(name, Null.NullString, Null.NullInteger)
        {
        }

        public Term(string name, string description) : this(name, description, Null.NullInteger)
        {
        }

        public Term(string name, string description, int vocabularyId)
        {
            _description = description;
            _name = name;
            _vocabularyId = vocabularyId;

            _parentTermId = null;
            _termId = Null.NullInteger;
            _left = 0;
            _right = 0;
            _weight = 0;
        }

        #endregion

        #region "Public Properties"

        [XmlIgnore]
        [ScriptIgnore]
        public List<Term> ChildTerms
        {
            get
            {
                if (_childTerms == null)
                {
                    _childTerms = this.GetChildTerms(_termId, _vocabularyId);
                }
                return _childTerms;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public bool IsHeirarchical
        {
            get
            {
                return (Vocabulary.Type == VocabularyType.Hierarchy);
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public int Left
        {
            get
            {
                return _left;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = Security.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public int? ParentTermId
        {
            get
            {
                return _parentTermId;
            }
            set
            {
                _parentTermId = value;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public int Right
        {
            get
            {
                return _right;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public List<string> Synonyms
        {
            get
            {
                return _synonyms;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public int TermId
        {
            get
            {
                return _termId;
            }
            set
            {
                _termId = value;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public Vocabulary Vocabulary
        {
            get
            {
                if (_vocabulary == null && _vocabularyId > Null.NullInteger)
                {
                    _vocabulary = this.GetVocabulary(_vocabularyId);
                }
                return _vocabulary;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public int VocabularyId
        {
            get
            {
                return _vocabularyId;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public int Weight
        {
            get
            {
                return _weight;
            }
            set
            {
                _weight = value;
            }
        }

	    #endregion

        #region "IHydratable Implementation"

        public virtual void Fill(IDataReader dr)
        {
            TermId = Null.SetNullInteger(dr["TermID"]);
            Name = Null.SetNullString(dr["Name"]);
            Description = Null.SetNullString(dr["Description"]);
            Weight = Null.SetNullInteger(dr["Weight"]);
            _vocabularyId = Null.SetNullInteger(dr["VocabularyID"]);

            if (dr["TermLeft"] != DBNull.Value)
            {
                _left = Convert.ToInt32(dr["TermLeft"]);
            }
            if (dr["TermRight"] != DBNull.Value)
            {
                _right = Convert.ToInt32(dr["TermRight"]);
            }
            if (dr["ParentTermID"] != DBNull.Value)
            {
                ParentTermId = Convert.ToInt32(dr["ParentTermID"]);
            }

            //Fill base class properties
            FillInternal(dr);
        }

        public int KeyID
        {
            get
            {
                return TermId;
            }
            set
            {
                TermId = value;
            }
        }

        #endregion

        public string GetTermPath()
        {
            string path = "\\\\" + Name;

            if (ParentTermId.HasValue)
            {
                ITermController ctl = Util.GetTermController();

                Term parentTerm = (from t in ctl.GetTermsByVocabulary(VocabularyId) where t.TermId == ParentTermId select t).SingleOrDefault();

                if (parentTerm != null)
                {
                    path = parentTerm.GetTermPath() + path;
                }
            }
            return path;
        }
    }
}