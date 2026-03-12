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
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Content.Workflow.Exceptions;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The default <see cref="IWorkflowStateRepository"/> implementation.</summary>
    /// <param name="hostSettings">The host settings.</param>
    internal class WorkflowStateRepository(IHostSettings hostSettings)
        : ServiceLocator<IWorkflowStateRepository, WorkflowStateRepository>, IWorkflowStateRepository
    {
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="WorkflowStateRepository"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public WorkflowStateRepository()
            : this(null)
        {
        }

        /// <inheritdoc />
        public IEnumerable<WorkflowState> GetWorkflowStates(int workflowId)
        {
            using var context = DataContext.Instance(this.hostSettings);
            var rep = context.GetRepository<WorkflowState>();
            return rep.Find("WHERE WorkflowID = @0 ORDER BY [Order] ASC", workflowId);
        }

        /// <inheritdoc />
        public WorkflowState GetWorkflowStateByID(int stateId)
        {
            return CBO.GetCachedObject<WorkflowState>(
                this.hostSettings,
                new CacheItemArgs(GetWorkflowStateKey(stateId), DataCache.WorkflowsCacheTimeout, DataCache.WorkflowsCachePriority),
                _ =>
                {
                    using var context = DataContext.Instance(this.hostSettings);
                    var rep = context.GetRepository<WorkflowState>();
                    return rep.GetById(stateId);
                });
        }

        /// <inheritdoc />
        public void AddWorkflowState(WorkflowState state)
        {
            Requires.NotNull("state", state);
            Requires.PropertyNotNullOrEmpty("state", "StateName", state.StateName);

            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<WorkflowState>();
                if (DoesExistWorkflowState(state, rep))
                {
                    throw new WorkflowStateNameAlreadyExistsException();
                }

                rep.Insert(state);
            }

            CacheWorkflowState(this.hostSettings, state);
        }

        /// <inheritdoc />
        public void UpdateWorkflowState(WorkflowState state)
        {
            Requires.NotNull("state", state);
            Requires.PropertyNotNegative("state", "StateID", state.StateID);
            Requires.PropertyNotNullOrEmpty("state", "StateName", state.StateName);

            using (var context = DataContext.Instance(this.hostSettings))
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
            CacheWorkflowState(this.hostSettings, state);
        }

        /// <inheritdoc />
        public void DeleteWorkflowState(WorkflowState state)
        {
            Requires.NotNull("state", state);
            Requires.PropertyNotNegative("state", "StateID", state.StateID);

            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<WorkflowState>();
                rep.Delete(state);
            }

            DataCache.RemoveCache(GetWorkflowStateKey(state.StateID));
            DataCache.RemoveCache(WorkflowRepository.GetWorkflowItemKey(state.WorkflowID));
        }

        /// <inheritdoc />
        protected override Func<IWorkflowStateRepository> GetFactory()
        {
            return () => ActivatorUtilities.GetServiceOrCreateInstance<WorkflowStateRepository>(Globals.DependencyProvider);
        }

        private static bool DoesExistWorkflowState(WorkflowState state, IRepository<WorkflowState> rep)
        {
            return rep.Find(
                           "WHERE StateName = @0 AND WorkflowID = @1 AND StateId != @2",
                           state.StateName,
                           state.WorkflowID,
                           state.StateID)
                       .SingleOrDefault() != null;
        }

        private static string GetWorkflowStateKey(int stateId)
        {
            return string.Format(CultureInfo.InvariantCulture, DataCache.ContentWorkflowStateCacheKey, stateId);
        }

        private static void CacheWorkflowState(IHostSettings hostSettings, WorkflowState state)
        {
            if (state.StateID > 0)
            {
                CBO.GetCachedObject<WorkflowState>(
                    hostSettings,
                    new CacheItemArgs(GetWorkflowStateKey(state.StateID), DataCache.WorkflowsCacheTimeout, DataCache.WorkflowsCachePriority),
                    _ => state);
            }
        }
    }
}
