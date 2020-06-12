// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections.Generic;

#endregion

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

        #region constructor

        public GroupedDetailView()
        {
            this.Results = new List<DetailedView>();
        }

        #endregion
    }
}
