// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Entities
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The document that will be stored in Search Index.
    /// </summary>
    /// <remarks>Each document is one discrete unit of content to be indexed and is independent from other Documents.</remarks>
    [Serializable]
    public class SearchDocument : SearchDocumentToDelete
    {
        public SearchDocument()
        {
            this.Tags = new string[0];
            this.IsActive = true;
        }

        /// <summary>
        /// Gets or sets content's Title.
        /// </summary>
        /// <remarks>
        /// HTML tags are stripped from this property, but certain HTML attribute values will be retain, ie. alt and title attribute values.
        /// </remarks>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets content's Description.
        /// </summary>
        /// <remarks>
        /// Description should generally be no more than two sentences. This property is used by RSS Syndication. It is also used in search result when highlighted text is not found during searching.
        /// HTML tags are stripped from this property, but certain HTML attribute values will be retain, ie. alt and title attribute values.
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets content's Body.
        /// </summary>
        /// <remarks>
        /// HTML tags are stripped from this property, but certain HTML attribute values will be retain, ie. alt and title attribute values.
        /// </remarks>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets url for the indexed item.
        /// </summary>
        /// <remarks>Usually TabId or ModuleId is enough to generate Document Url in Search Result. However, Url field is used if present in SearchResult.</remarks>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets time when Content was last modified (in Utc).
        /// </summary>
        public DateTime ModifiedTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether flag to indicate if Content is Active or Not. Content will be deleted from Index when IsActive = false. Default is True.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a string representation of roles and users who have view (or denied view) permissions.
        /// </summary>
        /// <remarks>The Permission property is same as how it�s implement internally in the Platform. Allow or Deny permission can be specified for RoleNamess and / or UserIds.
        /// A semicolon must be specified to separate two RoleName or UserId.
        ///     "!Translator (en-US);![3];[5];Administrators; ContentEditorRole"
        ///     ! -- identifies denied permission
        ///     [n] -- identifies UserId
        /// Above example denies permission to Role �Translator (en-Us)� and UserId 3, but allows permission to Roles �Administrators� and �ContentEditorRole� and UserId.</remarks>
        public string Permissions { get; set; }

        /// <summary>
        /// Gets or sets tags can be specified as additional information.
        /// </summary>
        public IEnumerable<string> Tags { get; set; }

        public override string ToString()
        {
            return string.Join(", ", new[]
            {
                "Portal ID: " + this.PortalId,
                "Tab ID: " + this.TabId,
                "Module ID: " + this.ModuleId,
                "Mod. Def.ID: " + this.ModuleDefId,
                "Url: " + this.Url,
                "Unique Key: " + this.UniqueKey,
                "Last Modified: " + this.ModifiedTimeUtc.ToString("O"),
                "Culture: " + this.CultureCode,
                "Search Type: " + this.SearchTypeId,
            });
        }
    }
}
