// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Web.InternalServices.Views.Search
{
    /// <summary>
    /// Detailed Search Result View
    /// </summary>
    public class DetailedView : BasicView
    {
        /// <summary>
        /// Tags associated with Document
        /// </summary>
        public IEnumerable<string> Tags { get; set; }
        
        /// <summary>
        /// Time when Content was last modified (in Utc)
        /// </summary>
        public String DisplayModifiedTime { get; set; }

        /// <summary>
        /// Author profile URL
        /// </summary>
        public string AuthorProfileUrl { get; set; }

        /// <summary>
        /// Optional: Display Name of the Author
        /// </summary>
        /// <remarks>This may be different form current Display Name when Index was run prior to change in Display Name.</remarks>
        public string AuthorName { get; set; }

        /// <summary>
        /// Number of Likes associated with Content.
        /// </summary>
        /// <remarks>Content with more Likes is ranked higher.</remarks>
        public int Likes { get; set; }

        /// <summary>
        /// Number of Comments associated with Content.
        /// </summary>
        /// <remarks>Content with more Comments is ranked higher.</remarks>
        public int Comments { get; set; }

        /// <summary>
        /// Number of Views associated with Content.
        /// </summary>
        /// <remarks>Content with more Views is ranked higher.</remarks>
        public int Views { get; set; }
        
    }
}
