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
using System.Linq;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Workflow.Actions;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    internal class WorkflowActionRepository : ServiceLocator<IWorkflowActionRepository, WorkflowActionRepository>, IWorkflowActionRepository
    {
        #region Service Locator
        protected override Func<IWorkflowActionRepository> GetFactory()
        {
            return () => new WorkflowActionRepository();
        }
        #endregion

        #region Public Methods
        public WorkflowAction GetWorkflowAction(int contentTypeId, string type)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowAction>();
                return rep.Find("WHERE ContentTypeId = @0 AND ActionType = @1", contentTypeId, type).SingleOrDefault();
            }
        }

        public void AddWorkflowAction(WorkflowAction action)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowAction>();
                rep.Insert(action);
            }
        }
        #endregion
    }
}
