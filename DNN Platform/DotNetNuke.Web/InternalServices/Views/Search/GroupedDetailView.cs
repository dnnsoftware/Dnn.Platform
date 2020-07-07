// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.InternalServices.Views.Search
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// DetailedView grouped by Url (TabId).
    /// </summary>
    public class GroupedDetailView
    {
        public GroupedDetailView()
        {
            this.Results = new List<DetailedView>();
        }

        /// <summary>
        /// Gets or sets document's Url.
        /// </summary>
        public string DocumentUrl { get; set; }

        /// <summary>
        /// Gets or sets document's Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets results of the Search.
        /// </summary>
        public List<DetailedView> Results { get; set; }
    }
}
