﻿#region Copyright
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

using System.Collections.Generic;
using DotNetNuke.Entities.Content.Workflow.Entities;

namespace DotNetNuke.Entities.Content.Workflow
{
    /// <summary>
    /// This class is responsible to manage the workflows logs. 
    /// It provides addition and get operation methods
    /// </summary>
    public interface IWorkflowLogger
    {
        /// <summary>
        /// Adds a log comment regarding a specific workflow
        /// </summary>
        /// <param name="contentItemId">Content item Id related with the log</param>
        /// <param name="workflowId">Workflow Id owner of the log</param>
        /// <param name="type">Log Type</param>
        /// <param name="comment">Comment to be added</param>
        /// <param name="userId">User Id who adds the log</param>
        void AddWorkflowLog(int contentItemId, int workflowId, WorkflowLogType type, string comment, int userId);

        /// <summary>
        /// Adds a log comment regarding a specific workflow
        /// </summary>
        /// <param name="contentItemId">Content item Id related with the log</param>
        /// <param name="workflowId">Workflow Id owner of the log</param>
        /// <param name="action">Custom action related with the log</param>
        /// <param name="comment">Comment to be added</param>
        /// <param name="userId">User Id who adds the log</param>
        void AddWorkflowLog(int contentItemId, int workflowId, string action, string comment, int userId);
        
        /// <summary>
        /// Gets all logs regarding a specific workflow
        /// </summary>
        /// <param name="contentItemId">Content item Id related with the logs</param>
        /// <param name="workflowId">Workflow Id owner of logs</param>
        /// <returns></returns>
        IEnumerable<WorkflowLog> GetWorkflowLogs(int contentItemId, int workflowId);
    }
}