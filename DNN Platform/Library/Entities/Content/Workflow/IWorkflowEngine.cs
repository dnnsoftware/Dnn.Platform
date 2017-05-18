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

using System;
using DotNetNuke.Entities.Content.Workflow.Dto;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Entities.Content.Workflow
{
    /// <summary>
    /// This class represents the Workflow Engine. 
    /// It allows start, complete/discard and move forward and backward the workflow associated to a ContentItem
    /// </summary>
    public interface IWorkflowEngine
    {
        /// <summary>
        /// This method starts a workflow for a Content Item
        /// </summary>
        /// <param name="workflowId">Workflow Id</param>
        /// <param name="contentItemId">Content item Id</param>
        /// <param name="userId">User Id of the user that start the workflow</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when start a workflow on a Content Item that already has a started workflow.</exception>        
        /// <exception cref="ArgumentOutOfRangeException">When workflowId param is negative</exception>
        void StartWorkflow(int workflowId, int contentItemId, int userId);

        /// <summary>
        /// This method completes a state moving the workflow forward to the next state.
        /// If the next state is not the last one it send notifications to the reviewers of the next state, 
        /// otherwise send the notification to the user that submit the draft in case the workflow complete.
        /// </summary>
        /// <param name="stateTransaction">State transaction Dto</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowConcurrencyException">Thrown when the current state of the workflow is not the same of the current state specified in the StateTransaction Dto</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowSecurityException">Thrown when the user does not have review permission on the current state</exception>
        void CompleteState(StateTransaction stateTransaction);

        /// <summary>
        /// This method discard a state moving the workflow backward to the previous state.
        /// If the previous state is not the first one it send notifications to the reviewers of the previous state, 
        /// otherwise send the notification to the user that submit the draft in case the workflow is in the draft state.
        /// </summary>
        /// <param name="stateTransaction">State transaction Dto</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowConcurrencyException">Thrown when the current state of the workflow is not the same of the current state specified in the StateTransaction Dto</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowSecurityException">Thrown when the user does not have review permission on the current state</exception>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowInvalidOperationException">Thrown when trying to discard a workflow in the last state</exception>
        void DiscardState(StateTransaction stateTransaction);

        /// <summary>
        /// This method returns true if the workflow associated to the Content Item is completed (it is in the last state)
        /// </summary>
        /// <remarks>Content Item without workflow is considered as completed as well</remarks>
        /// <param name="contentItemId">Content item Id</param>
        /// <returns>True if the workflow is completed, false otherwise</returns>
        bool IsWorkflowCompleted(int contentItemId);

        /// <summary>
        /// This method returns true if the workflow associated to the Content Item is completed (it is in the last state)
        /// </summary>
        /// <remarks>Content Item without workflow is considered as completed as well</remarks>
        /// <param name="contentItem">Content item entity</param>
        /// <returns>True if the workflow is completed, false otherwise</returns>
        bool IsWorkflowCompleted(ContentItem contentItem);

        /// <summary>
        /// This method returns true if the workflow associated to the Content Item is in draft (it is in the first state)
        /// </summary>
        /// <remarks>Content Item without workflow is considered as not in draft</remarks>
        /// <param name="contentItemId">Content item Id</param>
        /// <returns>True if the workflow is in draft, false otherwise</returns>
        bool IsWorkflowOnDraft(int contentItemId);

        /// <summary>
        /// This method returns true if the workflow associated to the Content Item is in draft (it is in the first state)
        /// </summary>
        /// <remarks>Content Item without workflow is considered as not in draft</remarks>
        /// <param name="contentItem">Content item entity</param>
        /// <returns>True if the workflow is in draft, false otherwise</returns>
        bool IsWorkflowOnDraft(ContentItem contentItem);

        /// <summary>
        /// This method discards the workflow no matter what is the current state.
        /// It also sends a system notification to the user that submit the workflow to let him know about the discard workflow action.
        /// </summary>
        /// <remarks>This method does not check review permission on the current state</remarks>
        /// <param name="stateTransaction">State transaction Dto</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowConcurrencyException">Thrown when the current state of the workflow is not the same of the current state specified in the StateTransaction Dto</exception>
        void DiscardWorkflow(StateTransaction stateTransaction);

        /// <summary>
        /// This method completes the workflow no matter what is the current state.
        /// It also sends a system notification to the user that submit the workflow to let him know about the complete workflow action.
        /// </summary>
        /// <remarks>This method does not check review permission on the current state</remarks>
        /// <param name="stateTransaction">State transaction Dto</param>
        /// <exception cref="DotNetNuke.Entities.Content.Workflow.Exceptions.WorkflowConcurrencyException">Thrown when the current state of the workflow is not the same of the current state specified in the StateTransaction Dto</exception>
        void CompleteWorkflow(StateTransaction stateTransaction);


        /// <summary>
        /// This method returns the user that started the workflow for the contentItem.
        /// </summary>
        /// <remarks>If Content Item has no workflow, returns null.</remarks>
        /// <param name="contentItem">ContentItem</param>
        /// <returns>User Info</returns>
        UserInfo GetStartedDraftStateUser(ContentItem contentItem);

        /// <summary>
        /// This method returns the user that submitted the contentItem.
        /// </summary>
        /// <remarks>If Content Item has no workflow or the content has not submitted yet, returns null.</remarks>
        /// <param name="contentItem">ContentItem</param>
        /// <returns>User Info</returns>
        UserInfo GetSubmittedDraftStateUser(ContentItem contentItem);

    }
}