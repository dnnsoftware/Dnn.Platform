
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Collections.Generic;

namespace DotNetNuke.Web.InternalServices.Views.Search
{
    /// <summary>
    /// DetailedView grouped by Url (TabId)
    /// </summary>
    public class GroupedDetailView
    {
        /// <summary>
        /// Document's Url
        /// </summary>
        public string DocumentUrl { get; set; }

        /// <summary>
        /// Document's Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Results of the Search
        /// </summary>
        public List<DetailedView> Results { get; set; }

        public GroupedDetailView()
        {
            this.Results = new List<DetailedView>();
        }
    }
}
