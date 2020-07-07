// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow.Exceptions;
    using DotNetNuke.Framework;

    // TODO: add interface metadata documentation
    // TODO: removed unused SPRoc and DataProvider layer
    internal class WorkflowRepository : ServiceLocator<IWorkflowRepository, WorkflowRepository>, IWorkflowRepository
    {
        private readonly IWorkflowStateRepository _stateRepository;

        public WorkflowRepository()
        {
            this._stateRepository = WorkflowStateRepository.Instance;
        }

        public IEnumerable<Entities.Workflow> GetWorkflows(int portalId)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Entities.Workflow>();
                var workflows = rep.Find("WHERE (PortalId = @0 OR PortalId IS NULL)", portalId).ToArray();

                // Worfklow States eager loading
                foreach (var workflow in workflows)
                {
                    workflow.States = this._stateRepository.GetWorkflowStates(workflow.WorkflowID);
                }

                return workflows;
            }
        }

        public IEnumerable<Entities.Workflow> GetSystemWorkflows(int portalId)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Entities.Workflow>();
                var workflows = rep.Find("WHERE (PortalId = @0 OR PortalId IS NULL) AND IsSystem = 1", portalId).ToArray();

                // Worfklow States eager loading
                foreach (var workflow in workflows)
                {
                    workflow.States = this._stateRepository.GetWorkflowStates(workflow.WorkflowID);
                }

                return workflows;
            }
        }

        public Entities.Workflow GetWorkflow(int workflowId)
        {
            return CBO.GetCachedObject<Entities.Workflow>(
                new CacheItemArgs(
                GetWorkflowItemKey(workflowId), DataCache.WorkflowsCacheTimeout, DataCache.WorkflowsCachePriority),
                _ =>
                {
                    Entities.Workflow workflow;
                    using (var context = DataContext.Instance())
                    {
                        var rep = context.GetRepository<Entities.Workflow>();
                        workflow = rep.Find("WHERE WorkflowID = @0", workflowId).SingleOrDefault();
                    }

                    if (workflow == null)
                    {
                        return null;
                    }

                    workflow.States = this._stateRepository.GetWorkflowStates(workflowId);
                    return workflow;
                });
        }

        public Entities.Workflow GetWorkflow(ContentItem item)
        {
            var state = this._stateRepository.GetWorkflowStateByID(item.StateID);
            return state == null ? null : this.GetWorkflow(state.WorkflowID);
        }

        // TODO: validation
        public void AddWorkflow(Entities.Workflow workflow)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Entities.Workflow>();

                if (DoesExistWorkflow(workflow, rep))
                {
                    throw new WorkflowNameAlreadyExistsException();
                }

                rep.Insert(workflow);
            }

            CacheWorkflow(workflow);
        }

        // TODO: validation
        public void UpdateWorkflow(Entities.Workflow workflow)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Entities.Workflow>();

                if (DoesExistWorkflow(workflow, rep))
                {
                    throw new WorkflowNameAlreadyExistsException();
                }

                rep.Update(workflow);
            }

            DataCache.RemoveCache(GetWorkflowItemKey(workflow.WorkflowID));
            CacheWorkflow(workflow);
        }

        public void DeleteWorkflow(Entities.Workflow workflow)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Entities.Workflow>();
                rep.Delete(workflow);
            }

            DataCache.RemoveCache(GetWorkflowItemKey(workflow.WorkflowID));
        }

        internal static string GetWorkflowItemKey(int workflowId)
        {
            return string.Format(DataCache.ContentWorkflowCacheKey, workflowId);
        }

        protected override Func<IWorkflowRepository> GetFactory()
        {
            return () => new WorkflowRepository();
        }

        private static bool DoesExistWorkflow(Entities.Workflow workflow, IRepository<Entities.Workflow> rep)
        {
            return rep.Find(
                "WHERE IsDeleted = 0 AND (PortalId = @0 OR PortalId IS NULL) AND WorkflowName = @1 AND WorkflowID != @2",
                workflow.PortalID, workflow.WorkflowName, workflow.WorkflowID).SingleOrDefault() != null;
        }

        private static void CacheWorkflow(Entities.Workflow workflow)
        {
            if (workflow.WorkflowID > 0)
            {
                CBO.GetCachedObject<Entities.Workflow>(
                    new CacheItemArgs(
                    GetWorkflowItemKey(workflow.WorkflowID),
                    DataCache.WorkflowsCacheTimeout, DataCache.WorkflowsCachePriority), _ => workflow);
            }
        }
    }
}
