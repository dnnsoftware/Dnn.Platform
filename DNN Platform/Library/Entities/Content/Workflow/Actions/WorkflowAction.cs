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

using DotNetNuke.ComponentModel.DataAnnotations;

namespace DotNetNuke.Entities.Content.Workflow.Actions
{
    /// <summary>
    /// This entity represents a workflow action implementation
    /// </summary>
    [PrimaryKey("ActionId")]
    [TableName("ContentWorkflowActions")]
    public class WorkflowAction
    {
        /// <summary>
        /// Action Id
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// Content item type Id
        /// </summary>
        public int ContentTypeId { get; set; }

        /// <summary>
        /// Action type. This is a string representation of the enum <see cref="WorkflowActionTypes"/>
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Action Source. This property represents the path to the class that implement the IWorkflowAction interface
        /// i.e.: "MyProject.WorkflowActions.WorkflowDiscardction, MyProject"
        /// </summary>
        public string ActionSource { get; set; }
    }
}
