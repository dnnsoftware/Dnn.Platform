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

#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Data
{
    public sealed class SqlDataProvider : DataProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SqlDataProvider));
        private static DatabaseConnectionProvider _dbConnectionProvider = DatabaseConnectionProvider.Instance() ?? new SqlDatabaseConnectionProvider();

        #region Private Members

        private const string ScriptDelimiter = "(?<=(?:[^\\w]+|^))GO(?=(?: |\\t)*?(?:\\r?\\n|$))";

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
                _dbConnectionProvider.ExecuteReader(connectionString, CommandType.StoredProcedure, 0, owner + qualifier + "GetDatabaseVersion");
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

        private string ExecuteScriptInternal(string connectionString, string script, int timeoutSec = 0)
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

                    try
                    {
                        Logger.Trace("Executing SQL Script " + sql);

                        _dbConnectionProvider.ExecuteNonQuery(connectionString, CommandType.Text, timeoutSec, sql);
                    }
                    catch (SqlException objException)
                    {
                        Logger.Error(objException);
                        exceptions += objException + Environment.NewLine + Environment.NewLine + sql + Environment.NewLine + Environment.NewLine;
                    }
                }
            }

            return exceptions;
        }

        private IDataReader ExecuteSQLInternal(string connectionString, string sql, int timeoutSec = 0)
        {
            string errorMessage;
            return ExecuteSQLInternal(connectionString, sql, timeoutSec, out errorMessage);
        }

        private IDataReader ExecuteSQLInternal(string connectionString, string sql, int timeoutSec, out string errorMessage)
        {
            try
            {
                sql = DataUtil.ReplaceTokens(sql);
                errorMessage = "";

                if (string.IsNullOrEmpty(connectionString))
                    throw new ArgumentNullException(nameof(connectionString));

                return _dbConnectionProvider.ExecuteSql(connectionString, CommandType.Text, timeoutSec, sql);
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
            //Normalize to uppercase before all of the comparisons
            var connectionStringUppercase = ConnectionString.ToUpper();
            if (connectionStringUppercase.Contains("USER ID") || connectionStringUppercase.Contains("UID") || connectionStringUppercase.Contains("USER"))
            {
                string[] ConnSettings = connectionStringUppercase.Split(';');

                foreach (string s in ConnSettings)
                {
                    if (s != string.Empty)
                    {
                        string[] ConnSetting = s.Split('=');
                        if ("USER ID|UID|USER".Contains(ConnSetting[0].Trim()))
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
            //grant rights to a login or role for all stored procedures
            var sql = "if exists (select * from dbo.sysusers where name='" + LoginOrRole + "')"
                + "  begin"
                + "    declare @exec nvarchar(2000) "
                + "    declare @name varchar(150) "
                + "    declare sp_cursor cursor for select o.name as name "
                + "    from dbo.sysobjects o "
                + "    where ( OBJECTPROPERTY(o.id, N'IsProcedure') = 1 or OBJECTPROPERTY(o.id, N'IsExtendedProc') = 1 or OBJECTPROPERTY(o.id, N'IsReplProc') = 1 ) "
                + "    and OBJECTPROPERTY(o.id, N'IsMSShipped') = 0 "
                + "    and o.name not like N'#%%' "
                + "    and (left(o.name,len('" + ObjectQualifier + "')) = '" + ObjectQualifier + "' or left(o.name,7) = 'aspnet_') "
                + "    open sp_cursor "
                + "    fetch sp_cursor into @name "
                + "    while @@fetch_status >= 0 "
                + "      begin"
                + "        select @exec = 'grant " + Permission + " on [' +  @name  + '] to [" + LoginOrRole + "]'"
                + "        execute (@exec)"
                + "        fetch sp_cursor into @name "
                + "      end "
                + "    deallocate sp_cursor"
                + "  end ";

            return ExecuteUpgradedConnectionQuery(sql);
        }

        private string GrantUserDefinedFunctionsPermission(string ScalarPermission, string TablePermission, string LoginOrRole)
        {
            //grant EXECUTE rights to a login or role for all functions
            var sql = "if exists (select * from dbo.sysusers where name='" + LoginOrRole + "')"
                + "  begin"
                + "    declare @exec nvarchar(2000) "
                + "    declare @name varchar(150) "
                + "    declare @isscalarfunction int "
                + "    declare @istablefunction int "
                + "    declare sp_cursor cursor for select o.name as name, OBJECTPROPERTY(o.id, N'IsScalarFunction') as IsScalarFunction "
                + "    from dbo.sysobjects o "
                + "    where ( OBJECTPROPERTY(o.id, N'IsScalarFunction') = 1 OR OBJECTPROPERTY(o.id, N'IsTableFunction') = 1 ) "
                + "      and OBJECTPROPERTY(o.id, N'IsMSShipped') = 0 "
                + "      and o.name not like N'#%%' "
                + "      and (left(o.name,len('" + ObjectQualifier + "')) = '" + ObjectQualifier + "' or left(o.name,7) = 'aspnet_') "
                + "    open sp_cursor "
                + "    fetch sp_cursor into @name, @isscalarfunction "
                + "    while @@fetch_status >= 0 "
                + "      begin "
                + "        if @IsScalarFunction = 1 "
                + "          begin"
                + "            select @exec = 'grant " + ScalarPermission + " on [' +  @name  + '] to [" + LoginOrRole + "]'"
                + "            execute (@exec)"
                + "            fetch sp_cursor into @name, @isscalarfunction  "
                + "          end "
                + "        else "
                + "          begin"
                + "            select @exec = 'grant " + TablePermission + " on [' +  @name  + '] to [" + LoginOrRole + "]'"
                + "            execute (@exec)"
                + "            fetch sp_cursor into @name, @isscalarfunction  "
                + "          end "
                + "      end "
                + "    deallocate sp_cursor"
                + "  end ";

            return ExecuteUpgradedConnectionQuery(sql);
        }

        private string ExecuteUpgradedConnectionQuery(string sql)
        {
            var exceptions = string.Empty;

            try
            {
                _dbConnectionProvider.ExecuteNonQuery(UpgradeConnectionString, CommandType.Text, 0, sql);
            }
            catch (SqlException objException)
            {
                Logger.Error(objException);

                exceptions += objException + Environment.NewLine + Environment.NewLine + sql + Environment.NewLine + Environment.NewLine;
            }
            return exceptions;
        }

        #endregion

        #region Abstract Methods

        public override void ExecuteNonQuery(string procedureName, params object[] commandParameters)
        {
            _dbConnectionProvider.ExecuteNonQuery(ConnectionString, CommandType.StoredProcedure, 0, DatabaseOwner + ObjectQualifier + procedureName, commandParameters);
        }

        public override void ExecuteNonQuery(int timeoutSec, string procedureName, params object[] commandParameters)
        {
            _dbConnectionProvider.ExecuteNonQuery(ConnectionString, CommandType.StoredProcedure, timeoutSec, DatabaseOwner + ObjectQualifier + procedureName, commandParameters);
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable)
        {
            _dbConnectionProvider.BulkInsert(ConnectionString, 0, DatabaseOwner + ObjectQualifier + procedureName, tableParameterName, dataTable);
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec)
        {
            _dbConnectionProvider.BulkInsert(ConnectionString, timeoutSec, DatabaseOwner + ObjectQualifier + procedureName, tableParameterName, dataTable);
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, Dictionary<string, object> commandParameters)
        {
            _dbConnectionProvider.BulkInsert(ConnectionString, 0, DatabaseOwner + ObjectQualifier + procedureName, tableParameterName, dataTable, commandParameters);
        }

        public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec, Dictionary<string, object> commandParameters)
        {
            _dbConnectionProvider.BulkInsert(ConnectionString, 0, DatabaseOwner + ObjectQualifier + procedureName, tableParameterName, dataTable, commandParameters);
        }

        public override IDataReader ExecuteReader(string procedureName, params object[] commandParameters)
        {
            return _dbConnectionProvider.ExecuteReader(ConnectionString, CommandType.StoredProcedure, 0, DatabaseOwner + ObjectQualifier + procedureName, commandParameters);
        }

        public override IDataReader ExecuteReader(int timeoutSec, string procedureName, params object[] commandParameters)
        {
            return _dbConnectionProvider.ExecuteReader(ConnectionString, CommandType.StoredProcedure, timeoutSec, DatabaseOwner + ObjectQualifier + procedureName, commandParameters);
        }

        public override T ExecuteScalar<T>(string procedureName, params object[] commandParameters)
        {
            return _dbConnectionProvider.ExecuteScalar<T>(ConnectionString, CommandType.StoredProcedure, 0, DatabaseOwner + ObjectQualifier + procedureName, commandParameters);
        }

        public override T ExecuteScalar<T>(int timeoutSec, string procedureName, params object[] commandParameters)
        {
            return _dbConnectionProvider.ExecuteScalar<T>(ConnectionString, CommandType.StoredProcedure, timeoutSec, DatabaseOwner + ObjectQualifier + procedureName, commandParameters);
        }

        public override string ExecuteScript(string script)
        {
            return ExecuteScript(script, 0);
        }

        public override string ExecuteScript(string script, int timeoutSec)
        {
            string exceptions = ExecuteScriptInternal(UpgradeConnectionString, script, timeoutSec);

            //if the upgrade connection string is specified or or db_owner setting is not set to dbo
            if (UpgradeConnectionString != ConnectionString || !DatabaseOwner.Trim().Equals("dbo.", StringComparison.InvariantCultureIgnoreCase))
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

        public override string ExecuteScript(string connectionString, string script, int timeoutSec)
        {
            return ExecuteScriptInternal(connectionString, script, timeoutSec);
        }

        public override IDataReader ExecuteSQL(string sql)
        {
            return ExecuteSQLInternal(ConnectionString, sql);
        }

        public override IDataReader ExecuteSQL(string sql, int timeoutSec)
        {
            return ExecuteSQLInternal(ConnectionString, sql, timeoutSec);
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql)
        {
            string errorMessage;
            return ExecuteSQLTemp(connectionString, sql, out errorMessage);
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec)
        {
            string errorMessage;
            return ExecuteSQLTemp(connectionString, sql, timeoutSec, out errorMessage);
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, out string errorMessage)
        {
            return ExecuteSQLInternal(connectionString, sql, 0, out errorMessage);
        }

        public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec, out string errorMessage)
        {
            return ExecuteSQLInternal(connectionString, sql, timeoutSec, out errorMessage);
        }

        #endregion
    }
}
