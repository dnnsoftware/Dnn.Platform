// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Content.Workflow.Exceptions;
    using DotNetNuke.Framework;

    internal class WorkflowStateRepository : ServiceLocator<IWorkflowStateRepository, WorkflowStateRepository>, IWorkflowStateRepository
    {
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
            return CBO.GetCachedObject<WorkflowState>(
                new CacheItemArgs(
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

            this.CacheWorkflowState(state);
        }

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
            this.CacheWorkflowState(state);
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

        protected override Func<IWorkflowStateRepository> GetFactory()
        {
            return () => new WorkflowStateRepository();
        }

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
                CBO.GetCachedObject<WorkflowState>(
                    new CacheItemArgs(
                GetWorkflowStateKey(state.StateID), DataCache.WorkflowsCacheTimeout, DataCache.WorkflowsCachePriority),
                    _ => state);
            }
        }
    }
}
