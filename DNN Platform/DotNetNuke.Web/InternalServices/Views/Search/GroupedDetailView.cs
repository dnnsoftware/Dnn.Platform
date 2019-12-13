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
