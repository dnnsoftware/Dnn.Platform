

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Collections.Generic;
using DotNetNuke.Services.Search.Internals;

namespace DotNetNuke.Web.InternalServices.Views.Search
{
    /// <summary>
    /// Detailed Search Result View
    /// </summary>
    public class BasicView
    {
        /// <summary>
        /// Document's Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Highlighted snippet from document
        /// </summary>
        public string Snippet { get; set; }

        /// <summary>
        /// Description from document
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Link to the Document
        /// </summary>
        public string DocumentUrl { get; set; }

        /// <summary>
        /// Display Name of the Document Type
        /// </summary>
        public string DocumentTypeName { get; set; }

        /// <summary>
        /// Custom Attributes of the document.
        /// </summary>
        public IDictionary<string, string> Attributes { get; set; }

        public BasicView()
        {
            this.Attributes = new Dictionary<string, string>();
        }
    }
}
