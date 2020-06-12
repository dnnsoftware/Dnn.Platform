

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetNuke.Services.Search.Entities
{
    /// <summary>
    /// The document that will be deleted from search Index
    /// </summary>
    /// <remarks>This is the base for all search documents (have ccommon properties) that can be used for deleting from the search index)</remarks>
    [Serializable]
    public class SearchDocumentToDelete
    {
        /// <summary>
        /// A key to uniquely identify a document in the Index
        /// </summary>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public string UniqueKey { get; set; }

        /// <summary>
        /// RoleId (GroupId) for additional filtering [Optional]
        /// </summary>
        /// <remarks>This property can be used while under Social Groups.</remarks>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int RoleId { get; set; }

        /// <summary>
        /// Portal Id
        /// </summary>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int PortalId { get; set; }

        /// <summary>
        /// Tab Id of the Content [Optional]
        /// </summary>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int TabId { get; set; }

        /// <summary>
        /// Module Definition Id of the Content.
        /// </summary>
        /// <remarks>This is needed when SearchTypeId is for a Module</remarks>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int ModuleDefId { get; set; }

        /// <summary>
        /// Module Id of the Content
        /// </summary>
        /// <remarks>This is needed when SearchTypeId is for a Module</remarks>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int ModuleId { get; set; }

        /// <summary>
        /// User Id of the Author
        /// </summary>
        /// <remarks>Author's display name is automatically found and stored. AuthorName can be found in SearchResult.
        /// However, this may get out of date if Display Name is changed after Index.</remarks>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int AuthorUserId { get; set; }

        /// <summary>
        /// Search Type Id, e.g. module, file or url
        /// </summary>
        /// <remarks>A value of -1 means this is property is not used.</remarks>
        public int SearchTypeId { get; set; }

        /// <summary>
        /// QueryString that may be associated with a Search Document.
        /// </summary>
        /// <remarks>This information will be used to creare Url for the document</remarks>
        /// <remarks>A value of NULL/EMPTY means this is property is not used.</remarks>
        public string QueryString { get; set; }

        /// <summary>
        /// Additional keywords can be specified for Indexing
        /// </summary>
        /// <remarks>This is key-value pair, e.g. "AliasName","something"</remarks>
        /// <remarks>An empty dictionary means this is property is not used.</remarks>
        public IDictionary<string, string> Keywords { get; set; }

        /// <summary>
        /// Additional numeric fields can be specified for Indexing
        /// </summary>
        /// <remarks>This is key-value pair, e.g. "ItemId","888"</remarks>
        /// <remarks>An empty dictionary means this is property is not used.</remarks>
        public IDictionary<string, int> NumericKeys { get; set; }

        /// <summary>
        /// Culture Code associated with the content.
        /// </summary>
        /// <remarks>A value of NULL/EMPTY means this is property is not used.</remarks>
        public string CultureCode { get; set; }

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
        /// This is to allow saving current instance into DB as JSON entity
        /// </summary>
        /// <returns></returns>
        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// This is overriden to present current instance as JSON string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToJsonString();
        }
    }
}
