// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SqlConsole.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.SqlConsole.Components;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Api;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class SqlConsoleController : PersonaBarApiController
    {
        const string ScriptDelimiterRegex = "(?<=(?:[^\\w]+|^))GO(?=(?: |\\t)*?(?:\\r?\\n|$))";

        private static readonly Regex SqlObjRegex = new Regex(ScriptDelimiterRegex,
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private ISqlQueryController _controller = SqlQueryController.Instance;

        //private const int MaxOutputRecords = 500;

        [HttpGet]
        public HttpResponseMessage GetSavedQueries()
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                queries = this._controller.GetQueries(),
                connections = this._controller.GetConnections()
            });
        }

        [HttpGet]
        public HttpResponseMessage GetSavedQuery(int id)
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, this._controller.GetQuery(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveQuery(SqlQuery query)
        {
            query.CreatedOnDate = DateTime.Now;
            query.CreatedByUserId = this.UserInfo.UserID;
            query.LastModifiedOnDate = DateTime.Now;
            query.LastModifiedByUserId = this.UserInfo.UserID;

            if (query.QueryId <= 0)
            {
                var saveQueries = this._controller.GetQueries();
                var saveQuery = saveQueries.FirstOrDefault(q => q.Name.Equals(query.Name, StringComparison.OrdinalIgnoreCase));
                if (saveQuery != null)
                {
                    query.QueryId = saveQuery.QueryId;
                }
            }

            if (query.QueryId > 0)
            {
                this._controller.UpdateQuery(query);
            }
            else
            {
                this._controller.AddQuery(query);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, query);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteQuery(SqlQuery query)
        {
            var savedQuery = this._controller.GetQuery(query.QueryId);
            if (savedQuery != null)
            {
                this._controller.DeleteQuery(savedQuery);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { });
            }

            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RunQuery(AdhocSqlQuery query)
        {
            var connectionstring = Config.GetConnectionString(query.ConnectionStringName);

            var outputTables = new List<DataTable>();
            string errorMessage;

            var runAsQuery = this.RunAsScript(query.Query);
            if (runAsQuery)
            {
                errorMessage = DataProvider.Instance().ExecuteScript(connectionstring, query.Query, query.Timeout);
            }
            else
            {
                try
                {
                    var dr = DataProvider.Instance().ExecuteSQLTemp(connectionstring, query.Query, query.Timeout, out errorMessage);
                    if (dr != null)
                    {
                        do
                        {
                            var table = new DataTable { Locale = CultureInfo.CurrentCulture };
                            table.Load(dr);
                            outputTables.Add(table);
                        }
                        while (!dr.IsClosed);
                    }
                }
                catch (SqlException sqlException)
                {
                    errorMessage = sqlException.Message;
                }
            }

            this.RecordAuditEventLog(query.Query);

            var statusCode = string.IsNullOrEmpty(errorMessage) ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
            return this.Request.CreateResponse(statusCode, new { Data = runAsQuery ? null : outputTables, Error = errorMessage });
        }

        private void RecordAuditEventLog(string query)
        {
            var props = new LogProperties { new LogDetailInfo("User", this.UserInfo.Username), new LogDetailInfo("SQL Query", query) };

            //Add the event log with host portal id.
            var log = new LogInfo
            {
                LogUserID = this.UserInfo.UserID,
                LogTypeKey = EventLogController.EventLogType.HOST_SQL_EXECUTED.ToString(),
                LogProperties = props,
                BypassBuffering = true,
                LogPortalID = Null.NullInteger
            };

            LogController.Instance.AddLog(log);
        }

        private bool RunAsScript(string query)
        {
            return SqlObjRegex.IsMatch(query);
        }
    }
}
