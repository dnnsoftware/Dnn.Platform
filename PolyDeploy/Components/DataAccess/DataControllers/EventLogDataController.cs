using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using Cantarus.Modules.PolyDeploy.Components.Logging;
using DotNetNuke.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers
{
    internal class EventLogDataController
    {
        #region Create

        public void Create(EventLog eventLog)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<EventLog>();

                eventLog.Date = DateTime.Now;

                repo.Insert(eventLog);
            }
        }

        #endregion

        #region Read

        public IEnumerable<EventLog> Get()
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<EventLog>();

                return repo.Get();
            }
        }

        public IEnumerable<EventLog> Browse(int pageIndex, int pageSize, string eventType, EventLogSeverity? severity)
        {
            using (IDataContext context = DataContext.Instance())
            {
                return context.ExecuteQuery<EventLog>(
                    System.Data.CommandType.StoredProcedure,
                    "{databaseOwner}[{objectQualifier}Cantarus_PolyDeploy_EventLogsPaginated]",
                    pageIndex,
                    pageSize,
                    eventType,
                    severity
                );
            }
        }

        public int BrowseCount(int pageIndex, int pageSize, string eventType, EventLogSeverity? severity)
        {
            using (IDataContext context = DataContext.Instance())
            {
                return context.ExecuteQuery<int>(
                    System.Data.CommandType.StoredProcedure,
                    "{databaseOwner}[{objectQualifier}Cantarus_PolyDeploy_EventLogsPaginatedRows]",
                    pageIndex,
                    pageSize,
                    eventType,
                    severity
                ).FirstOrDefault();
            }
        }

        public IEnumerable<string> GetEventTypes()
        {
            using (IDataContext context = DataContext.Instance())
            {
                return context.ExecuteQuery<string>(System.Data.CommandType.Text, "SELECT DISTINCT [EventType] FROM [dbo].[Cantarus_PolyDeploy_EventLogs]", null);
            }
        }

        public int EventCount()
        {
            using (IDataContext context = DataContext.Instance())
            {
                return context.ExecuteQuery<int>(System.Data.CommandType.Text, "SELECT COUNT(*) FROM [dbo].[Cantarus_PolyDeploy_EventLogs]", null).FirstOrDefault();
            }
        }

        #endregion
    }
}
