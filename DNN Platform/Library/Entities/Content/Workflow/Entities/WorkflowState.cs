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
using System.ComponentModel.DataAnnotations;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    /// <summary>
    /// This entity represents a Workflow State
    /// </summary>
    [PrimaryKey("StateID")]
    [TableName("ContentWorkflowStates")]
    [Serializable]
    public class WorkflowState 
    {
        /// <summary>
        /// State Id
        /// </summary>
        public int StateID { get; set; }

        /// <summary>
        /// Workflow associated to the state
        /// </summary>
        public int WorkflowID { get; set; }

        /// <summary>
        /// State name
        /// </summary>
        [Required]
        [StringLength(40)]
        public string StateName { get; set; }

        /// <summary>
        /// State Order
        /// </summary>
        public int Order { get; internal set; } 

        /// <summary>
        /// Indicates if the state is a system state. System states (i.e.: Draft, Published) have a special behavior. They cannot be deleted or moved.
        /// </summary>
        public bool IsSystem { get; internal set; }

        /// <summary>
        /// If set to true the Workflow Engine will send system notification to the reviewer of the state when the workflow reach it
        /// </summary>
        public bool SendNotification { get; set; }

        /// <summary>
        /// If set to true the Workflow Engine <see cref="IWorkflowEngine"/> will send system notification to administrators user when the workflow reach it
        /// </summary>
        public bool SendNotificationToAdministrators { get; set; }
    }
}
