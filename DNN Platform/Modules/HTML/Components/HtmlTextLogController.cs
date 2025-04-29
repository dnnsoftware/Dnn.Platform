// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html;

using System.Collections;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.Html.Components;

/// <summary>The HtmlTextLogController is the Controller class for managing Log History information the HtmlText module.</summary>
public class HtmlTextLogController
{
    /// <summary>GetHtmlTextLog retrieves a collection of HtmlTextLogInfo objects for an Item.</summary>
    /// <param name="itemID">The Id of the Item.</param>
    /// <returns>An <see cref="ArrayList"/> of <see cref="HtmlTextLogInfo"/> instances.</returns>
    public ArrayList GetHtmlTextLog(int itemID)
    {
        return CBO.FillCollection(DataProvider.Instance().GetHtmlTextLog(itemID), typeof(HtmlTextLogInfo));
    }

    /// <summary>AddHtmlTextLog creates a new HtmlTextLog for an Item.</summary>
    /// <param name="objHtmlTextLog">An HtmlTextLogInfo object.</param>
    public void AddHtmlTextLog(HtmlTextLogInfo objHtmlTextLog)
    {
        DataProvider.Instance().AddHtmlTextLog(objHtmlTextLog.ItemID, objHtmlTextLog.StateID, objHtmlTextLog.Comment, objHtmlTextLog.Approved, UserController.Instance.GetCurrentUserInfo().UserID);
    }
}
