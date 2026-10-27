// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.PageLinks
{
    using System.Runtime.Serialization;

    /// <summary>A page the editor can link to.</summary>
    [DataContract]
    public class PageLink
    {
        /// <summary>Gets or sets the page's tab ID.</summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>Gets or sets the page name.</summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>Gets or sets the page's depth in the page tree (0 for top-level pages).</summary>
        [DataMember(Name = "level")]
        public int Level { get; set; }

        /// <summary>Gets or sets the site-relative URL to link to.</summary>
        [DataMember(Name = "url")]
        public string Url { get; set; }

        /// <summary>Gets or sets a value indicating whether the page can be linked to; pages with links disabled are listed only to show the tree.</summary>
        [DataMember(Name = "selectable")]
        public bool Selectable { get; set; }
    }
}
