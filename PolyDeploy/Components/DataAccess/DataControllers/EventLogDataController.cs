using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using Cantarus.Modules.PolyDeploy.Components.Logging;
using DotNetNuke.Data;
using System;
using System.Collections.Generic;

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

        public IEnumerable<EventLog> Browse(int pageIndex, int pageSize, string eventType = null, EventLogSeverity? severity = null)
        {
            Dictionary<string, object> conditions = new Dictionary<string, dynamic>();

            if (eventType != null)
            {
                conditions.Add("EventType", eventType.ToUpper());
            }

            if (severity != null)
            {
                conditions.Add("Severity", severity);
            }

            using (IDataContext context = DataContext.Instance())
            {
                var repo = context.GetRepository<EventLog>();

                if (conditions.Count > 0)
                {
                    string sqlQuery = "";
                    object [] sqlParams = new object[] { conditions.Values };
                    int paramNo = 0;

                    foreach (KeyValuePair<string, object> kvp in conditions)
                    {
                        if (string.IsNullOrEmpty(sqlQuery))
                        {
                            sqlQuery = sqlQuery + "WHERE " + kvp.Key + " = @" + paramNo;
                        }
                        else
                        {
                            sqlQuery = sqlQuery + " AND " + kvp.Key + " = @0" + paramNo;
                        }

                        paramNo++;
                    }

                    return repo.Find(pageIndex, pageSize, sqlQuery, sqlParams);
                }
                else
                {
                    return repo.GetPage(pageIndex, pageSize);
                }
            }
        }

        public IEnumerable<string> GetEventTypes()
        {
            using (IDataContext context = DataContext.Instance())
            {
                return context.ExecuteQuery<string>(System.Data.CommandType.Text, "SELECT DISTINCT [EventType] FROM [dbo].[Cantarus_PolyDeploy_EventLogs]", null);
            }
        }

        #endregion
    }
}
