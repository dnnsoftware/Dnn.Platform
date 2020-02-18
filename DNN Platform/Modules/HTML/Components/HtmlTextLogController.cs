// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.Html.Components;


namespace DotNetNuke.Modules.Html
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      HtmlTextLogController
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The HtmlTextLogController is the Controller class for managing Log History information the HtmlText module
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class HtmlTextLogController
    {
        #region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetHtmlTextLog retrieves a collection of HtmlTextLogInfo objects for an Item
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ItemID">The Id of the Item</param>
        /// -----------------------------------------------------------------------------
        public ArrayList GetHtmlTextLog(int ItemID)
        {
            return CBO.FillCollection(DataProvider.Instance().GetHtmlTextLog(ItemID), typeof (HtmlTextLogInfo));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   AddHtmlTextLog creates a new HtmlTextLog for an Item
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "objHtmlTextLog">An HtmlTextLogInfo object</param>
        /// -----------------------------------------------------------------------------
        public void AddHtmlTextLog(HtmlTextLogInfo objHtmlTextLog)
        {
            DataProvider.Instance().AddHtmlTextLog(objHtmlTextLog.ItemID, objHtmlTextLog.StateID, objHtmlTextLog.Comment, objHtmlTextLog.Approved, UserController.Instance.GetCurrentUserInfo().UserID);
        }

        #endregion
    }
}
