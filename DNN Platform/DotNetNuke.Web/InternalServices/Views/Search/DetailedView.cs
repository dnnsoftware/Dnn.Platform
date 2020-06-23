// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.InternalServices.Views.Search
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Detailed Search Result View.
    /// </summary>
    public class DetailedView : BasicView
    {
        /// <summary>
        /// Gets or sets tags associated with Document.
        /// </summary>
        public IEnumerable<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets time when Content was last modified (in Utc).
        /// </summary>
        public string DisplayModifiedTime { get; set; }

        /// <summary>
        /// Gets or sets author profile URL.
        /// </summary>
        public string AuthorProfileUrl { get; set; }

        /// <summary>
        /// Gets or sets optional: Display Name of the Author.
        /// </summary>
        /// <remarks>This may be different form current Display Name when Index was run prior to change in Display Name.</remarks>
        public string AuthorName { get; set; }

        /// <summary>
        /// Gets or sets number of Likes associated with Content.
        /// </summary>
        /// <remarks>Content with more Likes is ranked higher.</remarks>
        public int Likes { get; set; }

        /// <summary>
        /// Gets or sets number of Comments associated with Content.
        /// </summary>
        /// <remarks>Content with more Comments is ranked higher.</remarks>
        public int Comments { get; set; }

        /// <summary>
        /// Gets or sets number of Views associated with Content.
        /// </summary>
        /// <remarks>Content with more Views is ranked higher.</remarks>
        public int Views { get; set; }
    }
}
