#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

#endregion

namespace DotNetNuke.Services.Search.Entities
{
    /// <summary>
    /// The document that will be stored in Search Index
    /// </summary>
    /// <remarks>Each document is one discrete unit of content to be indexed and is independent from other Documents</remarks>
    [Serializable]
    public class SearchDocument : SearchDocumentToDelete
    {
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
        /// Time when Content was last modified (in Utc)
        /// </summary>
        public DateTime ModifiedTimeUtc { get; set; }

        /// <summary>
        /// Flag to indicate if Content is Active or Not. Content will be deleted from Index when IsActive = false. Default is True.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// A string representation of roles and users who have view (or denied view) permissions
        /// </summary>
        /// <remarks>The Permission property is same as how it’s implement internally in the Platform. Allow or Deny permission can be specified for RoleNamess and / or UserIds. 
        /// A semicolon must be specified to separate two RoleName or UserId.
        ///     "!Translator (en-US);![3];[5];Administrators; ContentEditorRole"
        ///     ! -- identifies denied permission
        ///     [n] -- identifies UserId
        ///Above example denies permission to Role “Translator (en-Us)” and UserId 3, but allows permission to Roles “Administrators” and “ContentEditorRole” and UserId</remarks>
        public string Permissions { get; set; }

        /// <summary>
        /// Tags can be specified as additional information
        /// </summary>
        public IEnumerable<string> Tags { get; set; }

        #region constructor

        public SearchDocument()
        {
            Tags = new string[0];
            IsActive = true;
        }

        #endregion

        public override string ToString()
        {
            return string.Join(", ", new[]
            {
                "Portal ID: " + PortalId,
                "Tab ID: " + TabId,
                "Module ID: " + ModuleId,
                "Mod. Def.ID: " + ModuleDefId,
                "Url: " + Url,
                "Unique Key: " + UniqueKey,
                "Last Modified: " + ModifiedTimeUtc.ToString("O"),
                "Culture: " + CultureCode,
                "Search Type: " + SearchTypeId
            });
        }
    }
}
