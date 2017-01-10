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

#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Log.EventLog;

using Microsoft.ApplicationBlocks.Data;

#endregion

namespace DotNetNuke.Data
{
    public sealed class SqlDataProvider : DataProvider
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (SqlDataProvider));
        #region Private Members

        private const string ScriptDelimiter = "(?<=(?:[^\\w]+|^))GO(?=(?: |\\t)*?(?:\\r?\\n|$))";

		private static readonly Regex ScriptWithRegex = new Regex("WITH\\s*\\([\\s\\S]*?((PAD_INDEX|ALLOW_ROW_LOCKS|ALLOW_PAGE_LOCKS)\\s*=\\s*(ON|OFF))+[\\s\\S]*?\\)", 
															RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
		private static readonly Regex ScriptOnPrimaryRegex = new Regex("(TEXTIMAGE_)*ON\\s*\\[\\s*PRIMARY\\s*\\]", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        #endregion

        #region Public Properties

        public override bool IsConnectionValid
        {
            get
            {
                return CanConnect(ConnectionString, DatabaseOwner, ObjectQualifier);
            }
        }

        public override Dictionary<string, string> Settings
        {
            get
            {
                return ComponentFactory.GetComponentSettings<SqlDataProvider>() as Dictionary<string, string>;
            }
        }

        #endregion

        #region Private Methods

        private static bool CanConnect(string connectionString, string owner, string qualifier)
        {
            bool connectionValid = true;

            try
            {
                PetaPocoHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, owner + qualifier + "GetDatabaseVersion");
            }
            catch (SqlException ex)
            {
                if (ex.Errors.Cast<SqlError>().Any(c => !(c.Number == 2812 && c.Class == 16)))
                {
                    connectionValid = false;
                }
            }

            return connectionValid;
        }

        private string ExecuteScriptInternal(string connectionString, string script, int timeout = 0)
        {
            string exceptions = "";

            var sqlDelimiterRegex = RegexUtils.GetCachedRegex(ScriptDelimiter, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] sqlStatements = sqlDelimiterRegex.Split(script);
            foreach (string statement in sqlStatements)
            {
                var sql = statement.Trim();
                if (!String.IsNullOrEmpty(sql))
                {

                    // script dynamic substitution
                    sql = DataUtil.ReplaceTokens(sql);

                    //Clean up some SQL Azure incompatabilities
	                var query = GetAzureCompactScript(sql);

                    if (query != sql)
                    {
                        var props = new LogProperties { new LogDetailInfo("SQL Script Modified", query) };

                        EventLogController.Instance.AddLog(props,
                                    PortalController.Instance.GetCurrentPortalSettings(),
                                    UserController.Instance.GetCurrentUserInfo().UserID,
                                    EventLogController.EventLogType.HOST_ALERT.ToString(),
                                    true);
                    }

                    try
                    {
                        Logger.Trace("Executing SQL Script " + query);

                        //Create a new connection
                        using (var connection = new SqlConnection(connectionString))
                        {
                            //Create a new command
                            using (var command = new SqlCommand(query, connection) {CommandTimeout = timeout})
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                                connection.Close();
                            }
                        }
                    }
                    catch (SqlException objException)
                    {
                        Logger.Error(objException);
                        exceptions += objException + Environment.NewLine + Environment.NewLine + query + Environment.NewLine + Environment.NewLine;
                    }
                }
            }

            return exceptions;
        }

        private IDataReader ExecuteSQLInternal(string connectionString, string sql, int timeout = 0)
        {
            string errorMessage;
            return ExecuteSQLInternal(connectionString, sql, timeout, out errorMessage);
        }

        private IDataReader ExecuteSQLInternal(string connectionString, string sql, int timeout, out string errorMessage)
        {
            try
            {
                sql = DataUtil.ReplaceTokens(sql);
                errorMessage = "";

                if (string.IsNullOrEmpty(connectionString))
                    throw new ArgumentNullException(nameof(connectionString));

                if (timeout > 0)
                {
                    var builder = GetConnectionStringBuilder();
                    builder.ConnectionString = connectionString;
                    builder["Connect Timeout"] = null;
                    builder["Connection Timeout"] = timeout;
                    connectionString = builder.ConnectionString;
                }

                SqlConnection connection = null;
                try
                {
                    connection = new SqlConnection(connectionString);
                    connection.Open();
                    return SqlHelper.ExecuteReader(connection, CommandType.Text, sql);
                }
                catch (Exception)
                {
                    connection?.Dispose();
                    throw;
                }
            }
            catch (SqlException sqlException)
            {
                //error in SQL query
                Logger.Error(sqlException);
                errorMessage = sqlException.Message;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                errorMessage = ex.ToString();
            }
            errorMessage += Environment.NewLine + Environment.NewLine + sql + Environment.NewLine + Environment.NewLine;
            return null;
        }

        private string GetConnectionStringUserID()
        {
            string DBUser = "public";

            //If connection string does not use integrated security, then get user id.
            if (ConnectionString.ToUpper().Contains("USER ID") || ConnectionString.ToUpper().Contains("UID") || ConnectionString.ToUpper().Contains("USER"))
            {
                string[] ConnSettings = ConnectionString.Split(';');

                foreach (string s in ConnSettings)
                {
                    if (s != string.Empty)
                    {
                        string[] ConnSetting = s.Split('=');
                        if ("USER ID|UID|USER".Contains(ConnSetting[0].Trim().ToUpper()))
                        {
                            DBUser = ConnSetting[1].Trim();
                        }
                    }
                }
            }
            return DBUser;
        }

        private string GrantStoredProceduresPermission(string Permission, string LoginOrRole)
        {
            string SQL = string.Empty;
            string Exceptions = string.Empty;

            try
            {
                //grant rights to a login or role for all stored procedures
                SQL += "if exists (select * from dbo.sysusers where name='" + LoginOrRole + "')";
                SQL += "  begin";
                SQL += "    declare @exec nvarchar(2000) ";
                SQL += "    declare @name varchar(150) ";
                SQL += "    declare sp_cursor cursor for select o.name as name ";
                SQL += "    from dbo.sysobjects o ";
                SQL += "    where ( OBJECTPROPERTY(o.id, N'IsProcedure') = 1 or OBJECTPROPERTY(o.id, N'IsExtendedProc') = 1 or OBJECTPROPERTY(o.id, N'IsReplProc') = 1 ) ";
                SQL += "    and OBJECTPROPERTY(o.id, N'IsMSShipped') = 0 ";
                SQL += "    and o.name not like N'#%%' ";
                SQL += "    and (left(o.name,len('" + ObjectQualifier + "')) = '" + ObjectQualifier + "' or left(o.name,7) = 'aspnet_') ";
                SQL += "    open sp_cursor ";
                SQL += "    fetch sp_cursor into @name ";
                SQL += "    while @@fetch_status >= 0 ";
                SQL += "      begin";
                SQL += "        select @exec = 'grant " + Permission + " on [' +  @name  + '] to [" + LoginOrRole + "]'";
                SQL += "        execute (@exec)";
                SQL += "        fetch sp_cursor into @name ";
                SQL += "      end ";
                SQL += "    deallocate sp_cursor";
                SQL += "  end ";

                SqlHelper.ExecuteNonQuery(UpgradeConnectionString, CommandType.Text, SQL);
            }
            catch (SqlException objException)
            {
                Logger.Error(objException);

                Exceptions += objException + Environment.NewLine + Environment.NewLine + SQL + Environment.NewLine + Environment.NewLine;
            }
            return Exceptions;
        }

        private string GrantUserDefinedFunctionsPermission(string ScalarPermission, string TablePermission, string LoginOrRole)
        {
            string SQL = string.Empty;
            string Exceptions = string.Empty;
            try
            {
                //grant EXECUTE rights to a login or role for all functions
                SQL += "if exists (select * from dbo.sysusers where name='" + LoginOrRole + "')";
                SQL += "  begin";
                SQL += "    declare @exec nvarchar(2000) ";
                SQL += "    declare @name varchar(150) ";
                SQL += "    declare @isscalarfunction int ";
                SQL += "    declare @istablefunction int ";
                SQL += "    declare sp_cursor cursor for select o.name as name, OBJECTPROPERTY(o.id, N'IsScalarFunction') as IsScalarFunction ";
                SQL += "    from dbo.sysobjects o ";
                SQL += "    where ( OBJECTPROPERTY(o.id, N'IsScalarFunction') = 1 OR OBJECTPROPERTY(o.id, N'IsTableFunction') = 1 ) ";
                SQL += "      and OBJECTPROPERTY(o.id, N'IsMSShipped') = 0 ";
                SQL += "      and o.name not like N'#%%' ";
                SQL += "      and (left(o.name,len('" + ObjectQualifier + "')) = '" + ObjectQualifier + "' or left(o.name,7) = 'aspnet_') ";
                SQL += "    open sp_cursor ";
                SQL += "    fetch sp_cursor into @name, @isscalarfunction ";
                SQL += "    while @@fetch_status >= 0 ";
                SQL += "      begin ";
                SQL += "        if @IsScalarFunction = 1 ";
                SQL += "          begin";
                SQL += "            select @exec = 'grant " + ScalarPermission + " on [' +  @name  + '] to [" + LoginOrRole + "]'";
                SQL += "            execute (@exec)";
                SQL += "            fetch sp_cursor into @name, @isscalarfunction  ";
                SQL += "          end ";
                SQL += "        else ";
                SQL += "          begin";
                SQL += "            select @exec = 'grant " + TablePermission + " on [' +  @name  + '] to [" + LoginOrRole + "]'";
                SQL += "            execute (@exec)";
                SQL += "            fetch sp_cursor into @name, @isscalarfunction  ";
                SQL += "          end ";
                SQL += "      end ";
                SQL += "    deallocate sp_cursor";
                SQL += "  end ";

                SqlHelper.ExecuteNonQuery(UpgradeConnectionString, CommandType.Text, SQL);
            }
            catch (SqlException objException)
            {
                Logger.Error(objException);

                Exceptions += objException + Environment.NewLine + Environment.NewLine + SQL + Environment.NewLine + Environment.NewLine;
            }
            return Exceptions;
        }

		private string GetAzureCompactScript(string script)
		{
			if (ScriptWithRegex.IsMatch(script))
			{
				script = ScriptWithRegex.Replace(script, string.Empty);
			}

			if (ScriptOnPrimaryRegex.IsMatch(script))
			{
				script = ScriptOnPrimaryRegex.Replace(script, string.Empty);
			}

			return script;
		}

        #endregion

        #region Abstract Methods

        public override void ExecuteNonQuery(string procedureName, params object[] commandParameters)
        {
            PetaPocoHelper.ExecuteNonQuery(ConnectionString, CommandType.StoredProcedure, DatabaseOwner + ObjectQualifier + procedureName, commandParameters);
        }

        public override void ExecuteNonQuery(int timeout, string procedureName, params object[] commandParameters)
        {
            PetaPocoHelper.ExecuteNonQuery(ConnectionString, CommandType.StoredProcedure, timeout, DatabaseOwner + ObjectQualifier + procedureName, commandParameters);
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable)
        {
            PetaPocoHelper.BulkInsert(ConnectionString, DatabaseOwner + ObjectQualifier + procedureName, tableParameterName, dataTable);
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeout)
        {
            PetaPocoHelper.BulkInsert(ConnectionString, timeout, DatabaseOwner + ObjectQualifier + procedureName, tableParameterName, dataTable);
        }

        public override IDataReader ExecuteReader(string procedureName, params object[] commandParameters)
        {
            return PetaPocoHelper.ExecuteReader(ConnectionString, CommandType.StoredProcedure, DatabaseOwner + ObjectQualifier + procedureName, commandParameters);
        }

        public override IDataReader ExecuteReader(int timeout, string procedureName, params object[] commandParameters)
        {
            return PetaPocoHelper.ExecuteReader(ConnectionString, CommandType.StoredProcedure, timeout, DatabaseOwner + ObjectQualifier + procedureName, commandParameters);
        }

        public override T ExecuteScalar<T>(string procedureName, params object[] commandParameters)
        {
            return PetaPocoHelper.ExecuteScalar<T>(ConnectionString, CommandType.StoredProcedure, DatabaseOwner + ObjectQualifier + procedureName, commandParameters); 
        }

        public override T ExecuteScalar<T>(int timeout, string procedureName, params object[] commandParameters)
        {
            return PetaPocoHelper.ExecuteScalar<T>(ConnectionString, CommandType.StoredProcedure, timeout, DatabaseOwner + ObjectQualifier + procedureName, commandParameters); 
        }

        public override string ExecuteScript(string script)
        {
            return ExecuteScript(script, 0);
        }

        public override string ExecuteScript(string script, int timeout)
        {
            string exceptions = ExecuteScriptInternal(UpgradeConnectionString, script, timeout);

            //if the upgrade connection string is specified or or db_owner setting is not set to dbo
            if (UpgradeConnectionString != ConnectionString || DatabaseOwner.Trim().ToLower() != "dbo.")
            {
                try
                {
                    //grant execute rights to the public role or userid for all stored procedures. This is
                    //necesary because the UpgradeConnectionString will create stored procedures
                    //which restrict execute permissions for the ConnectionString user account. This is also
                    //necessary when db_owner is not set to "dbo" 
                    exceptions += GrantStoredProceduresPermission("EXECUTE", GetConnectionStringUserID());
                }
                catch (SqlException objException)
                {
                    Logger.Error(objException);

                    exceptions += objException + Environment.NewLine + Environment.NewLine + script + Environment.NewLine + Environment.NewLine;
                }

                try
                {
                    //grant execute or select rights to the public role or userid for all user defined functions based
                    //on what type of function it is (scalar function or table function). This is
                    //necesary because the UpgradeConnectionString will create user defined functions
                    //which restrict execute permissions for the ConnectionString user account.  This is also
                    //necessary when db_owner is not set to "dbo" 
                    exceptions += GrantUserDefinedFunctionsPermission("EXECUTE", "SELECT", GetConnectionStringUserID());
                }
                catch (SqlException objException)
                {
                    Logger.Error(objException);

                    exceptions += objException + Environment.NewLine + Environment.NewLine + script + Environment.NewLine + Environment.NewLine;
                }
            }
            return exceptions;
        }

        public override string ExecuteScript(string connectionString, string script)
        {
            return ExecuteScriptInternal(connectionString, script); 
        }

        public override string ExecuteScript(string connectionString, string script, int timeout)
        {
            return ExecuteScriptInternal(connectionString, script, timeout); 
        }

        public override IDataReader ExecuteSQL(string sql)
        {
            return ExecuteSQLInternal(ConnectionString, sql);
        }

        public override IDataReader ExecuteSQL(string sql, int timeout)
        {
            return ExecuteSQLInternal(ConnectionString, sql, timeout);
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql)
        {
            string errorMessage;
            return ExecuteSQLTemp(connectionString, sql, out errorMessage);
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeout)
        {
            string errorMessage;
            return ExecuteSQLTemp(connectionString, sql, timeout, out errorMessage);
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, out string errorMessage)
        {
            return ExecuteSQLInternal(connectionString, sql, 0, out errorMessage);
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeout, out string errorMessage)
        {
            return ExecuteSQLInternal(connectionString, sql, timeout, out errorMessage);
        }

        #endregion
    }
}