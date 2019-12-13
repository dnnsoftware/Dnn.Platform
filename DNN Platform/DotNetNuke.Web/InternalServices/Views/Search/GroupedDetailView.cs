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
            Results = new List<DetailedView>();
        }

        #endregion
    }
}
