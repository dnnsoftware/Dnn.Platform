#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System.Collections;
using System.Web.Caching;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.Html.Components;


namespace DotNetNuke.Modules.Html
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      WorkflowStateController
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The WorkflowStateController is the Controller class for managing workflows and states for the HtmlText module
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class WorkflowStateController
    {
        private const string WORKFLOW_CACHE_KEY = "Workflow{0}";
        private const int WORKFLOW_CACHE_TIMEOUT = 20;

        private const CacheItemPriority WORKFLOW_CACHE_PRIORITY = CacheItemPriority.Normal;

        #region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetWorkFlows retrieves a collection of workflows for the portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "PortalID">The ID of the Portal</param>
        /// -----------------------------------------------------------------------------
        public ArrayList GetWorkflows(int PortalID)
        {
            return CBO.FillCollection(DataProvider.Instance().GetWorkflows(PortalID), typeof (WorkflowStateInfo));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetWorkFlowStates retrieves a collection of WorkflowStateInfo objects for the Workflow from the cache
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "WorkflowID">The ID of the Workflow</param>
        /// -----------------------------------------------------------------------------
        public ArrayList GetWorkflowStates(int WorkflowID)
        {
            string cacheKey = string.Format(WORKFLOW_CACHE_KEY, WorkflowID);
            return CBO.GetCachedObject<ArrayList>(new CacheItemArgs(cacheKey, WORKFLOW_CACHE_TIMEOUT, WORKFLOW_CACHE_PRIORITY, WorkflowID), GetWorkflowStatesCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetWorkFlowStatesCallback retrieves a collection of WorkflowStateInfo objects for the Workflow from the database
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "cacheItemArgs">Arguments passed by the GetWorkflowStates method</param>
        /// -----------------------------------------------------------------------------
        public object GetWorkflowStatesCallBack(CacheItemArgs cacheItemArgs)
        {
            var WorkflowID = (int) (cacheItemArgs.ParamList[0]);
            return CBO.FillCollection(DataProvider.Instance().GetWorkflowStates(WorkflowID), typeof (WorkflowStateInfo));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetFirstWorkFlowStateID retrieves the first StateID for the Workflow
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "WorkflowID">The ID of the Workflow</param>
        /// -----------------------------------------------------------------------------
        public int GetFirstWorkflowStateID(int WorkflowID)
        {
            int intStateID = -1;
            ArrayList arrWorkflowStates = GetWorkflowStates(WorkflowID);
            if (arrWorkflowStates.Count > 0)
            {
                intStateID = ((WorkflowStateInfo) (arrWorkflowStates[0])).StateID;
            }
            return intStateID;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetPreviousWorkFlowStateID retrieves the previous StateID for the Workflow and State specified
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "WorkflowID">The ID of the Workflow</param>
        /// <param name = "StateID">The ID of the State</param>
        /// -----------------------------------------------------------------------------
        public int GetPreviousWorkflowStateID(int WorkflowID, int StateID)
        {
            int intPreviousStateID = -1;
            ArrayList arrWorkflowStates = GetWorkflowStates(WorkflowID);
            int intItem = 0;

            // locate the current state
            for (intItem = 0; intItem < arrWorkflowStates.Count; intItem++)
            {
                if (((WorkflowStateInfo) (arrWorkflowStates[intItem])).StateID == StateID)
                {
                    intPreviousStateID = StateID;
                    break;
                }
            }

            // get previous active state
            if (intPreviousStateID == StateID)
            {
                intItem = intItem - 1;
                while (intItem >= 0)
                {
                    if (((WorkflowStateInfo) (arrWorkflowStates[intItem])).IsActive)
                    {
                        intPreviousStateID = ((WorkflowStateInfo) (arrWorkflowStates[intItem])).StateID;
                        break;
                    }
                    intItem = intItem - 1;
                }
            }

            // if none found then reset to first state
            if (intPreviousStateID == -1)
            {
                intPreviousStateID = GetFirstWorkflowStateID(WorkflowID);
            }

            return intPreviousStateID;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetNextWorkFlowStateID retrieves the next StateID for the Workflow and State specified
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "WorkflowID">The ID of the Workflow</param>
        /// <param name = "StateID">The ID of the State</param>
        /// -----------------------------------------------------------------------------
        public int GetNextWorkflowStateID(int WorkflowID, int StateID)
        {
            int intNextStateID = -1;
            ArrayList arrWorkflowStates = GetWorkflowStates(WorkflowID);
            int intItem = 0;

            // locate the current state
            for (intItem = 0; intItem < arrWorkflowStates.Count; intItem++)
            {
                if (((WorkflowStateInfo) (arrWorkflowStates[intItem])).StateID == StateID)
                {
                    intNextStateID = StateID;
                    break;
                }
            }

            // get next active state
            if (intNextStateID == StateID)
            {
                intItem = intItem + 1;
                while (intItem < arrWorkflowStates.Count)
                {
                    if (((WorkflowStateInfo) (arrWorkflowStates[intItem])).IsActive)
                    {
                        intNextStateID = ((WorkflowStateInfo) (arrWorkflowStates[intItem])).StateID;
                        break;
                    }
                    intItem = intItem + 1;
                }
            }

            // if none found then reset to first state
            if (intNextStateID == -1)
            {
                intNextStateID = GetFirstWorkflowStateID(WorkflowID);
            }

            return intNextStateID;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetLastWorkFlowStateID retrieves the last StateID for the Workflow
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "WorkflowID">The ID of the Workflow</param>
        /// -----------------------------------------------------------------------------
        public int GetLastWorkflowStateID(int WorkflowID)
        {
            int intStateID = -1;
            ArrayList arrWorkflowStates = GetWorkflowStates(WorkflowID);
            if (arrWorkflowStates.Count > 0)
            {
                intStateID = ((WorkflowStateInfo) (arrWorkflowStates[arrWorkflowStates.Count - 1])).StateID;
            }
            return intStateID;
        }

        #endregion
    }
}