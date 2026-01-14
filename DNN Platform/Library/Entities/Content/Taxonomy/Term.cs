// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Web.Script.Serialization;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;
    using Newtonsoft.Json;

    /// <summary>Major class of Taxonomy.</summary>
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
        private readonly List<string> synonyms = new List<string>();

        private List<Term> childTerms;
        private string description;
        private int left;
        private string name;
        private int? parentTermId;
        private int right;
        private int termId;
        private Vocabulary vocabulary;
        private int vocabularyId;
        private int weight;

        /// <summary>Initializes a new instance of the <see cref="Term"/> class.</summary>
        public Term()
            : this(Null.NullString, Null.NullString, Null.NullInteger)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Term"/> class.</summary>
        /// <param name="vocabularyId">The vocabulary ID.</param>
        public Term(int vocabularyId)
            : this(Null.NullString, Null.NullString, vocabularyId)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Term"/> class.</summary>
        /// <param name="name">The term name.</param>
        public Term(string name)
            : this(name, Null.NullString, Null.NullInteger)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Term"/> class.</summary>
        /// <param name="name">The term name.</param>
        /// <param name="description">The term description.</param>
        public Term(string name, string description)
            : this(name, description, Null.NullInteger)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Term"/> class.</summary>
        /// <param name="name">The term name.</param>
        /// <param name="description">The term description.</param>
        /// <param name="vocabularyId">The vocabulary ID.</param>
        public Term(string name, string description, int vocabularyId)
        {
            this.Description = description;
            this.Name = name;
            this.vocabularyId = vocabularyId;

            this.ParentTermId = null;
            this.TermId = Null.NullInteger;
            this.left = 0;
            this.right = 0;
            this.Weight = 0;
        }

        [XmlIgnore]
        [ScriptIgnore]
        [JsonIgnore]
        public List<Term> ChildTerms
        {
            get
            {
                if (this.childTerms == null)
                {
                    this.childTerms = this.GetChildTerms(this.termId, this.vocabularyId);
                }

                return this.childTerms;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        [JsonIgnore]
        public bool IsHeirarchical
        {
            get
            {
                return this.Vocabulary.Type == VocabularyType.Hierarchy;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        [JsonIgnore]
        public int Left
        {
            get
            {
                return this.left;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        [JsonIgnore]
        public int Right
        {
            get
            {
                return this.right;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        [JsonIgnore]
        public List<string> Synonyms
        {
            get
            {
                return this.synonyms;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        [JsonIgnore]
        public Vocabulary Vocabulary
        {
            get
            {
                if (this.vocabulary == null && this.vocabularyId > Null.NullInteger)
                {
                    this.vocabulary = this.GetVocabulary(this.vocabularyId);
                }

                return this.vocabulary;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        [JsonIgnore]
        public int VocabularyId
        {
            get
            {
                return this.vocabularyId;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        [JsonIgnore]
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.description = Security.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        [JsonIgnore]
        public string Name
        {
            get
            {
                return this.name;
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

                this.name = Security.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        [JsonIgnore]
        public int? ParentTermId
        {
            get
            {
                return this.parentTermId;
            }

            set
            {
                this.parentTermId = value;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        [JsonIgnore]
        public int TermId
        {
            get
            {
                return this.termId;
            }

            set
            {
                this.termId = value;
            }
        }

        [XmlIgnore]
        [ScriptIgnore]
        [JsonIgnore]
        public int Weight
        {
            get
            {
                return this.weight;
            }

            set
            {
                this.weight = value;
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public virtual void Fill(IDataReader dr)
        {
            this.TermId = Null.SetNullInteger(dr["TermID"]);
            this.Name = Null.SetNullString(dr["Name"]);
            this.Description = Null.SetNullString(dr["Description"]);
            this.Weight = Null.SetNullInteger(dr["Weight"]);
            this.vocabularyId = Null.SetNullInteger(dr["VocabularyID"]);

            if (dr["TermLeft"] != DBNull.Value)
            {
                this.left = Convert.ToInt32(dr["TermLeft"], CultureInfo.InvariantCulture);
            }

            if (dr["TermRight"] != DBNull.Value)
            {
                this.right = Convert.ToInt32(dr["TermRight"], CultureInfo.InvariantCulture);
            }

            if (dr["ParentTermID"] != DBNull.Value)
            {
                this.ParentTermId = Convert.ToInt32(dr["ParentTermID"], CultureInfo.InvariantCulture);
            }

            // Fill base class properties
            this.FillInternal(dr);
        }

        public string GetTermPath()
        {
            string path = $@"\\{this.Name}";

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
