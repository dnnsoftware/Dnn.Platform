﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.Html.Components;


namespace DotNetNuke.Modules.Html
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      HtmlTextUserController
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The HtmlTextUserController is the Controller class for managing User information the HtmlText module
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class HtmlTextUserController
    {
        #region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetHtmlTextUser retrieves a collection of HtmlTextUserInfo objects for an Item
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "UserID">The Id of the User</param>
        /// -----------------------------------------------------------------------------
        public ArrayList GetHtmlTextUser(int UserID)
        {
            return CBO.FillCollection(DataProvider.Instance().GetHtmlTextUser(UserID), typeof (HtmlTextUserInfo));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   AddHtmlTextUser creates a new HtmlTextUser for an Item
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "objHtmlTextUser">An HtmlTextUserInfo object</param>
        /// -----------------------------------------------------------------------------
        public void AddHtmlTextUser(HtmlTextUserInfo objHtmlTextUser)
        {
            DataProvider.Instance().AddHtmlTextUser(objHtmlTextUser.ItemID, objHtmlTextUser.StateID, objHtmlTextUser.ModuleID, objHtmlTextUser.TabID, objHtmlTextUser.UserID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   DeleteHtmlTextUsers cleans up old HtmlTextUser records
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void DeleteHtmlTextUsers()
        {
            DataProvider.Instance().DeleteHtmlTextUsers();
        }

        #endregion
    }
}
