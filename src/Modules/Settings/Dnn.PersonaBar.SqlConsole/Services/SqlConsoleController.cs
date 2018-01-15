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

namespace Dnn.PersonaBar.SqlConsole.Services
{
    [MenuPermission(Scope = ServiceScope.Host)]
    public class SqlConsoleController : PersonaBarApiController
    {
        private ISqlQueryController _controller = SqlQueryController.Instance;
        const string ScriptDelimiterRegex = "(?<=(?:[^\\w]+|^))GO(?=(?: |\\t)*?(?:\\r?\\n|$))";
        private static readonly Regex SqlObjRegex = new Regex(ScriptDelimiterRegex,
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        //private const int MaxOutputRecords = 500;

        #region API

        [HttpGet]
        public HttpResponseMessage GetSavedQueries()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                queries = _controller.GetQueries(),
                connections = _controller.GetConnections()
            });
        }

        [HttpGet]
        public HttpResponseMessage GetSavedQuery(int id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, _controller.GetQuery(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveQuery(SqlQuery query)
        {
            query.CreatedOnDate = DateTime.Now;
            query.CreatedByUserId = UserInfo.UserID;
            query.LastModifiedOnDate = DateTime.Now;
            query.LastModifiedByUserId = UserInfo.UserID;

            if (query.QueryId <= 0)
            {
                var saveQueries = _controller.GetQueries();
                var saveQuery = saveQueries.FirstOrDefault(q => q.Name.Equals(query.Name, StringComparison.OrdinalIgnoreCase));
                if (saveQuery != null)
                {
                    query.QueryId = saveQuery.QueryId;
                }
            }

            if (query.QueryId > 0)
            {
                _controller.UpdateQuery(query);
            }
            else
            {
                _controller.AddQuery(query);
            }

            return Request.CreateResponse(HttpStatusCode.OK, query);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteQuery(SqlQuery query)
        {
            var savedQuery = _controller.GetQuery(query.QueryId);
            if (savedQuery != null)
            {
                _controller.DeleteQuery(savedQuery);

                return Request.CreateResponse(HttpStatusCode.OK, new {});
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RunQuery(AdhocSqlQuery query)
        {
            var connectionstring = Config.GetConnectionString(query.ConnectionStringName);

            var outputTables = new List<DataTable>();
            string errorMessage;

            var runAsQuery = RunAsScript(query.Query);
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

            RecordAuditEventLog(query.Query);

            var statusCode = string.IsNullOrEmpty(errorMessage) ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
            return Request.CreateResponse(statusCode,  new { Data = runAsQuery ? null : outputTables, Error = errorMessage });
        }

        #endregion

        #region Private Methods

        private void RecordAuditEventLog(string query)
        {
            var props = new LogProperties { new LogDetailInfo("User", UserInfo.Username), new LogDetailInfo("SQL Query", query) };

            //Add the event log with host portal id.
            var log = new LogInfo
            {
                LogUserID = UserInfo.UserID,
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

        #endregion
    }
}
