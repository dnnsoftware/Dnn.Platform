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

using DotNetNuke.Entities.Content.Workflow.Entities;

namespace DotNetNuke.Entities.Content.Workflow
{
    /// <summary>
    /// This class is responsible to manage the system workflows of the portal. 
    /// It provides creation operation methods and methods to get specifically all system workflows
    /// </summary>
    public interface ISystemWorkflowManager
    {
        /// <summary>
        /// Creates predefined system workflows
        /// </summary>
        /// <param name="portalId">Portal Id where system workflows will be created</param>
        void CreateSystemWorkflows(int portalId);

        /// <summary>
        /// Get the 'Direct Publish' system workflow of a specific portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>The 'Direct Publish' workflow</returns>
        Entities.Workflow GetDirectPublishWorkflow(int portalId);
        
        /// <summary>
        /// Get the 'Save Draft' system workflow of a specific portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>The 'Save Draft' workflow</returns>
        Entities.Workflow GetSaveDraftWorkflow(int portalId);

        /// <summary>
        /// Get the 'Content Approval' system workflow of a specific portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>The 'Content Approval' workflow</returns>
        Entities.Workflow GetContentApprovalWorkflow(int portalId);
        
        /// <summary>
        /// Gets a default definition of the 'Draft' system state
        /// </summary>
        /// <param name="order">Order number to be included in the state definition</param>
        /// <returns>A 'Draft' state definition</returns>
        WorkflowState GetDraftStateDefinition(int order);
        
        /// <summary>
        /// Gets a default definition of the 'Published' system state
        /// </summary>
        /// <param name="order">Order number to be included in the state definition</param>
        /// <returns>A 'Published' state definition</returns>
        WorkflowState GetPublishedStateDefinition(int order);

        /// <summary>
        /// Gets a default definition of the 'Ready for review' system state
        /// </summary>
        /// <param name="order">Order number to be included in the state definition</param>
        /// <returns>A 'Ready for review' state definition</returns>
        WorkflowState GetReadyForReviewStateDefinition(int order);
    }
}