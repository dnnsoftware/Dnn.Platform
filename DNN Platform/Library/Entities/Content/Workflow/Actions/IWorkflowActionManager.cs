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

namespace DotNetNuke.Entities.Content.Workflow.Actions
{
    /// <summary>
    /// This class is responsible to manage Workflow Actions
    /// </summary>
    public interface IWorkflowActionManager
    {
        /// <summary>
        /// This method gets an instance of the IWorkflowAction associated to the content type and action type
        /// </summary>
        /// <param name="contentTypeId">Content Item Id</param>
        /// <param name="actionType">Action type</param>
        /// <returns>IWorkflowAction instance</returns>
        IWorkflowAction GetWorkflowActionInstance(int contentTypeId, WorkflowActionTypes actionType);

        /// <summary>
        /// This method registers a new workflow action
        /// </summary>
        /// <remarks>This method checks that the WorkflowAction Source implements the IWorkflowAction interface before register it</remarks>
        /// <param name="workflowAction">Workflow action entity</param>
        /// <exception cref="ArgumentException">Thrown if the ActionSource does not implement the IWorkflowAction interface</exception>
        void RegisterWorkflowAction(WorkflowAction workflowAction);
    }
}
