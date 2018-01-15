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

using DotNetNuke.Entities.Content.Workflow.Dto;
using DotNetNuke.Entities.Content.Workflow.Entities;

namespace DotNetNuke.Entities.Content.Workflow.Actions
{
    /// <summary>
    /// This interface represents a point of extension that third party can implement to inject behavior on workflow action inside the Workflow Engine.
    /// i.e.: Discard State, Complete State, Discard Workflow, Complete Workflow
    /// Third party can implement the interface for each one of the 4 actions and then register it using the <see cref="IWorkflowActionManager"/>
    /// </summary>
    public interface IWorkflowAction
    {
        /// <summary>
        /// The method gets the action message that will be sent to notify the users depending on the type of action
        /// i.e.: with action type Complete Workflow the action message represents the notification sent to the user that started the workflow
        /// </summary>
        /// <param name="stateTransaction">State transaction dto</param>
        /// <param name="currentState">Workflow state that represent the current state after the workflow action</param>
        /// <returns>Action message that the engine will use to send the notification</returns>
        ActionMessage GetActionMessage(StateTransaction stateTransaction, WorkflowState currentState);

        /// <summary>
        /// This method implements some action on state changed.
        /// i.e.: on Complete Workflow user can implement clear a cache or log some info
        /// </summary>
        /// <param name="stateTransaction">State transaction dto</param>
        void DoActionOnStateChanged(StateTransaction stateTransaction);

        /// <summary>
        /// This method implements some action on state changed.
        /// i.e.: on Complete Workflow user can implement the publish of a pending content version.
        /// </summary>
        /// <param name="stateTransaction">State transaction dto</param>
        void DoActionOnStateChanging(StateTransaction stateTransaction);
    }
}
