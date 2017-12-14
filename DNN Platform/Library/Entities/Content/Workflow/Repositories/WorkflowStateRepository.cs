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
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Workflow.Entities;
using DotNetNuke.Entities.Content.Workflow.Exceptions;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    internal class WorkflowStateRepository : ServiceLocator<IWorkflowStateRepository, WorkflowStateRepository>, IWorkflowStateRepository
    {
        #region Public Methods
        public IEnumerable<WorkflowState> GetWorkflowStates(int workflowId)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowState>();
                return rep.Find("WHERE WorkflowID = @0 ORDER BY [Order] ASC", workflowId);
            }
        }

        public WorkflowState GetWorkflowStateByID(int stateId)
        {
            return CBO.GetCachedObject<WorkflowState>(new CacheItemArgs(
                GetWorkflowStateKey(stateId), DataCache.WorkflowsCacheTimeout, DataCache.WorkflowsCachePriority),
                _ =>
                {
                    using (var context = DataContext.Instance())
                    {
                        var rep = context.GetRepository<WorkflowState>();
                        return rep.GetById(stateId);
                    }
                });
        }

        // TODO: Validate
        public void AddWorkflowState(WorkflowState state)
        {
            Requires.NotNull("state", state);
            Requires.PropertyNotNullOrEmpty("state", "StateName", state.StateName);

            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowState>();
                if (DoesExistWorkflowState(state, rep))
                {
                    throw new WorkflowStateNameAlreadyExistsException();
                }

                rep.Insert(state);
            }

            CacheWorkflowState(state);
        }

        // TODO: Validate
        public void UpdateWorkflowState(WorkflowState state)
        {
            Requires.NotNull("state", state);
            Requires.PropertyNotNegative("state", "StateID", state.StateID);
            Requires.PropertyNotNullOrEmpty("state", "StateName", state.StateName);

            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowState>();
                if (DoesExistWorkflowState(state, rep))
                {
                    throw new WorkflowStateNameAlreadyExistsException();
                }

                rep.Update(state);
            }

            DataCache.RemoveCache(GetWorkflowStateKey(state.StateID));
            DataCache.RemoveCache(WorkflowRepository.GetWorkflowItemKey(state.WorkflowID));
            CacheWorkflowState(state);
        }

        public void DeleteWorkflowState(WorkflowState state)
        {
            Requires.NotNull("state", state);
            Requires.PropertyNotNegative("state", "StateID", state.StateID);

            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowState>();
                rep.Delete(state);
            }

            DataCache.RemoveCache(GetWorkflowStateKey(state.StateID));
            DataCache.RemoveCache(WorkflowRepository.GetWorkflowItemKey(state.WorkflowID));
        }
        #endregion

        #region Private Methods

        private static bool DoesExistWorkflowState(WorkflowState state, IRepository<WorkflowState> rep)
        {
            return rep.Find(
                "WHERE StateName = @0 AND WorkflowID = @1 AND StateId != @2",
                state.StateName, state.WorkflowID, state.StateID).SingleOrDefault() != null;
        }

        private static string GetWorkflowStateKey(int stateId)
        {
            return string.Format(DataCache.ContentWorkflowStateCacheKey, stateId);
        }

        private void CacheWorkflowState(WorkflowState state)
        {
            if (state.StateID > 0)
            {
                CBO.GetCachedObject<WorkflowState>(new CacheItemArgs(
                GetWorkflowStateKey(state.StateID), DataCache.WorkflowsCacheTimeout, DataCache.WorkflowsCachePriority),
                _ => state);
            }
        }
        #endregion

        #region Service Locator
        protected override Func<IWorkflowStateRepository> GetFactory()
        {
            return () => new WorkflowStateRepository();
        }
        #endregion
    }
}
