// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html
{
    using System.Collections;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.Html.Components;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      HtmlTextLogController
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The HtmlTextLogController is the Controller class for managing Log History information the HtmlText module.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class HtmlTextLogController
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetHtmlTextLog retrieves a collection of HtmlTextLogInfo objects for an Item.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ItemID">The Id of the Item.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public ArrayList GetHtmlTextLog(int ItemID)
        {
            return CBO.FillCollection(DataProvider.Instance().GetHtmlTextLog(ItemID), typeof(HtmlTextLogInfo));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   AddHtmlTextLog creates a new HtmlTextLog for an Item.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "objHtmlTextLog">An HtmlTextLogInfo object.</param>
        /// -----------------------------------------------------------------------------
        public void AddHtmlTextLog(HtmlTextLogInfo objHtmlTextLog)
        {
            DataProvider.Instance().AddHtmlTextLog(objHtmlTextLog.ItemID, objHtmlTextLog.StateID, objHtmlTextLog.Comment, objHtmlTextLog.Approved, UserController.Instance.GetCurrentUserInfo().UserID);
        }
    }
}
