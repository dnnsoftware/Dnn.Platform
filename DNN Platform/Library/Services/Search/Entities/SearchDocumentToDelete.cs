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
using Newtonsoft.Json;

#endregion

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
            Keywords = new Dictionary<string, string>();
            NumericKeys = new Dictionary<string, int>();
            CultureCode = string.Empty;
            SearchTypeId =
                PortalId =
                RoleId = -1;
            ModuleDefId =
                ModuleId =
                TabId =
                AuthorUserId = 0;
        }

        /// <summary>
        /// This is overriden to allow saving into DB using object.ToString() as JSON entity
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
