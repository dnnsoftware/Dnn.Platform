﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow
// ReSharper enable CheckNamespace
{
    /// <summary>
    /// This entity represents a Workflow
    /// </summary>
    [Obsolete("Deprecated in Platform 7.4.0")]
    public class ContentWorkflow
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
        public string WorkflowName { get; set; }
        
        /// <summary>
        /// Workflow Description
        /// </summary>
        public string Description { get; set; }

        public bool IsDeleted { get; set; }

        public bool StartAfterCreating { get; set; }

        public bool StartAfterEditing { get; set; }

        public bool DispositionEnabled { get; set; }

        /// <summary>
        /// Workflow states
        /// </summary>
        public IEnumerable<ContentWorkflowState> States { get; set; }
        
    }
}
