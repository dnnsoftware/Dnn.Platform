#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

namespace DotNetNuke.Entities.Tabs
{
    /// <summary>
    /// This interface is responsible to provide the tab workflow settings
    /// </summary>
    public interface ITabWorkflowSettings
    {
        /// <summary>
        /// This method returns the default tab workflow of the portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <remarks>If no default workflow is defined for a portal the method returns the Direct Publish system workflow</remarks>
        /// <returns>The workflow Id of the portal default workflow</returns>
        int GetDefaultTabWorkflowId(int portalId);
        
        /// <summary>
        /// This method sets the default workflow for a portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="workflowId">Workflow Id</param>
        void SetDefaultTabWorkflowId(int portalId, int workflowId);

        /// <summary>
        /// This method enables or disables the tab workflow for the entire portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="enabled">true for enable it, false for disable it</param>
        void SetWorkflowEnabled(int portalId, bool enabled);

        /// <summary>
        /// This method enables or disables the tab workflow for a specific tab
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="tabId">Tab Id</param>
        /// <param name="enabled">true for enable it, false for disable it</param>
        /// <remarks>this won't enable workflow of a tab if the tab workflow is disabled at portal level</remarks>
        void SetWorkflowEnabled(int portalId, int tabId, bool enabled);

        /// <summary>
        /// The method returns true if the workflow is enabled for a tab
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="tabId">Tab Id</param>
        /// <returns>True if the workflow is enabled, false otherwise</returns>
        bool IsWorkflowEnabled(int portalId, int tabId);

        /// <summary>
        /// The method returns true is the workflow is enabled at portal level
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>True if the workflow is enabled, false otherwise</returns>
        bool IsWorkflowEnabled(int portalId);
    }
}
