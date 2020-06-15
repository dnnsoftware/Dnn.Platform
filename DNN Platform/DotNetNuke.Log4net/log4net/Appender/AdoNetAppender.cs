// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

//
// Licensed to the Apache Software Foundation (ASF) under one or more
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership.
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

// SSCLI 1.0 has no support for ADO.NET
#if !SSCLI

using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;

using log4net.Core;
using log4net.Layout;
using log4net.Util;

namespace log4net.Appender
{
    /// <summary>
    /// Appender that logs to a database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="AdoNetAppender"/> appends logging events to a table within a
    /// database. The appender can be configured to specify the connection
    /// string by setting the <see cref="ConnectionString"/> property.
    /// The connection type (provider) can be specified by setting the <see cref="ConnectionType"/>
    /// property. For more information on database connection strings for
    /// your specific database see <a href="http://www.connectionstrings.com/">http://www.connectionstrings.com/</a>.
    /// </para>
    /// <para>
    /// Records are written into the database either using a prepared
    /// statement or a stored procedure. The <see cref="CommandType"/> property
    /// is set to <see cref="System.Data.CommandType.Text"/> (<c>System.Data.CommandType.Text</c>) to specify a prepared statement
    /// or to <see cref="System.Data.CommandType.StoredProcedure"/> (<c>System.Data.CommandType.StoredProcedure</c>) to specify a stored
    /// procedure.
    /// </para>
    /// <para>
    /// The prepared statement text or the name of the stored procedure
    /// must be set in the <see cref="CommandText"/> property.
    /// </para>
    /// <para>
    /// The prepared statement or stored procedure can take a number
    /// of parameters. Parameters are added using the <see cref="AddParameter"/>
    /// method. This adds a single <see cref="AdoNetAppenderParameter"/> to the
    /// ordered list of parameters. The <see cref="AdoNetAppenderParameter"/>
    /// type may be subclassed if required to provide database specific
    /// functionality. The <see cref="AdoNetAppenderParameter"/> specifies
    /// the parameter name, database type, size, and how the value should
    /// be generated using a <see cref="ILayout"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// An example of a SQL Server table that could be logged to:
    /// <code lang="SQL">
    /// CREATE TABLE [dbo].[Log] (
    ///   [ID] [int] IDENTITY (1, 1) NOT NULL ,
    ///   [Date] [datetime] NOT NULL ,
    ///   [Thread] [varchar] (255) NOT NULL ,
    ///   [Level] [varchar] (20) NOT NULL ,
    ///   [Logger] [varchar] (255) NOT NULL ,
    ///   [Message] [varchar] (4000) NOT NULL
    /// ) ON [PRIMARY]
    /// </code>
    /// </example>
    /// <example>
    /// An example configuration to log to the above table:
    /// <code lang="XML" escaped="true">
    /// <appender name="AdoNetAppender_SqlServer" type="log4net.Appender.AdoNetAppender" >
    ///   <connectionType value="System.Data.SqlClient.SqlConnection, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
    ///   <connectionString value="data source=SQLSVR;initial catalog=test_log4net;integrated security=false;persist security info=True;User ID=sa;Password=sa" />
    ///   <commandText value="INSERT INTO Log ([Date],[Thread],[Level],[Logger],[Message]) VALUES (@log_date, @thread, @log_level, @logger, @message)" />
    ///   <parameter>
    ///     <parameterName value="@log_date" />
    ///     <dbType value="DateTime" />
    ///     <layout type="log4net.Layout.PatternLayout" value="%date{yyyy'-'MM'-'dd HH':'mm':'ss'.'fff}" />
    ///   </parameter>
    ///   <parameter>
    ///     <parameterName value="@thread" />
    ///     <dbType value="String" />
    ///     <size value="255" />
    ///     <layout type="log4net.Layout.PatternLayout" value="%thread" />
    ///   </parameter>
    ///   <parameter>
    ///     <parameterName value="@log_level" />
    ///     <dbType value="String" />
    ///     <size value="50" />
    ///     <layout type="log4net.Layout.PatternLayout" value="%level" />
    ///   </parameter>
    ///   <parameter>
    ///     <parameterName value="@logger" />
    ///     <dbType value="String" />
    ///     <size value="255" />
    ///     <layout type="log4net.Layout.PatternLayout" value="%logger" />
    ///   </parameter>
    ///   <parameter>
    ///     <parameterName value="@message" />
    ///     <dbType value="String" />
    ///     <size value="4000" />
    ///     <layout type="log4net.Layout.PatternLayout" value="%message" />
    ///   </parameter>
    /// </appender>
    /// </code>
    /// </example>
    /// <author>Julian Biddle.</author>
    /// <author>Nicko Cadell.</author>
    /// <author>Gert Driesen.</author>
    /// <author>Lance Nehring.</author>
    public class AdoNetAppender : BufferingAppenderSkeleton
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdoNetAppender" /> class.
        /// </summary>
        /// <remarks>
        /// Public default constructor to initialize a new instance of this class.
        /// </remarks>
        public AdoNetAppender()
        {
            this.ConnectionType = "System.Data.OleDb.OleDbConnection, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            this.UseTransactions = true;
            this.CommandType = System.Data.CommandType.Text;
            this.m_parameters = new ArrayList();
            this.ReconnectOnError = false;
        }

        /// <summary>
        /// Gets or sets the database connection string that is used to connect to
        /// the database.
        /// </summary>
        /// <value>
        /// The database connection string used to connect to the database.
        /// </value>
        /// <remarks>
        /// <para>
        /// The connections string is specific to the connection type.
        /// See <see cref="ConnectionType"/> for more information.
        /// </para>
        /// </remarks>
        /// <example>Connection string for MS Access via ODBC:
        /// <code>"DSN=MS Access Database;UID=admin;PWD=;SystemDB=C:\data\System.mdw;SafeTransactions = 0;FIL=MS Access;DriverID = 25;DBQ=C:\data\train33.mdb"</code>
        /// </example>
        /// <example>Another connection string for MS Access via ODBC:
        /// <code>"Driver={Microsoft Access Driver (*.mdb)};DBQ=C:\Work\cvs_root\log4net-1.2\access.mdb;UID=;PWD=;"</code>
        /// </example>
        /// <example>Connection string for MS Access via OLE DB:
        /// <code>"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Work\cvs_root\log4net-1.2\access.mdb;User Id=;Password=;"</code>
        /// </example>
        public string ConnectionString
        {
            get { return this.m_connectionString; }
            set { this.m_connectionString = value; }
        }

        /// <summary>
        /// Gets or sets the appSettings key from App.Config that contains the connection string.
        /// </summary>
        public string AppSettingsKey
        {
            get { return this.m_appSettingsKey; }
            set { this.m_appSettingsKey = value; }
        }

#if NET_2_0
        /// <summary>
        /// Gets or sets the connectionStrings key from App.Config that contains the connection string.
        /// </summary>
        /// <remarks>
        /// This property requires at least .NET 2.0.
        /// </remarks>
        public string ConnectionStringName
        {
            get { return this.m_connectionStringName; }
            set { this.m_connectionStringName = value; }
        }
#endif

        /// <summary>
        /// Gets or sets the type name of the <see cref="IDbConnection"/> connection
        /// that should be created.
        /// </summary>
        /// <value>
        /// The type name of the <see cref="IDbConnection"/> connection.
        /// </value>
        /// <remarks>
        /// <para>
        /// The type name of the ADO.NET provider to use.
        /// </para>
        /// <para>
        /// The default is to use the OLE DB provider.
        /// </para>
        /// </remarks>
        /// <example>Use the OLE DB Provider. This is the default value.
        /// <code>System.Data.OleDb.OleDbConnection, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</code>
        /// </example>
        /// <example>Use the MS SQL Server Provider.
        /// <code>System.Data.SqlClient.SqlConnection, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</code>
        /// </example>
        /// <example>Use the ODBC Provider.
        /// <code>Microsoft.Data.Odbc.OdbcConnection,Microsoft.Data.Odbc,version=1.0.3300.0,publicKeyToken=b77a5c561934e089,culture=neutral</code>
        /// This is an optional package that you can download from
        /// <a href="http://msdn.microsoft.com/downloads">http://msdn.microsoft.com/downloads</a>
        /// search for <b>ODBC .NET Data Provider</b>.
        /// </example>
        /// <example>Use the Oracle Provider.
        /// <code>System.Data.OracleClient.OracleConnection, System.Data.OracleClient, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</code>
        /// This is an optional package that you can download from
        /// <a href="http://msdn.microsoft.com/downloads">http://msdn.microsoft.com/downloads</a>
        /// search for <b>.NET Managed Provider for Oracle</b>.
        /// </example>
        public string ConnectionType
        {
            get { return this.m_connectionType; }
            set { this.m_connectionType = value; }
        }

        /// <summary>
        /// Gets or sets the command text that is used to insert logging events
        /// into the database.
        /// </summary>
        /// <value>
        /// The command text used to insert logging events into the database.
        /// </value>
        /// <remarks>
        /// <para>
        /// Either the text of the prepared statement or the
        /// name of the stored procedure to execute to write into
        /// the database.
        /// </para>
        /// <para>
        /// The <see cref="CommandType"/> property determines if
        /// this text is a prepared statement or a stored procedure.
        /// </para>
        /// <para>
        /// If this property is not set, the command text is retrieved by invoking
        /// <see cref="GetLogStatement(LoggingEvent)"/>.
        /// </para>
        /// </remarks>
        public string CommandText
        {
            get { return this.m_commandText; }
            set { this.m_commandText = value; }
        }

        /// <summary>
        /// Gets or sets the command type to execute.
        /// </summary>
        /// <value>
        /// The command type to execute.
        /// </value>
        /// <remarks>
        /// <para>
        /// This value may be either <see cref="System.Data.CommandType.Text"/> (<c>System.Data.CommandType.Text</c>) to specify
        /// that the <see cref="CommandText"/> is a prepared statement to execute,
        /// or <see cref="System.Data.CommandType.StoredProcedure"/> (<c>System.Data.CommandType.StoredProcedure</c>) to specify that the
        /// <see cref="CommandText"/> property is the name of a stored procedure
        /// to execute.
        /// </para>
        /// <para>
        /// The default value is <see cref="System.Data.CommandType.Text"/> (<c>System.Data.CommandType.Text</c>).
        /// </para>
        /// </remarks>
        public CommandType CommandType
        {
            get { return this.m_commandType; }
            set { this.m_commandType = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether should transactions be used to insert logging events in the database.
        /// </summary>
        /// <value>
        /// <c>true</c> if transactions should be used to insert logging events in
        /// the database, otherwise <c>false</c>. The default value is <c>true</c>.
        /// </value>
        /// <remarks>
        /// <para>
        /// Gets or sets a value that indicates whether transactions should be used
        /// to insert logging events in the database.
        /// </para>
        /// <para>
        /// When set a single transaction will be used to insert the buffered events
        /// into the database. Otherwise each event will be inserted without using
        /// an explicit transaction.
        /// </para>
        /// </remarks>
        public bool UseTransactions
        {
            get { return this.m_useTransactions; }
            set { this.m_useTransactions = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SecurityContext"/> used to call the NetSend method.
        /// </summary>
        /// <value>
        /// The <see cref="SecurityContext"/> used to call the NetSend method.
        /// </value>
        /// <remarks>
        /// <para>
        /// Unless a <see cref="SecurityContext"/> specified here for this appender
        /// the <see cref="SecurityContextProvider.DefaultProvider"/> is queried for the
        /// security context to use. The default behavior is to use the security context
        /// of the current thread.
        /// </para>
        /// </remarks>
        public SecurityContext SecurityContext
        {
            get { return this.m_securityContext; }
            set { this.m_securityContext = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether should this appender try to reconnect to the database on error.
        /// </summary>
        /// <value>
        /// <c>true</c> if the appender should try to reconnect to the database after an
        /// error has occurred, otherwise <c>false</c>. The default value is <c>false</c>,
        /// i.e. not to try to reconnect.
        /// </value>
        /// <remarks>
        /// <para>
        /// The default behaviour is for the appender not to try to reconnect to the
        /// database if an error occurs. Subsequent logging events are discarded.
        /// </para>
        /// <para>
        /// To force the appender to attempt to reconnect to the database set this
        /// property to <c>true</c>.
        /// </para>
        /// <note>
        /// When the appender attempts to connect to the database there may be a
        /// delay of up to the connection timeout specified in the connection string.
        /// This delay will block the calling application's thread.
        /// Until the connection can be reestablished this potential delay may occur multiple times.
        /// </note>
        /// </remarks>
        public bool ReconnectOnError
        {
            get { return this.m_reconnectOnError; }
            set { this.m_reconnectOnError = value; }
        }

        /// <summary>
        /// Gets or sets the underlying <see cref="IDbConnection" />.
        /// </summary>
        /// <value>
        /// The underlying <see cref="IDbConnection" />.
        /// </value>
        /// <remarks>
        /// <see cref="AdoNetAppender" /> creates a <see cref="IDbConnection" /> to insert
        /// logging events into a database.  Classes deriving from <see cref="AdoNetAppender" />
        /// can use this property to get or set this <see cref="IDbConnection" />.  Use the
        /// underlying <see cref="IDbConnection" /> returned from <see cref="Connection" /> if
        /// you require access beyond that which <see cref="AdoNetAppender" /> provides.
        /// </remarks>
        protected IDbConnection Connection
        {
            get { return this.m_dbConnection; }
            set { this.m_dbConnection = value; }
        }

        /// <summary>
        /// Initialize the appender based on the options set.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is part of the <see cref="IOptionHandler"/> delayed object
        /// activation scheme. The <see cref="ActivateOptions"/> method must
        /// be called on this object after the configuration properties have
        /// been set. Until <see cref="ActivateOptions"/> is called this
        /// object is in an undefined state and must not be used.
        /// </para>
        /// <para>
        /// If any of the configuration properties are modified then
        /// <see cref="ActivateOptions"/> must be called again.
        /// </para>
        /// </remarks>
        public override void ActivateOptions()
        {
            base.ActivateOptions();

            if (this.SecurityContext == null)
            {
                this.SecurityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
            }

            this.InitializeDatabaseConnection();
        }

        /// <summary>
        /// Override the parent method to close the database.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Closes the database command and database connection.
        /// </para>
        /// </remarks>
        protected override void OnClose()
        {
            base.OnClose();
            this.DiposeConnection();
        }

        /// <summary>
        /// Inserts the events into the database.
        /// </summary>
        /// <param name="events">The events to insert into the database.</param>
        /// <remarks>
        /// <para>
        /// Insert all the events specified in the <paramref name="events"/>
        /// array into the database.
        /// </para>
        /// </remarks>
        protected override void SendBuffer(LoggingEvent[] events)
        {
            if (this.ReconnectOnError && (this.Connection == null || this.Connection.State != ConnectionState.Open))
            {
                LogLog.Debug(declaringType, "Attempting to reconnect to database. Current Connection State: " + ((this.Connection == null) ? SystemInfo.NullText : this.Connection.State.ToString()));

                this.InitializeDatabaseConnection();
            }

            // Check that the connection exists and is open
            if (this.Connection != null && this.Connection.State == ConnectionState.Open)
            {
                if (this.UseTransactions)
                {
                    // Create transaction
                    // NJC - Do this on 2 lines because it can confuse the debugger
                    using (IDbTransaction dbTran = this.Connection.BeginTransaction())
                    {
                        try
                        {
                            this.SendBuffer(dbTran, events);

                            // commit transaction
                            dbTran.Commit();
                        }
                        catch (Exception ex)
                        {
                            // rollback the transaction
                            try
                            {
                                dbTran.Rollback();
                            }
                            catch (Exception)
                            {
                                // Ignore exception
                            }

                            // Can't insert into the database. That's a bad thing
                            this.ErrorHandler.Error("Exception while writing to database", ex);
                        }
                    }
                }
                else
                {
                    // Send without transaction
                    this.SendBuffer(null, events);
                }
            }
        }

        /// <summary>
        /// Adds a parameter to the command.
        /// </summary>
        /// <param name="parameter">The parameter to add to the command.</param>
        /// <remarks>
        /// <para>
        /// Adds a parameter to the ordered list of command parameters.
        /// </para>
        /// </remarks>
        public void AddParameter(AdoNetAppenderParameter parameter)
        {
            this.m_parameters.Add(parameter);
        }

        /// <summary>
        /// Writes the events to the database using the transaction specified.
        /// </summary>
        /// <param name="dbTran">The transaction that the events will be executed under.</param>
        /// <param name="events">The array of events to insert into the database.</param>
        /// <remarks>
        /// <para>
        /// The transaction argument can be <c>null</c> if the appender has been
        /// configured not to use transactions. See <see cref="UseTransactions"/>
        /// property for more information.
        /// </para>
        /// </remarks>
        protected virtual void SendBuffer(IDbTransaction dbTran, LoggingEvent[] events)
        {
            // string.IsNotNullOrWhiteSpace() does not exist in ancient .NET frameworks
            if (this.CommandText != null && this.CommandText.Trim() != string.Empty)
            {
                using (IDbCommand dbCmd = this.Connection.CreateCommand())
                {
                    // Set the command string
                    dbCmd.CommandText = this.CommandText;

                    // Set the command type
                    dbCmd.CommandType = this.CommandType;

                    // Send buffer using the prepared command object
                    if (dbTran != null)
                    {
                        dbCmd.Transaction = dbTran;
                    }

                    // prepare the command, which is significantly faster
                    dbCmd.Prepare();

                    // run for all events
                    foreach (LoggingEvent e in events)
                    {
                        // clear parameters that have been set
                        dbCmd.Parameters.Clear();

                        // Set the parameter values
                        foreach (AdoNetAppenderParameter param in this.m_parameters)
                        {
                            param.Prepare(dbCmd);
                            param.FormatValue(dbCmd, e);
                        }

                        // Execute the query
                        dbCmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                // create a new command
                using (IDbCommand dbCmd = this.Connection.CreateCommand())
                {
                    if (dbTran != null)
                    {
                        dbCmd.Transaction = dbTran;
                    }

                    // run for all events
                    foreach (LoggingEvent e in events)
                    {
                        // Get the command text from the Layout
                        string logStatement = this.GetLogStatement(e);

                        LogLog.Debug(declaringType, "LogStatement [" + logStatement + "]");

                        dbCmd.CommandText = logStatement;
                        dbCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Formats the log message into database statement text.
        /// </summary>
        /// <param name="logEvent">The event being logged.</param>
        /// <remarks>
        /// This method can be overridden by subclasses to provide
        /// more control over the format of the database statement.
        /// </remarks>
        /// <returns>
        /// Text that can be passed to a <see cref="System.Data.IDbCommand"/>.
        /// </returns>
        protected virtual string GetLogStatement(LoggingEvent logEvent)
        {
            if (this.Layout == null)
            {
                this.ErrorHandler.Error("AdoNetAppender: No Layout specified.");
                return string.Empty;
            }
            else
            {
                StringWriter writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
                this.Layout.Format(writer, logEvent);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Creates an <see cref="IDbConnection"/> instance used to connect to the database.
        /// </summary>
        /// <remarks>
        /// This method is called whenever a new IDbConnection is needed (i.e. when a reconnect is necessary).
        /// </remarks>
        /// <param name="connectionType">The <see cref="Type"/> of the <see cref="IDbConnection"/> object.</param>
        /// <param name="connectionString">The connectionString output from the ResolveConnectionString method.</param>
        /// <returns>An <see cref="IDbConnection"/> instance with a valid connection string.</returns>
        protected virtual IDbConnection CreateConnection(Type connectionType, string connectionString)
        {
            IDbConnection connection = (IDbConnection)Activator.CreateInstance(connectionType);
            connection.ConnectionString = connectionString;
            return connection;
        }

        /// <summary>
        /// Resolves the connection string from the ConnectionString, ConnectionStringName, or AppSettingsKey
        /// property.
        /// </summary>
        /// <remarks>
        /// ConnectiongStringName is only supported on .NET 2.0 and higher.
        /// </remarks>
        /// <param name="connectionStringContext">Additional information describing the connection string.</param>
        /// <returns>A connection string used to connect to the database.</returns>
        protected virtual string ResolveConnectionString(out string connectionStringContext)
        {
            if (this.ConnectionString != null && this.ConnectionString.Length > 0)
            {
                connectionStringContext = "ConnectionString";
                return this.ConnectionString;
            }

#if NET_2_0
            if (!string.IsNullOrEmpty(this.ConnectionStringName))
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[this.ConnectionStringName];
                if (settings != null)
                {
                    connectionStringContext = "ConnectionStringName";
                    return settings.ConnectionString;
                }
                else
                {
                    throw new LogException("Unable to find [" + this.ConnectionStringName + "] ConfigurationManager.ConnectionStrings item");
                }
            }
#endif

            if (this.AppSettingsKey != null && this.AppSettingsKey.Length > 0)
            {
                connectionStringContext = "AppSettingsKey";
                string appSettingsConnectionString = SystemInfo.GetAppSetting(this.AppSettingsKey);
                if (appSettingsConnectionString == null || appSettingsConnectionString.Length == 0)
                {
                    throw new LogException("Unable to find [" + this.AppSettingsKey + "] AppSettings key.");
                }

                return appSettingsConnectionString;
            }

            connectionStringContext = "Unable to resolve connection string from ConnectionString, ConnectionStrings, or AppSettings.";
            return string.Empty;
        }

        /// <summary>
        /// Retrieves the class type of the ADO.NET provider.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Gets the Type of the ADO.NET provider to use to connect to the
        /// database. This method resolves the type specified in the
        /// <see cref="ConnectionType"/> property.
        /// </para>
        /// <para>
        /// Subclasses can override this method to return a different type
        /// if necessary.
        /// </para>
        /// </remarks>
        /// <returns>The <see cref="Type"/> of the ADO.NET provider.</returns>
        protected virtual Type ResolveConnectionType()
        {
            try
            {
                return SystemInfo.GetTypeFromString(this.ConnectionType, true, false);
            }
            catch (Exception ex)
            {
                this.ErrorHandler.Error("Failed to load connection type [" + this.ConnectionType + "]", ex);
                throw;
            }
        }

        /// <summary>
        /// Connects to the database.
        /// </summary>
        private void InitializeDatabaseConnection()
        {
            string connectionStringContext = "Unable to determine connection string context.";
            string resolvedConnectionString = string.Empty;

            try
            {
                this.DiposeConnection();

                // Set the connection string
                resolvedConnectionString = this.ResolveConnectionString(out connectionStringContext);

                this.Connection = this.CreateConnection(this.ResolveConnectionType(), resolvedConnectionString);

                using (this.SecurityContext.Impersonate(this))
                {
                    // Open the database connection
                    this.Connection.Open();
                }
            }
            catch (Exception e)
            {
                // Sadly, your connection string is bad.
                this.ErrorHandler.Error("Could not open database connection [" + resolvedConnectionString + "]. Connection string context [" + connectionStringContext + "].", e);

                this.Connection = null;
            }
        }

        /// <summary>
        /// Cleanup the existing connection.
        /// </summary>
        /// <remarks>
        /// Calls the IDbConnection's <see cref="IDbConnection.Close"/> method.
        /// </remarks>
        private void DiposeConnection()
        {
            if (this.Connection != null)
            {
                try
                {
                    this.Connection.Close();
                }
                catch (Exception ex)
                {
                    LogLog.Warn(declaringType, "Exception while disposing cached connection object", ex);
                }

                this.Connection = null;
            }
        }

        /// <summary>
        /// The list of <see cref="AdoNetAppenderParameter"/> objects.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The list of <see cref="AdoNetAppenderParameter"/> objects.
        /// </para>
        /// </remarks>
        protected ArrayList m_parameters;

        /// <summary>
        /// The security context to use for privileged calls.
        /// </summary>
        private SecurityContext m_securityContext;

        /// <summary>
        /// The <see cref="IDbConnection" /> that will be used
        /// to insert logging events into a database.
        /// </summary>
        private IDbConnection m_dbConnection;

        /// <summary>
        /// Database connection string.
        /// </summary>
        private string m_connectionString;

        /// <summary>
        /// The appSettings key from App.Config that contains the connection string.
        /// </summary>
        private string m_appSettingsKey;

#if NET_2_0
        /// <summary>
        /// The connectionStrings key from App.Config that contains the connection string.
        /// </summary>
        private string m_connectionStringName;
#endif

        /// <summary>
        /// String type name of the <see cref="IDbConnection"/> type name.
        /// </summary>
        private string m_connectionType;

        /// <summary>
        /// The text of the command.
        /// </summary>
        private string m_commandText;

        /// <summary>
        /// The command type.
        /// </summary>
        private CommandType m_commandType;

        /// <summary>
        /// Indicates whether to use transactions when writing to the database.
        /// </summary>
        private bool m_useTransactions;

        /// <summary>
        /// Indicates whether to reconnect when a connection is lost.
        /// </summary>
        private bool m_reconnectOnError;

        /// <summary>
        /// The fully qualified type of the AdoNetAppender class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private static readonly Type declaringType = typeof(AdoNetAppender);
    }

    /// <summary>
    /// Parameter type used by the <see cref="AdoNetAppender"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class provides the basic database parameter properties
    /// as defined by the <see cref="System.Data.IDbDataParameter"/> interface.
    /// </para>
    /// <para>This type can be subclassed to provide database specific
    /// functionality. The two methods that are called externally are
    /// <see cref="Prepare"/> and <see cref="FormatValue"/>.
    /// </para>
    /// </remarks>
    public class AdoNetAppenderParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdoNetAppenderParameter" /> class.
        /// </summary>
        /// <remarks>
        /// Default constructor for the AdoNetAppenderParameter class.
        /// </remarks>
        public AdoNetAppenderParameter()
        {
            this.Precision = 0;
            this.Scale = 0;
            this.Size = 0;
        }

        /// <summary>
        /// Gets or sets the name of this parameter.
        /// </summary>
        /// <value>
        /// The name of this parameter.
        /// </value>
        /// <remarks>
        /// <para>
        /// The name of this parameter. The parameter name
        /// must match up to a named parameter to the SQL stored procedure
        /// or prepared statement.
        /// </para>
        /// </remarks>
        public string ParameterName
        {
            get { return this.m_parameterName; }
            set { this.m_parameterName = value; }
        }

        /// <summary>
        /// Gets or sets the database type for this parameter.
        /// </summary>
        /// <value>
        /// The database type for this parameter.
        /// </value>
        /// <remarks>
        /// <para>
        /// The database type for this parameter. This property should
        /// be set to the database type from the <see cref="DbType"/>
        /// enumeration. See <see cref="IDataParameter.DbType"/>.
        /// </para>
        /// <para>
        /// This property is optional. If not specified the ADO.NET provider
        /// will attempt to infer the type from the value.
        /// </para>
        /// </remarks>
        /// <seealso cref="IDataParameter.DbType" />
        public DbType DbType
        {
            get { return this.m_dbType; }

            set
            {
                this.m_dbType = value;
                this.m_inferType = false;
            }
        }

        /// <summary>
        /// Gets or sets the precision for this parameter.
        /// </summary>
        /// <value>
        /// The precision for this parameter.
        /// </value>
        /// <remarks>
        /// <para>
        /// The maximum number of digits used to represent the Value.
        /// </para>
        /// <para>
        /// This property is optional. If not specified the ADO.NET provider
        /// will attempt to infer the precision from the value.
        /// </para>
        /// </remarks>
        /// <seealso cref="IDbDataParameter.Precision" />
        public byte Precision
        {
            get { return this.m_precision; }
            set { this.m_precision = value; }
        }

        /// <summary>
        /// Gets or sets the scale for this parameter.
        /// </summary>
        /// <value>
        /// The scale for this parameter.
        /// </value>
        /// <remarks>
        /// <para>
        /// The number of decimal places to which Value is resolved.
        /// </para>
        /// <para>
        /// This property is optional. If not specified the ADO.NET provider
        /// will attempt to infer the scale from the value.
        /// </para>
        /// </remarks>
        /// <seealso cref="IDbDataParameter.Scale" />
        public byte Scale
        {
            get { return this.m_scale; }
            set { this.m_scale = value; }
        }

        /// <summary>
        /// Gets or sets the size for this parameter.
        /// </summary>
        /// <value>
        /// The size for this parameter.
        /// </value>
        /// <remarks>
        /// <para>
        /// The maximum size, in bytes, of the data within the column.
        /// </para>
        /// <para>
        /// This property is optional. If not specified the ADO.NET provider
        /// will attempt to infer the size from the value.
        /// </para>
        /// <para>
        /// For BLOB data types like VARCHAR(max) it may be impossible to infer the value automatically, use -1 as the size in this case.
        /// </para>
        /// </remarks>
        /// <seealso cref="IDbDataParameter.Size" />
        public int Size
        {
            get { return this.m_size; }
            set { this.m_size = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="IRawLayout"/> to use to
        /// render the logging event into an object for this
        /// parameter.
        /// </summary>
        /// <value>
        /// The <see cref="IRawLayout"/> used to render the
        /// logging event into an object for this parameter.
        /// </value>
        /// <remarks>
        /// <para>
        /// The <see cref="IRawLayout"/> that renders the value for this
        /// parameter.
        /// </para>
        /// <para>
        /// The <see cref="RawLayoutConverter"/> can be used to adapt
        /// any <see cref="ILayout"/> into a <see cref="IRawLayout"/>
        /// for use in the property.
        /// </para>
        /// </remarks>
        public IRawLayout Layout
        {
            get { return this.m_layout; }
            set { this.m_layout = value; }
        }

        /// <summary>
        /// Prepare the specified database command object.
        /// </summary>
        /// <param name="command">The command to prepare.</param>
        /// <remarks>
        /// <para>
        /// Prepares the database command object by adding
        /// this parameter to its collection of parameters.
        /// </para>
        /// </remarks>
        public virtual void Prepare(IDbCommand command)
        {
            // Create a new parameter
            IDbDataParameter param = command.CreateParameter();

            // Set the parameter properties
            param.ParameterName = this.ParameterName;

            if (!this.m_inferType)
            {
                param.DbType = this.DbType;
            }

            if (this.Precision != 0)
            {
                param.Precision = this.Precision;
            }

            if (this.Scale != 0)
            {
                param.Scale = this.Scale;
            }

            if (this.Size != 0)
            {
                param.Size = this.Size;
            }

            // Add the parameter to the collection of params
            command.Parameters.Add(param);
        }

        /// <summary>
        /// Renders the logging event and set the parameter value in the command.
        /// </summary>
        /// <param name="command">The command containing the parameter.</param>
        /// <param name="loggingEvent">The event to be rendered.</param>
        /// <remarks>
        /// <para>
        /// Renders the logging event using this parameters layout
        /// object. Sets the value of the parameter on the command object.
        /// </para>
        /// </remarks>
        public virtual void FormatValue(IDbCommand command, LoggingEvent loggingEvent)
        {
            // Lookup the parameter
            IDbDataParameter param = (IDbDataParameter)command.Parameters[this.ParameterName];

            // Format the value
            object formattedValue = this.Layout.Format(loggingEvent);

            // If the value is null then convert to a DBNull
            if (formattedValue == null)
            {
                formattedValue = DBNull.Value;
            }

            param.Value = formattedValue;
        }

        /// <summary>
        /// The name of this parameter.
        /// </summary>
        private string m_parameterName;

        /// <summary>
        /// The database type for this parameter.
        /// </summary>
        private DbType m_dbType;

        /// <summary>
        /// Flag to infer type rather than use the DbType.
        /// </summary>
        private bool m_inferType = true;

        /// <summary>
        /// The precision for this parameter.
        /// </summary>
        private byte m_precision;

        /// <summary>
        /// The scale for this parameter.
        /// </summary>
        private byte m_scale;

        /// <summary>
        /// The size for this parameter.
        /// </summary>
        private int m_size;

        /// <summary>
        /// The <see cref="IRawLayout"/> to use to render the
        /// logging event into an object for this parameter.
        /// </summary>
        private IRawLayout m_layout;
    }
}

#endif // !SSCLI
