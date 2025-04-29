// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Instrumentation;

public sealed class SqlDataProvider : DataProvider
{
    private const string ScriptDelimiter = "(?<=(?:[^\\w]+|^))GO(?=(?: |\\t)*?(?:\\r?\\n|$))";
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SqlDataProvider));
    private static DatabaseConnectionProvider dbConnectionProvider = DatabaseConnectionProvider.Instance() ?? new SqlDatabaseConnectionProvider();

    /// <inheritdoc/>
    public override bool IsConnectionValid => this.CanConnect();

    /// <inheritdoc/>
    public override Dictionary<string, string> Settings
    {
        get
        {
            return ComponentFactory.GetComponentSettings<SqlDataProvider>() as Dictionary<string, string>;
        }
    }

    /// <inheritdoc/>
    public override void ExecuteNonQuery(string procedureName, params object[] commandParameters)
    {
        dbConnectionProvider.ExecuteNonQuery(this.ConnectionString, CommandType.StoredProcedure, 0, this.DatabaseOwner + this.ObjectQualifier + procedureName, commandParameters);
    }

    /// <inheritdoc/>
    public override void ExecuteNonQuery(int timeoutSec, string procedureName, params object[] commandParameters)
    {
        dbConnectionProvider.ExecuteNonQuery(this.ConnectionString, CommandType.StoredProcedure, timeoutSec, this.DatabaseOwner + this.ObjectQualifier + procedureName, commandParameters);
    }

    /// <inheritdoc/>
    public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable)
    {
        dbConnectionProvider.BulkInsert(this.ConnectionString, 0, this.DatabaseOwner + this.ObjectQualifier + procedureName, tableParameterName, dataTable);
    }

    /// <inheritdoc/>
    public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec)
    {
        dbConnectionProvider.BulkInsert(this.ConnectionString, timeoutSec, this.DatabaseOwner + this.ObjectQualifier + procedureName, tableParameterName, dataTable);
    }

    /// <inheritdoc/>
    public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, Dictionary<string, object> commandParameters)
    {
        dbConnectionProvider.BulkInsert(this.ConnectionString, 0, this.DatabaseOwner + this.ObjectQualifier + procedureName, tableParameterName, dataTable, commandParameters);
    }

    /// <inheritdoc/>
    public override void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec, Dictionary<string, object> commandParameters)
    {
        dbConnectionProvider.BulkInsert(this.ConnectionString, 0, this.DatabaseOwner + this.ObjectQualifier + procedureName, tableParameterName, dataTable, commandParameters);
    }

    /// <inheritdoc/>
    public override IDataReader ExecuteReader(string procedureName, params object[] commandParameters)
    {
        return dbConnectionProvider.ExecuteReader(this.ConnectionString, CommandType.StoredProcedure, 0, this.DatabaseOwner + this.ObjectQualifier + procedureName, commandParameters);
    }

    /// <inheritdoc/>
    public override IDataReader ExecuteReader(int timeoutSec, string procedureName, params object[] commandParameters)
    {
        return dbConnectionProvider.ExecuteReader(this.ConnectionString, CommandType.StoredProcedure, timeoutSec, this.DatabaseOwner + this.ObjectQualifier + procedureName, commandParameters);
    }

    /// <inheritdoc/>
    public override T ExecuteScalar<T>(string procedureName, params object[] commandParameters)
    {
        return dbConnectionProvider.ExecuteScalar<T>(this.ConnectionString, CommandType.StoredProcedure, 0, this.DatabaseOwner + this.ObjectQualifier + procedureName, commandParameters);
    }

    /// <inheritdoc/>
    public override T ExecuteScalar<T>(int timeoutSec, string procedureName, params object[] commandParameters)
    {
        return dbConnectionProvider.ExecuteScalar<T>(this.ConnectionString, CommandType.StoredProcedure, timeoutSec, this.DatabaseOwner + this.ObjectQualifier + procedureName, commandParameters);
    }

    /// <inheritdoc/>
    public override string ExecuteScript(string script)
    {
        return this.ExecuteScript(script, 0);
    }

    /// <inheritdoc/>
    public override string ExecuteScript(string script, int timeoutSec)
    {
        string exceptions = this.ExecuteScriptInternal(this.UpgradeConnectionString, script, timeoutSec);

        // if the upgrade connection string is specified or or db_owner setting is not set to dbo
        if (this.UpgradeConnectionString != this.ConnectionString || !this.DatabaseOwner.Trim().Equals("dbo.", StringComparison.InvariantCultureIgnoreCase))
        {
            try
            {
                // grant execute rights to the public role or userid for all stored procedures. This is
                // necesary because the UpgradeConnectionString will create stored procedures
                // which restrict execute permissions for the ConnectionString user account. This is also
                // necessary when db_owner is not set to "dbo"
                exceptions += this.GrantStoredProceduresPermission("EXECUTE", this.GetConnectionStringUserID());
            }
            catch (SqlException objException)
            {
                Logger.Error(objException);

                exceptions += objException + Environment.NewLine + Environment.NewLine + script + Environment.NewLine + Environment.NewLine;
            }

            try
            {
                // grant execute or select rights to the public role or userid for all user defined functions based
                // on what type of function it is (scalar function or table function). This is
                // necesary because the UpgradeConnectionString will create user defined functions
                // which restrict execute permissions for the ConnectionString user account.  This is also
                // necessary when db_owner is not set to "dbo"
                exceptions += this.GrantUserDefinedFunctionsPermission("EXECUTE", "SELECT", this.GetConnectionStringUserID());
            }
            catch (SqlException objException)
            {
                Logger.Error(objException);

                exceptions += objException + Environment.NewLine + Environment.NewLine + script + Environment.NewLine + Environment.NewLine;
            }
        }

        return exceptions;
    }

    /// <inheritdoc/>
    public override string ExecuteScript(string connectionString, string script)
    {
        return this.ExecuteScriptInternal(connectionString, script);
    }

    /// <inheritdoc/>
    public override string ExecuteScript(string connectionString, string script, int timeoutSec)
    {
        return this.ExecuteScriptInternal(connectionString, script, timeoutSec);
    }

    /// <inheritdoc/>
    public override IDataReader ExecuteSQL(string sql)
    {
        return this.ExecuteSQLInternal(this.ConnectionString, sql);
    }

    /// <inheritdoc/>
    public override IDataReader ExecuteSQL(string sql, int timeoutSec)
    {
        return this.ExecuteSQLInternal(this.ConnectionString, sql, timeoutSec);
    }

    /// <inheritdoc/>
    public override IDataReader ExecuteSQLTemp(string connectionString, string sql)
    {
        string errorMessage;
        return this.ExecuteSQLTemp(connectionString, sql, out errorMessage);
    }

    /// <inheritdoc/>
    public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec)
    {
        string errorMessage;
        return this.ExecuteSQLTemp(connectionString, sql, timeoutSec, out errorMessage);
    }

    /// <inheritdoc/>
    public override IDataReader ExecuteSQLTemp(string connectionString, string sql, out string errorMessage)
    {
        return this.ExecuteSQLInternal(connectionString, sql, 0, out errorMessage);
    }

    /// <inheritdoc/>
    public override IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec, out string errorMessage)
    {
        return this.ExecuteSQLInternal(connectionString, sql, timeoutSec, out errorMessage);
    }

    private bool CanConnect()
    {
        bool connectionValid = true;

        try
        {
            this.ExecuteNonQuery("GetDatabaseVersion");
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
        string exceptions = string.Empty;

        var sqlDelimiterRegex = RegexUtils.GetCachedRegex(ScriptDelimiter, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        string[] sqlStatements = sqlDelimiterRegex.Split(script);
        foreach (string statement in sqlStatements)
        {
            var sql = statement.Trim();
            if (!string.IsNullOrEmpty(sql))
            {
                // script dynamic substitution
                sql = DataUtil.ReplaceTokens(sql);

                try
                {
                    Logger.Trace("Executing SQL Script " + sql);

                    dbConnectionProvider.ExecuteNonQuery(connectionString, CommandType.Text, timeoutSec, sql);
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
        return this.ExecuteSQLInternal(connectionString, sql, timeoutSec, out errorMessage);
    }

    private IDataReader ExecuteSQLInternal(string connectionString, string sql, int timeoutSec, out string errorMessage)
    {
        try
        {
            sql = DataUtil.ReplaceTokens(sql);
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            return dbConnectionProvider.ExecuteSql(connectionString, CommandType.Text, timeoutSec, sql);
        }
        catch (SqlException sqlException)
        {
            // error in SQL query
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
        string dbUser = "public";

        // If connection string does not use integrated security, then get user id.
        // Normalize to uppercase before all of the comparisons
        var connectionStringUppercase = this.ConnectionString.ToUpper();
        if (connectionStringUppercase.Contains("USER ID") || connectionStringUppercase.Contains("UID") || connectionStringUppercase.Contains("USER"))
        {
            string[] connSettings = connectionStringUppercase.Split(';');

            foreach (string s in connSettings)
            {
                if (s != string.Empty)
                {
                    string[] connSetting = s.Split('=');
                    if ("USER ID|UID|USER".Contains(connSetting[0].Trim()))
                    {
                        dbUser = connSetting[1].Trim();
                    }
                }
            }
        }

        return dbUser;
    }

    private string GrantStoredProceduresPermission(string permission, string loginOrRole)
    {
        // grant rights to a login or role for all stored procedures
        var sql = "if exists (select * from dbo.sysusers where name='" + loginOrRole + "')"
                  + "  begin"
                  + "    declare @exec nvarchar(2000) "
                  + "    declare @name varchar(150) "
                  + "    declare sp_cursor cursor for select o.name as name "
                  + "    from dbo.sysobjects o "
                  + "    where ( OBJECTPROPERTY(o.id, N'IsProcedure') = 1 or OBJECTPROPERTY(o.id, N'IsExtendedProc') = 1 or OBJECTPROPERTY(o.id, N'IsReplProc') = 1 ) "
                  + "    and OBJECTPROPERTY(o.id, N'IsMSShipped') = 0 "
                  + "    and o.name not like N'#%%' "
                  + "    and (left(o.name,len('" + this.ObjectQualifier + "')) = '" + this.ObjectQualifier + "' or left(o.name,7) = 'aspnet_') "
                  + "    open sp_cursor "
                  + "    fetch sp_cursor into @name "
                  + "    while @@fetch_status >= 0 "
                  + "      begin"
                  + "        select @exec = 'grant " + permission + " on [' +  @name  + '] to [" + loginOrRole + "]'"
                  + "        execute (@exec)"
                  + "        fetch sp_cursor into @name "
                  + "      end "
                  + "    deallocate sp_cursor"
                  + "  end ";

        return this.ExecuteUpgradedConnectionQuery(sql);
    }

    private string GrantUserDefinedFunctionsPermission(string scalarPermission, string tablePermission, string loginOrRole)
    {
        // grant EXECUTE rights to a login or role for all functions
        var sql = "if exists (select * from dbo.sysusers where name='" + loginOrRole + "')"
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
                  + "      and (left(o.name,len('" + this.ObjectQualifier + "')) = '" + this.ObjectQualifier + "' or left(o.name,7) = 'aspnet_') "
                  + "    open sp_cursor "
                  + "    fetch sp_cursor into @name, @isscalarfunction "
                  + "    while @@fetch_status >= 0 "
                  + "      begin "
                  + "        if @IsScalarFunction = 1 "
                  + "          begin"
                  + "            select @exec = 'grant " + scalarPermission + " on [' +  @name  + '] to [" + loginOrRole + "]'"
                  + "            execute (@exec)"
                  + "            fetch sp_cursor into @name, @isscalarfunction  "
                  + "          end "
                  + "        else "
                  + "          begin"
                  + "            select @exec = 'grant " + tablePermission + " on [' +  @name  + '] to [" + loginOrRole + "]'"
                  + "            execute (@exec)"
                  + "            fetch sp_cursor into @name, @isscalarfunction  "
                  + "          end "
                  + "      end "
                  + "    deallocate sp_cursor"
                  + "  end ";

        return this.ExecuteUpgradedConnectionQuery(sql);
    }

    private string ExecuteUpgradedConnectionQuery(string sql)
    {
        var exceptions = string.Empty;

        try
        {
            dbConnectionProvider.ExecuteNonQuery(this.UpgradeConnectionString, CommandType.Text, 0, sql);
        }
        catch (SqlException objException)
        {
            Logger.Error(objException);

            exceptions += objException + Environment.NewLine + Environment.NewLine + sql + Environment.NewLine + Environment.NewLine;
        }

        return exceptions;
    }
}
