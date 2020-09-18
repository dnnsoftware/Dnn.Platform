// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Web;
    using System.Web.Script.Serialization;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;

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
        private static readonly PortalSecurity Security = PortalSecurity.Instance;
        private readonly List<string> _synonyms = new List<string>();

        private List<Term> _childTerms;
        private string _description;
        private int _left;
        private string _name;
        private int? _parentTermId;
        private int _right;
        private int _termId;
        private Vocabulary _vocabulary;
        private int _vocabularyId;
        private int _weight;

        public Term()
            : this(Null.NullString, Null.NullString, Null.NullInteger)
        {
        }

        public Term(int vocabularyId)
            : this(Null.NullString, Null.NullString, vocabularyId)
        {
        }

        public Term(string name)
            : this(name, Null.NullString, Null.NullInteger)
        {
        }

        public Term(string name, string description)
            : this(name, description, Null.NullInteger)
        {
        }

        public Term(string name, string description, int vocabularyId)
        {
            this.Description = description;
            this.Name = name;
            this._vocabularyId = vocabularyId;

            this.ParentTermId = null;
            this.TermId = Null.NullInteger;
            this._left = 0;
            this._right = 0;
            this.Weight = 0;
        }

        [XmlIgnore]
        [ScriptIgnore]
        public List<Term> ChildTerms
        {
            get
            {
                if (this._childTerms == null)
                {
                    this._childTerms = this.GetChildTerms(this._termId, this._vocabularyId);
                }

                return this._childTerms;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public bool IsHeirarchical
        {
            get
            {
                return this.Vocabulary.Type == VocabularyType.Hierarchy;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public int Left
        {
            get
            {
                return this._left;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public int Right
        {
            get
            {
                return this._right;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public List<string> Synonyms
        {
            get
            {
                return this._synonyms;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public Vocabulary Vocabulary
        {
            get
            {
                if (this._vocabulary == null && this._vocabularyId > Null.NullInteger)
                {
                    this._vocabulary = this.GetVocabulary(this._vocabularyId);
                }

                return this._vocabulary;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public int VocabularyId
        {
            get
            {
                return this._vocabularyId;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public string Description
        {
            get
            {
                return this._description;
            }

            set
            {
                this._description = Security.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public string Name
        {
            get
            {
                return this._name;
            }

            set
            {
                if (HtmlUtils.IsUrlEncoded(value))
                {
                    value = System.Net.WebUtility.UrlDecode(value);
                }

                if (HtmlUtils.ContainsEntity(value))
                {
                    value = System.Net.WebUtility.HtmlDecode(value);
                }

                this._name = Security.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public int? ParentTermId
        {
            get
            {
                return this._parentTermId;
            }

            set
            {
                this._parentTermId = value;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public int TermId
        {
            get
            {
                return this._termId;
            }

            set
            {
                this._termId = value;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        public int Weight
        {
            get
            {
                return this._weight;
            }

            set
            {
                this._weight = value;
            }
        }

        public int KeyID
        {
            get
            {
                return this.TermId;
            }

            set
            {
                this.TermId = value;
            }
        }

        public virtual void Fill(IDataReader dr)
        {
            this.TermId = Null.SetNullInteger(dr["TermID"]);
            this.Name = Null.SetNullString(dr["Name"]);
            this.Description = Null.SetNullString(dr["Description"]);
            this.Weight = Null.SetNullInteger(dr["Weight"]);
            this._vocabularyId = Null.SetNullInteger(dr["VocabularyID"]);

            if (dr["TermLeft"] != DBNull.Value)
            {
                this._left = Convert.ToInt32(dr["TermLeft"]);
            }

            if (dr["TermRight"] != DBNull.Value)
            {
                this._right = Convert.ToInt32(dr["TermRight"]);
            }

            if (dr["ParentTermID"] != DBNull.Value)
            {
                this.ParentTermId = Convert.ToInt32(dr["ParentTermID"]);
            }

            // Fill base class properties
            this.FillInternal(dr);
        }

        public string GetTermPath()
        {
            string path = "\\\\" + this.Name;

            if (this.ParentTermId.HasValue)
            {
                ITermController ctl = Util.GetTermController();

                Term parentTerm = (from t in ctl.GetTermsByVocabulary(this.VocabularyId) where t.TermId == this.ParentTermId select t).SingleOrDefault();

                if (parentTerm != null)
                {
                    path = parentTerm.GetTermPath() + path;
                }
            }

            return path;
        }
    }
}
