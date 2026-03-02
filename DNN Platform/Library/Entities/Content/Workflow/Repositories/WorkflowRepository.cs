// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow.Exceptions;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    // TODO: add interface metadata documentation
    // TODO: removed unused SPRoc and DataProvider layer
    internal class WorkflowRepository(IHostSettings hostSettings) : ServiceLocator<IWorkflowRepository, WorkflowRepository>, IWorkflowRepository
    {
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly IWorkflowStateRepository stateRepository = WorkflowStateRepository.Instance;

        /// <summary>Initializes a new instance of the <see cref="WorkflowRepository"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public WorkflowRepository()
            : this(null)
        {
        }

        /// <inheritdoc />
        public IEnumerable<Entities.Workflow> GetWorkflows(int portalId)
        {
            using var context = DataContext.Instance(this.hostSettings);
            var rep = context.GetRepository<Entities.Workflow>();
            var workflows = rep.Find("WHERE (PortalId = @0 OR PortalId IS NULL)", portalId).ToArray();

            // Workflow States eager loading
            foreach (var workflow in workflows)
            {
                workflow.States = this.stateRepository.GetWorkflowStates(workflow.WorkflowID);
            }

            return workflows;
        }

        /// <inheritdoc />
        public IEnumerable<Entities.Workflow> GetSystemWorkflows(int portalId)
        {
            using var context = DataContext.Instance(this.hostSettings);
            var rep = context.GetRepository<Entities.Workflow>();
            var workflows = rep.Find("WHERE (PortalId = @0 OR PortalId IS NULL) AND IsSystem = 1", portalId).ToArray();

            // Workflow States eager loading
            foreach (var workflow in workflows)
            {
                workflow.States = this.stateRepository.GetWorkflowStates(workflow.WorkflowID);
            }

            return workflows;
        }

        /// <inheritdoc />
        public Entities.Workflow GetWorkflow(int workflowId)
        {
            return CBO.GetCachedObject<Entities.Workflow>(
                this.hostSettings,
                new CacheItemArgs(GetWorkflowItemKey(workflowId), DataCache.WorkflowsCacheTimeout, DataCache.WorkflowsCachePriority),
                _ =>
                {
                    Entities.Workflow workflow;
                    using (var context = DataContext.Instance(this.hostSettings))
                    {
                        var rep = context.GetRepository<Entities.Workflow>();
                        workflow = rep.Find("WHERE WorkflowID = @0", workflowId).SingleOrDefault();
                    }

                    if (workflow == null)
                    {
                        return null;
                    }

                    workflow.States = this.stateRepository.GetWorkflowStates(workflowId);
                    return workflow;
                });
        }

        /// <inheritdoc />
        public Entities.Workflow GetWorkflow(ContentItem item)
        {
            var state = this.stateRepository.GetWorkflowStateByID(item.StateID);
            return state == null ? null : this.GetWorkflow(state.WorkflowID);
        }

        // TODO: validation

        /// <inheritdoc />
        public void AddWorkflow(Entities.Workflow workflow)
        {
            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<Entities.Workflow>();

                if (DoesExistWorkflow(workflow, rep))
                {
                    throw new WorkflowNameAlreadyExistsException();
                }

                rep.Insert(workflow);
            }

            CacheWorkflow(this.hostSettings, workflow);
        }

        // TODO: validation

        /// <inheritdoc />
        public void UpdateWorkflow(Entities.Workflow workflow)
        {
            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<Entities.Workflow>();

                if (DoesExistWorkflow(workflow, rep))
                {
                    throw new WorkflowNameAlreadyExistsException();
                }

                rep.Update(workflow);
            }

            DataCache.RemoveCache(GetWorkflowItemKey(workflow.WorkflowID));
            CacheWorkflow(this.hostSettings, workflow);
        }

        /// <inheritdoc />
        public void DeleteWorkflow(Entities.Workflow workflow)
        {
            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<Entities.Workflow>();
                rep.Delete(workflow);
            }

            DataCache.RemoveCache(GetWorkflowItemKey(workflow.WorkflowID));
        }

        internal static string GetWorkflowItemKey(int workflowId)
        {
            return string.Format(CultureInfo.InvariantCulture, DataCache.ContentWorkflowCacheKey, workflowId);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowRepository> GetFactory()
        {
            return () => ActivatorUtilities.GetServiceOrCreateInstance<WorkflowRepository>(Globals.DependencyProvider);
        }

        private static bool DoesExistWorkflow(Entities.Workflow workflow, IRepository<Entities.Workflow> rep)
        {
            return rep.Find(
                           "WHERE IsDeleted = 0 AND (PortalId = @0 OR PortalId IS NULL) AND WorkflowName = @1 AND WorkflowID != @2",
                           workflow.PortalID,
                           workflow.WorkflowName,
                           workflow.WorkflowID)
                       .SingleOrDefault() != null;
        }

        private static void CacheWorkflow(IHostSettings hostSettings, Entities.Workflow workflow)
        {
            if (workflow.WorkflowID > 0)
            {
                CBO.GetCachedObject<Entities.Workflow>(
                    hostSettings,
                    new CacheItemArgs(
                        GetWorkflowItemKey(workflow.WorkflowID),
                        DataCache.WorkflowsCacheTimeout,
                        DataCache.WorkflowsCachePriority),
                    _ => workflow);
            }
        }
    }
}
