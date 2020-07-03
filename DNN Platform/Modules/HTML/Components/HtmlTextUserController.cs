// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html
{
    using System.Collections;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Modules.Html.Components;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      HtmlTextUserController
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The HtmlTextUserController is the Controller class for managing User information the HtmlText module.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class HtmlTextUserController
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetHtmlTextUser retrieves a collection of HtmlTextUserInfo objects for an Item.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "UserID">The Id of the User.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public ArrayList GetHtmlTextUser(int UserID)
        {
            return CBO.FillCollection(DataProvider.Instance().GetHtmlTextUser(UserID), typeof(HtmlTextUserInfo));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   AddHtmlTextUser creates a new HtmlTextUser for an Item.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "objHtmlTextUser">An HtmlTextUserInfo object.</param>
        /// -----------------------------------------------------------------------------
        public void AddHtmlTextUser(HtmlTextUserInfo objHtmlTextUser)
        {
            DataProvider.Instance().AddHtmlTextUser(objHtmlTextUser.ItemID, objHtmlTextUser.StateID, objHtmlTextUser.ModuleID, objHtmlTextUser.TabID, objHtmlTextUser.UserID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   DeleteHtmlTextUsers cleans up old HtmlTextUser records.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void DeleteHtmlTextUsers()
        {
            DataProvider.Instance().DeleteHtmlTextUsers();
        }
    }
}
