// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Components;

using System;
using System.Data;

/// <summary>  The DataProvider is an abstract class that provides the Data Access Layer for the HtmlText module.</summary>
public class DataProvider
{
    // singleton reference to the instantiated object
    private static readonly DataProvider Provider;

    // constructor
    static DataProvider()
    {
        Provider = new DataProvider();
    }

    // return the provider
    public static DataProvider Instance()
    {
        return Provider;
    }

    public virtual IDataReader GetHtmlText(int moduleID, int itemID)
    {
        return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetHtmlText", moduleID, itemID);
    }

    public virtual IDataReader GetTopHtmlText(int moduleID, bool isPublished)
    {
        return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetTopHtmlText", moduleID, isPublished);
    }

    public virtual IDataReader GetAllHtmlText(int moduleID)
    {
        return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetAllHtmlText", moduleID);
    }

    public virtual int AddHtmlText(int moduleID, string content, string summary, int stateID, bool isPublished, int createdByUserID, int history)
    {
        return DotNetNuke.Data.DataProvider.Instance().ExecuteScalar<int>("AddHtmlText", moduleID, content, summary, stateID, isPublished, createdByUserID, history);
    }

    public virtual void UpdateHtmlText(int itemID, string content, string summary, int stateID, bool isPublished, int lastModifiedByUserID)
    {
        DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("UpdateHtmlText", itemID, content, summary, stateID, isPublished, lastModifiedByUserID);
    }

    public virtual void DeleteHtmlText(int moduleID, int itemID)
    {
        DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("DeleteHtmlText", moduleID, itemID);
    }

    public virtual IDataReader GetHtmlTextLog(int itemID)
    {
        return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetHtmlTextLog", itemID);
    }

    public virtual void AddHtmlTextLog(int itemID, int stateID, string comment, bool approved, int createdByUserID)
    {
        DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("AddHtmlTextLog", itemID, stateID, comment, approved, createdByUserID);
    }

    public virtual IDataReader GetHtmlTextUser(int userID)
    {
        return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetHtmlTextUser", userID);
    }

    public virtual void AddHtmlTextUser(int itemID, int stateID, int moduleID, int tabID, int userID)
    {
        DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("AddHtmlTextUser", itemID, stateID, moduleID, tabID, userID);
    }

    public virtual void DeleteHtmlTextUsers()
    {
        DotNetNuke.Data.DataProvider.Instance().ExecuteNonQuery("DeleteHtmlTextUsers");
    }

    public virtual IDataReader GetWorkflows(int portalID)
    {
        return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetWorkflows", portalID);
    }

    public virtual IDataReader GetWorkflowStates(int workflowID)
    {
        return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetWorkflowStates", workflowID);
    }

    public virtual IDataReader GetWorkflowStatePermissions()
    {
        return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetWorkflowStatePermissions");
    }

    public virtual IDataReader GetWorkflowStatePermissionsByStateID(int stateID)
    {
        return DotNetNuke.Data.DataProvider.Instance().ExecuteReader("GetWorkflowStatePermissionsByStateID", stateID);
    }
}
