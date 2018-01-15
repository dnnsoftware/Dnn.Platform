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
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Workflow.Exceptions;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    // TODO: add interface metadata documentation
    // TODO: removed unused SPRoc and DataProvider layer
    internal class WorkflowRepository : ServiceLocator<IWorkflowRepository, WorkflowRepository>, IWorkflowRepository
    {
        #region Members
        private readonly IWorkflowStateRepository _stateRepository;
        #endregion

        #region Constructor
        public WorkflowRepository()
        {
            _stateRepository = WorkflowStateRepository.Instance;
        }
        #endregion

        #region Public Methods
        public IEnumerable<Entities.Workflow> GetWorkflows(int portalId)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<Entities.Workflow>();
                var workflows = rep.Find("WHERE (PortalId = @0 OR PortalId IS NULL)", portalId).ToArray();

                // Worfklow States eager loading
                foreach (var workflow in workflows)
                {
                    workflow.States = _stateRepository.GetWorkflowStates(workflow.WorkflowID);
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
                    workflow.States = _stateRepository.GetWorkflowStates(workflow.WorkflowID);
                }

                return workflows;
            }
        }

        public Entities.Workflow GetWorkflow(int workflowId)
        {
            return CBO.GetCachedObject<Entities.Workflow>(new CacheItemArgs(
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

                    workflow.States = _stateRepository.GetWorkflowStates(workflowId);
                    return workflow;
                });
        }

        public Entities.Workflow GetWorkflow(ContentItem item)
        {
            var state = _stateRepository.GetWorkflowStateByID(item.StateID);
            return state == null ? null : GetWorkflow(state.WorkflowID);
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
        #endregion

        #region Private Methods
        private static bool DoesExistWorkflow(Entities.Workflow workflow, IRepository<Entities.Workflow> rep)
        {
            return rep.Find(
                "WHERE IsDeleted = 0 AND (PortalId = @0 OR PortalId IS NULL) AND WorkflowName = @1 AND WorkflowID != @2",
                workflow.PortalID, workflow.WorkflowName, workflow.WorkflowID).SingleOrDefault() != null;
        }

        internal static string GetWorkflowItemKey(int workflowId)
        {
            return string.Format(DataCache.ContentWorkflowCacheKey, workflowId);
        }

        private static void CacheWorkflow(Entities.Workflow workflow)
        {
            if (workflow.WorkflowID > 0)
            {
                CBO.GetCachedObject<Entities.Workflow>(new CacheItemArgs(GetWorkflowItemKey(workflow.WorkflowID),
                    DataCache.WorkflowsCacheTimeout, DataCache.WorkflowsCachePriority), _ => workflow);
            }
        }

        #endregion

        #region Service Locator
        protected override Func<IWorkflowRepository> GetFactory()
        {
            return () => new WorkflowRepository();
        }
        #endregion
    }
}
