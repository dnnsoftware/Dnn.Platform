// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html;

using System.Collections;
using System.Web.Caching;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.Html.Components;

/// <summary>The WorkflowStateController is the Controller class for managing workflows and states for the HtmlText module.</summary>
public class WorkflowStateController
{
    private const string WORKFLOWCACHEKEY = "Workflow{0}";
    private const string WORKFLOWSCACHEKEY = "Workflows{0}";
    private const int WORKFLOWCACHETIMEOUT = 20;

    private const CacheItemPriority WORKFLOWCACHEPRIORITY = CacheItemPriority.Normal;

    /// <summary>GetWorkFlows retrieves a collection of workflows for the portal from the cache.</summary>
    /// <param name="portalID">The ID of the Portal.</param>
    /// <returns>An <see cref="ArrayList"/> of <see cref="WorkflowStateInfo"/> instances.</returns>
    public ArrayList GetWorkflows(int portalID)
    {
        var cacheKey = string.Format(WORKFLOWSCACHEKEY, portalID);
        return CBO.GetCachedObject<ArrayList>(new CacheItemArgs(cacheKey, WORKFLOWCACHETIMEOUT, WORKFLOWCACHEPRIORITY, portalID), this.GetWorkflowsCallBack);
    }

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   GetWorkflowsCallBack retrieves a collection of WorkflowStateInfo objects for the Portal from the database.
    /// </summary>
    /// <param name = "cacheItemArgs">Arguments passed by the GetWorkflowStates method.</param>
    /// <returns>WorkflowStateInfo List.</returns>
    /// -----------------------------------------------------------------------------
    public ArrayList GetWorkflowsCallBack(CacheItemArgs cacheItemArgs)
    {
        var portalId = (int)cacheItemArgs.ParamList[0];
        return CBO.FillCollection(DataProvider.Instance().GetWorkflows(portalId), typeof(WorkflowStateInfo));
    }

    /// <summary>GetWorkFlowStates retrieves a collection of WorkflowStateInfo objects for the Workflow from the cache.</summary>
    /// <param name="workflowID">The ID of the Workflow.</param>
    /// <returns>An <see cref="ArrayList"/> of <see cref="WorkflowStateInfo"/> instances.</returns>
    public ArrayList GetWorkflowStates(int workflowID)
    {
        string cacheKey = string.Format(WORKFLOWCACHEKEY, workflowID);
        return CBO.GetCachedObject<ArrayList>(new CacheItemArgs(cacheKey, WORKFLOWCACHETIMEOUT, WORKFLOWCACHEPRIORITY, workflowID), this.GetWorkflowStatesCallBack);
    }

    /// <summary>GetWorkFlowStatesCallback retrieves a collection of WorkflowStateInfo objects for the Workflow from the database.</summary>
    /// <param name="cacheItemArgs">Arguments passed by the GetWorkflowStates method.</param>
    /// <returns>An <see cref="ArrayList"/> of <see cref="WorkflowStateInfo"/> instances.</returns>
    public object GetWorkflowStatesCallBack(CacheItemArgs cacheItemArgs)
    {
        var workflowID = (int)cacheItemArgs.ParamList[0];
        return CBO.FillCollection(DataProvider.Instance().GetWorkflowStates(workflowID), typeof(WorkflowStateInfo));
    }

    /// <summary>GetFirstWorkFlowStateID retrieves the first StateID for the Workflow.</summary>
    /// <param name="workflowID">The ID of the Workflow.</param>
    /// <returns>The workflow state ID or <c>-1</c>.</returns>
    public int GetFirstWorkflowStateID(int workflowID)
    {
        int intStateID = -1;
        ArrayList arrWorkflowStates = this.GetWorkflowStates(workflowID);
        if (arrWorkflowStates.Count > 0)
        {
            intStateID = ((WorkflowStateInfo)arrWorkflowStates[0]).StateID;
        }

        return intStateID;
    }

    /// <summary>GetPreviousWorkFlowStateID retrieves the previous StateID for the Workflow and State specified.</summary>
    /// <param name="workflowID">The ID of the Workflow.</param>
    /// <param name="stateID">The ID of the State.</param>
    /// <returns>The previous workflow state ID (or the first workflow state ID if a previous state cannot be found).</returns>
    public int GetPreviousWorkflowStateID(int workflowID, int stateID)
    {
        int intPreviousStateID = -1;
        ArrayList arrWorkflowStates = this.GetWorkflowStates(workflowID);
        int intItem = 0;

        // locate the current state
        for (intItem = 0; intItem < arrWorkflowStates.Count; intItem++)
        {
            if (((WorkflowStateInfo)arrWorkflowStates[intItem]).StateID == stateID)
            {
                intPreviousStateID = stateID;
                break;
            }
        }

        // get previous active state
        if (intPreviousStateID == stateID)
        {
            intItem = intItem - 1;
            while (intItem >= 0)
            {
                if (((WorkflowStateInfo)arrWorkflowStates[intItem]).IsActive)
                {
                    intPreviousStateID = ((WorkflowStateInfo)arrWorkflowStates[intItem]).StateID;
                    break;
                }

                intItem = intItem - 1;
            }
        }

        // if none found then reset to first state
        if (intPreviousStateID == -1)
        {
            intPreviousStateID = this.GetFirstWorkflowStateID(workflowID);
        }

        return intPreviousStateID;
    }

    /// <summary>GetNextWorkFlowStateID retrieves the next StateID for the Workflow and State specified.</summary>
    /// <param name="workflowID">The ID of the Workflow.</param>
    /// <param name="stateID">The ID of the State.</param>
    /// <returns>The next workflow state ID (or the first workflow state ID if a next state cannot be found).</returns>
    public int GetNextWorkflowStateID(int workflowID, int stateID)
    {
        int intNextStateID = -1;
        ArrayList arrWorkflowStates = this.GetWorkflowStates(workflowID);
        int intItem = 0;

        // locate the current state
        for (intItem = 0; intItem < arrWorkflowStates.Count; intItem++)
        {
            if (((WorkflowStateInfo)arrWorkflowStates[intItem]).StateID == stateID)
            {
                intNextStateID = stateID;
                break;
            }
        }

        // get next active state
        if (intNextStateID == stateID)
        {
            intItem = intItem + 1;
            while (intItem < arrWorkflowStates.Count)
            {
                if (((WorkflowStateInfo)arrWorkflowStates[intItem]).IsActive)
                {
                    intNextStateID = ((WorkflowStateInfo)arrWorkflowStates[intItem]).StateID;
                    break;
                }

                intItem = intItem + 1;
            }
        }

        // if none found then reset to first state
        if (intNextStateID == -1)
        {
            intNextStateID = this.GetFirstWorkflowStateID(workflowID);
        }

        return intNextStateID;
    }

    /// <summary>GetLastWorkFlowStateID retrieves the last StateID for the Workflow.</summary>
    /// <param name="workflowID">The ID of the Workflow.</param>
    /// <returns>The workflow state ID or <c>-1</c>.</returns>
    public int GetLastWorkflowStateID(int workflowID)
    {
        int intStateID = -1;
        ArrayList arrWorkflowStates = this.GetWorkflowStates(workflowID);
        if (arrWorkflowStates.Count > 0)
        {
            intStateID = ((WorkflowStateInfo)arrWorkflowStates[arrWorkflowStates.Count - 1]).StateID;
        }

        return intStateID;
    }
}
