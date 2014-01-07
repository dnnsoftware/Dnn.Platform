#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Services.Search.Entities
{
    /// <summary>
    /// The document that will be stored in Search Index
    /// </summary>
    /// <remarks>Each document is one discrete unit of content to be indexed and is independent from other Documents</remarks>
    [Serializable]
    public class SearchDocument
    {
        /// <summary>
        /// A key to uniquely identify a document in the Index
        /// </summary>
        public string UniqueKey { get; set; }

        /// <summary>
        /// Content's Title      
        /// </summary>
        /// <remarks>
        /// HTML tags are stripped from this property, but certain HTML attribute values will be retain, ie. alt and title attribute values.
        /// </remarks>
        public string Title { get; set; }

        /// <summary>
        /// Content's Description
        /// </summary>
        /// <remarks>
        /// Description should generally be no more than two sentences. This property is used by RSS Syndication. It is also used in search result when highlighted text is not found during searching.
        /// HTML tags are stripped from this property, but certain HTML attribute values will be retain, ie. alt and title attribute values.
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        /// RoleId (GroupId) for additional filtering [Optional]
        /// </summary>
        /// <remarks> This property can be used while under Social Groups.</remarks>
        public int RoleId { get; set; }

        /// <summary>
        ///Content's Body
        /// </summary>
        /// <remarks>
        /// HTML tags are stripped from this property, but certain HTML attribute values will be retain, ie. alt and title attribute values.
        /// </remarks>
        public string Body { get; set; }

        /// <summary>
        /// Url for the indexed item.
        /// </summary>
        /// <remarks>Usually TabId or ModuleId is enough to generate Document Url in Search Result. However, Url field is used if present in SearchResult</remarks>
        public string Url { get; set; }

        /// <summary>
        /// Portal Id
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// Tab Id of the Content [Optional]
        /// </summary>
        public int TabId { get; set; }

        /// <summary>
        /// Module Definition Id of the Content.
        /// </summary>
        /// <remarks>This is needed when SearchTypeId is for a Module</remarks>
        public int ModuleDefId { get; set; }

        /// <summary>
        /// Module Id of the Content
        /// </summary>
        /// <remarks>This is needed when SearchTypeId is for a Module</remarks>
        public int ModuleId { get; set; }

        /// <summary>
        /// User Id of the Author
        /// </summary>
        /// <remarks>Author's display name is automatically found and stored. AuthorName can be found in SearchResult. 
        /// However, this may get out of date if Display Name is changed after Index.</remarks>
        public int AuthorUserId { get; set; }

        /// <summary>
        /// Search Type Id, e.g. module, file or url
        /// </summary>
        public int SearchTypeId { get; set; }

        /// <summary>
        /// Time when Content was last modified (in Utc)
        /// </summary>
        public DateTime ModifiedTimeUtc { get; set; }

        /// <summary>
        /// Flag to indicate if Content is Active or Not. Content will be deleted from Index when IsActive = false. Default is True.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// QueryString that may be associated with a Search Document. 
        /// </summary>
        /// <remarks>This information will be used to creare Url for the document</remarks>
        public string QueryString { get; set; }

        /// <summary>
        /// A string representation of roles and users who have view (or denied view) permissions
        /// </summary>
        public string Permissions { get; set; }

        /// <summary>
        /// Additional keywords can be specified for Indexing
        /// </summary>
        /// <remarks>This is key-value pair, e.g. "AliasName","something"</remarks>
        public IDictionary<string, string> Keywords { get; set; }

        /// <summary>
        /// Additional numeric fields can be specified for Indexing
        /// </summary>
        /// <remarks>This is key-value pair, e.g. "ItemId","888"</remarks>
        public IDictionary<string, int> NumericKeys { get; set; }

        /// <summary>
        /// Tags can be specified as additional information
        /// </summary>
        public IEnumerable<string> Tags { get; set; }

        /// <summary>
        /// Culture Code associated with the content. 
        /// </summary>
        public string CultureCode { get; set; }

        #region constructor
        public SearchDocument()
        {
            Keywords = new Dictionary<string, string>();
            NumericKeys = new Dictionary<string, int>();
            Tags = new string[0];
            IsActive = true;
            CultureCode = string.Empty;
        }
        #endregion 

        public override string ToString()
        {
            var data = new string[]
                {
                    "Portal ID: " + PortalId.ToString(),
                    "Tab ID: " + TabId.ToString(),
                    "Module ID: " + ModuleId.ToString(),
                    "Mod. Def.ID: " + ModuleDefId.ToString(),
                    "Url: " + Url,
                    "Unique Key: " + UniqueKey,
                    "Last Modified: " + ModifiedTimeUtc.ToString("o"),
                    "Culture: " + CultureCode,
                };
            return string.Join(", ", data);
        }
    }
}