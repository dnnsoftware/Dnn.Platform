// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html;

using System.Collections;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.Html.Components;

/// <summary>The HtmlTextUserController is the Controller class for managing User information the HtmlText module.</summary>
public class HtmlTextUserController
{
    /// <summary>GetHtmlTextUser retrieves a collection of HtmlTextUserInfo objects for an Item.</summary>
    /// <param name="userID">The Id of the User.</param>
    /// <returns>An <see cref="ArrayList"/> of <see cref="HtmlTextUserInfo"/> instances.</returns>
    public ArrayList GetHtmlTextUser(int userID)
    {
        return CBO.FillCollection(DataProvider.Instance().GetHtmlTextUser(userID), typeof(HtmlTextUserInfo));
    }

    /// <summary>AddHtmlTextUser creates a new HtmlTextUser for an Item.</summary>
    /// <param name="objHtmlTextUser">An HtmlTextUserInfo object.</param>
    public void AddHtmlTextUser(HtmlTextUserInfo objHtmlTextUser)
    {
        DataProvider.Instance().AddHtmlTextUser(objHtmlTextUser.ItemID, objHtmlTextUser.StateID, objHtmlTextUser.ModuleID, objHtmlTextUser.TabID, objHtmlTextUser.UserID);
    }

    /// <summary>DeleteHtmlTextUsers cleans up old HtmlTextUser records.</summary>
    public void DeleteHtmlTextUsers()
    {
        DataProvider.Instance().DeleteHtmlTextUsers();
    }
}
