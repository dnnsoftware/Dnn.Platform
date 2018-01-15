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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    /// <summary>
    /// This entity represents a Workflow
    /// </summary>
    [PrimaryKey("WorkflowID")]
    [TableName("ContentWorkflows")]
    [Serializable]
    public class Workflow
    {
        /// <summary>
        /// Workflow Id
        /// </summary>
        public int WorkflowID { get; set; }

        /// <summary>
        /// Portal Id
        /// </summary>
        public int PortalID { get; set; }

        /// <summary>
        /// Workflow Name
        /// </summary>
        [Required]
        [StringLength(40)]
        public string WorkflowName { get; set; }

        /// <summary>
        /// Workflow Key. This property can be used to 
        /// </summary>
        [StringLength(40)]
        public string WorkflowKey { get; set; }

        /// <summary>
        /// Workflow Description
        /// </summary>
        [StringLength(256)]
        public string Description { get; set; }

        /// <summary>
        /// System workflow have a special behavior. It cannot be deleted and new states cannot be added
        /// </summary>
        public bool IsSystem { get; internal set; }

        /// <summary>
        /// Workflow states
        /// </summary>
        [IgnoreColumn]
        public IEnumerable<WorkflowState> States { get; internal set; }

        /// <summary>
        /// First workflow state
        /// </summary>
        [IgnoreColumn]
        public WorkflowState FirstState
        {
            get
            {
                return States == null || !States.Any() ? null : States.OrderBy(s => s.Order).FirstOrDefault();
            }
        }

        /// <summary>
        /// Last workflow state
        /// </summary>
        [IgnoreColumn]
        public WorkflowState LastState
        {
            get
            {
                return States == null || !States.Any() ? null : States.OrderBy(s => s.Order).LastOrDefault();
            }
        }
    }
}
