// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Entities
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// The document that will be deleted from search Index.
    /// </summary>
    /// <remarks>This is the base for all search documents (have ccommon properties) that can be used for deleting from the search index).</remarks>
    [Serializable]
    public class SearchDocumentToDelete
    {
        public SearchDocumentToDelete()
        {
            this.Keywords = new Dictionary<string, string>();
            this.NumericKeys = new Dictionary<string, int>();
            this.CultureCode = string.Empty;
            this.SearchTypeId =
                this.PortalId =
                this.RoleId = -1;
            this.ModuleDefId =
                this.ModuleId =
                this.TabId =
                this.AuthorUserId = 0;
        }

        /// <summary>
        /// Gets or sets a key to uniquely identify a document in the Index.
        /// </summary>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public string UniqueKey { get; set; }

        /// <summary>
        /// Gets or sets roleId (GroupId) for additional filtering [Optional].
        /// </summary>
        /// <remarks>This property can be used while under Social Groups.</remarks>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int RoleId { get; set; }

        /// <summary>
        /// Gets or sets portal Id.
        /// </summary>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int PortalId { get; set; }

        /// <summary>
        /// Gets or sets tab Id of the Content [Optional].
        /// </summary>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int TabId { get; set; }

        /// <summary>
        /// Gets or sets module Definition Id of the Content.
        /// </summary>
        /// <remarks>This is needed when SearchTypeId is for a Module.</remarks>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int ModuleDefId { get; set; }

        /// <summary>
        /// Gets or sets module Id of the Content.
        /// </summary>
        /// <remarks>This is needed when SearchTypeId is for a Module.</remarks>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int ModuleId { get; set; }

        /// <summary>
        /// Gets or sets user Id of the Author.
        /// </summary>
        /// <remarks>Author's display name is automatically found and stored. AuthorName can be found in SearchResult.
        /// However, this may get out of date if Display Name is changed after Index.</remarks>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int AuthorUserId { get; set; }

        /// <summary>
        /// Gets or sets search Type Id, e.g. module, file or url.
        /// </summary>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int SearchTypeId { get; set; }

        /// <summary>
        /// Gets or sets queryString that may be associated with a Search Document.
        /// </summary>
        /// <remarks>This information will be used to creare Url for the document.</remarks>
        /// <remarks>A value of NULL/EMPTY means this is property is not used.</remarks>
        public string QueryString { get; set; }

        /// <summary>
        /// Gets or sets additional keywords can be specified for Indexing.
        /// </summary>
        /// <remarks>This is key-value pair, e.g. "AliasName","something".</remarks>
        /// <remarks>An empty dictionary means this is property is not used.</remarks>
        public IDictionary<string, string> Keywords { get; set; }

        /// <summary>
        /// Gets or sets additional numeric fields can be specified for Indexing.
        /// </summary>
        /// <remarks>This is key-value pair, e.g. "ItemId","888".</remarks>
        /// <remarks>An empty dictionary means this is property is not used.</remarks>
        public IDictionary<string, int> NumericKeys { get; set; }

        /// <summary>
        /// Gets or sets culture Code associated with the content.
        /// </summary>
        /// <remarks>A value of NULL/EMPTY means this is property is not used.</remarks>
        public string CultureCode { get; set; }

        /// <summary>
        /// This is to allow saving current instance into DB as JSON entity.
        /// </summary>
        /// <returns></returns>
        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// This is overriden to present current instance as JSON string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToJsonString();
        }
    }
}
