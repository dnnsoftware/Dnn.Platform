#region Apache License
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
#endregion

// SSCLI 1.0 has no support for ADO.NET
#if !SSCLI

using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;

using log4net.Util;
using log4net.Layout;
using log4net.Core;

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
	/// <author>Julian Biddle</author>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	/// <author>Lance Nehring</author>
	public class AdoNetAppender : BufferingAppenderSkeleton
	{
		#region Public Instance Constructors

		/// <summary> 
		/// Initializes a new instance of the <see cref="AdoNetAppender" /> class.
		/// </summary>
		/// <remarks>
		/// Public default constructor to initialize a new instance of this class.
		/// </remarks>
		public AdoNetAppender()
		{
			m_connectionType = "System.Data.OleDb.OleDbConnection, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
			m_useTransactions = true;
			m_commandType = System.Data.CommandType.Text;
			m_parameters = new ArrayList();
			m_reconnectOnError = false;
		}

		#endregion // Public Instance Constructors

		#region Public Instance Properties

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
			get { return m_connectionString; }
			set { m_connectionString = value; }
		}

	    /// <summary>
	    /// The appSettings key from App.Config that contains the connection string.
	    /// </summary>
	    public string AppSettingsKey
	    {
	        get { return m_appSettingsKey; }
	        set { m_appSettingsKey = value; }
	    }

#if NET_2_0
	    /// <summary>
        /// The connectionStrings key from App.Config that contains the connection string.
	    /// </summary>
        /// <remarks>
        /// This property requires at least .NET 2.0.
        /// </remarks>
	    public string ConnectionStringName
	    {
	        get { return m_connectionStringName; }
	        set { m_connectionStringName = value; }
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
			get { return m_connectionType; }
			set { m_connectionType = value; }
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
		/// </remarks>
		public string CommandText
		{
			get { return m_commandText; }
			set { m_commandText = value; }
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
			get { return m_commandType; }
			set { m_commandType = value; }
		}

		/// <summary>
		/// Should transactions be used to insert logging events in the database.
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
			get { return m_useTransactions; }
			set { m_useTransactions = value; }
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
			get { return m_securityContext; }
			set { m_securityContext = value; }
		}

		/// <summary>
		/// Should this appender try to reconnect to the database on error.
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
			get { return m_reconnectOnError; }
			set { m_reconnectOnError = value; }
		}

		#endregion // Public Instance Properties

		#region Protected Instance Properties

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
			get { return m_dbConnection; }
			set { m_dbConnection = value; }
		}

		#endregion // Protected Instance Properties

		#region Implementation of IOptionHandler

		/// <summary>
		/// Initialize the appender based on the options set
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
		override public void ActivateOptions() 
		{
			base.ActivateOptions();

			// Are we using a command object
			m_usePreparedCommand = (m_commandText != null && m_commandText.Length > 0);

			if (m_securityContext == null)
			{
				m_securityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
			}

			InitializeDatabaseConnection();
			InitializeDatabaseCommand();
		}

		#endregion

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// Override the parent method to close the database
		/// </summary>
		/// <remarks>
		/// <para>
		/// Closes the database command and database connection.
		/// </para>
		/// </remarks>
		override protected void OnClose() 
		{
			base.OnClose();
            DisposeCommand(false);
            DiposeConnection();
		}

		#endregion

		#region Override implementation of BufferingAppenderSkeleton

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
		override protected void SendBuffer(LoggingEvent[] events)
		{
			if (m_reconnectOnError && (m_dbConnection == null || m_dbConnection.State != ConnectionState.Open))
			{
				LogLog.Debug(declaringType, "Attempting to reconnect to database. Current Connection State: " + ((m_dbConnection==null)?SystemInfo.NullText:m_dbConnection.State.ToString()) );

				InitializeDatabaseConnection();
				InitializeDatabaseCommand();
			}

			// Check that the connection exists and is open
			if (m_dbConnection != null && m_dbConnection.State == ConnectionState.Open)
			{
				if (m_useTransactions)
				{
					// Create transaction
					// NJC - Do this on 2 lines because it can confuse the debugger
					IDbTransaction dbTran = null;
					try
					{
						dbTran = m_dbConnection.BeginTransaction();

						SendBuffer(dbTran, events);

						// commit transaction
						dbTran.Commit();
					}
					catch(Exception ex)
					{
						// rollback the transaction
						if (dbTran != null)
						{
							try
							{
								dbTran.Rollback();
							}
							catch(Exception)
							{
								// Ignore exception
							}
						}

						// Can't insert into the database. That's a bad thing
						ErrorHandler.Error("Exception while writing to database", ex);
					}
				}
				else
				{
					// Send without transaction
					SendBuffer(null, events);
				}
			}
		}

		#endregion // Override implementation of BufferingAppenderSkeleton

		#region Public Instance Methods

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
			m_parameters.Add(parameter);
		}


		#endregion // Public Instance Methods

		#region Protected Instance Methods

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
		virtual protected void SendBuffer(IDbTransaction dbTran, LoggingEvent[] events)
		{
			if (m_usePreparedCommand) 
			{
				// Send buffer using the prepared command object

				if (m_dbCommand != null)
				{
					if (dbTran != null)
					{
						m_dbCommand.Transaction = dbTran;
					}

					// run for all events
					foreach(LoggingEvent e in events)
					{
						// Set the parameter values
						foreach(AdoNetAppenderParameter param in m_parameters)
						{
							param.FormatValue(m_dbCommand, e);
						}

						// Execute the query
						m_dbCommand.ExecuteNonQuery();
					}
				}
			}
			else
			{
				// create a new command
				using(IDbCommand dbCmd = m_dbConnection.CreateCommand())
				{
					if (dbTran != null)
					{
						dbCmd.Transaction = dbTran;
					}

					// run for all events
					foreach(LoggingEvent e in events)
					{
						// Get the command text from the Layout
						string logStatement = GetLogStatement(e);

						LogLog.Debug(declaringType, "LogStatement ["+logStatement+"]");

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
		virtual protected string GetLogStatement(LoggingEvent logEvent)
		{
			if (Layout == null)
			{
				ErrorHandler.Error("AdoNetAppender: No Layout specified.");
				return "";
			}
			else
			{
				StringWriter writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
				Layout.Format(writer, logEvent);
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
        virtual protected IDbConnection CreateConnection(Type connectionType, string connectionString)
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
        virtual protected string ResolveConnectionString(out string connectionStringContext)
        {
            if (m_connectionString != null && m_connectionString.Length > 0)
            {
                connectionStringContext = "ConnectionString";
                return m_connectionString;
            }

#if NET_2_0
            if (!String.IsNullOrEmpty(m_connectionStringName))
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[m_connectionStringName];
                if (settings != null)
                {
                    connectionStringContext = "ConnectionStringName";
                    return settings.ConnectionString;
                }
                else
                {
                    throw new LogException("Unable to find [" + m_connectionStringName + "] ConfigurationManager.ConnectionStrings item");
                }
            }
#endif

            if (m_appSettingsKey != null && m_appSettingsKey.Length > 0)
            {
                connectionStringContext = "AppSettingsKey";
                string appSettingsConnectionString = SystemInfo.GetAppSetting(m_appSettingsKey);
                if (appSettingsConnectionString == null || appSettingsConnectionString.Length == 0)
                {
                    throw new LogException("Unable to find [" + m_appSettingsKey + "] AppSettings key.");
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
		/// <returns>The <see cref="Type"/> of the ADO.NET provider</returns>
		virtual protected Type ResolveConnectionType()
		{
			try
			{
				return SystemInfo.GetTypeFromString(m_connectionType, true, false);
			}
			catch(Exception ex)
			{
				ErrorHandler.Error("Failed to load connection type ["+m_connectionType+"]", ex);
				throw;
			}
		}

		#endregion // Protected Instance Methods

        #region Private Instance Methods

        /// <summary>
        /// Prepares the database command and initialize the parameters.
        /// </summary>
        private void InitializeDatabaseCommand()
        {
            if (m_dbConnection != null && m_usePreparedCommand)
            {
                try
                {
                    DisposeCommand(false);

                    // Create the command object
                    m_dbCommand = m_dbConnection.CreateCommand();

                    // Set the command string
                    m_dbCommand.CommandText = m_commandText;

                    // Set the command type
                    m_dbCommand.CommandType = m_commandType;
                }
                catch (Exception e)
                {
                    ErrorHandler.Error("Could not create database command [" + m_commandText + "]", e);

                    DisposeCommand(true);
                }

                if (m_dbCommand != null)
                {
                    try
                    {
                        foreach (AdoNetAppenderParameter param in m_parameters)
                        {
                            try
                            {
                                param.Prepare(m_dbCommand);
                            }
                            catch (Exception e)
                            {
                                ErrorHandler.Error("Could not add database command parameter [" + param.ParameterName + "]", e);
                                throw;
                            }
                        }
                    }
                    catch
                    {
                        DisposeCommand(true);
                    }
                }

                if (m_dbCommand != null)
                {
                    try
                    {
                        // Prepare the command statement.
                        m_dbCommand.Prepare();
                    }
                    catch (Exception e)
                    {
                        ErrorHandler.Error("Could not prepare database command [" + m_commandText + "]", e);

                        DisposeCommand(true);
                    }
                }
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
                DisposeCommand(true);
                DiposeConnection();

                // Set the connection string
                resolvedConnectionString = ResolveConnectionString(out connectionStringContext);

                m_dbConnection = CreateConnection(ResolveConnectionType(), resolvedConnectionString);

                using (SecurityContext.Impersonate(this))
                {
                    // Open the database connection
                    m_dbConnection.Open();
                }
            }
            catch (Exception e)
            {
                // Sadly, your connection string is bad.
                ErrorHandler.Error("Could not open database connection [" + resolvedConnectionString + "]. Connection string context [" + connectionStringContext + "].", e);

                m_dbConnection = null;
            }
        }

        /// <summary>
        /// Cleanup the existing command.
        /// </summary>
        /// <param name="ignoreException">
        /// If true, a message will be written using LogLog.Warn if an exception is encountered when calling Dispose.
        /// </param>
        private void DisposeCommand(bool ignoreException)
        {
            // Cleanup any existing command or connection
            if (m_dbCommand != null)
            {
                try
                {
                    m_dbCommand.Dispose();
                }
                catch (Exception ex)
                {
                    if (!ignoreException)
                    {
                        LogLog.Warn(declaringType, "Exception while disposing cached command object", ex);
                    }
                }
                m_dbCommand = null;
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
            if (m_dbConnection != null)
            {
                try
                {
                    m_dbConnection.Close();
                }
                catch (Exception ex)
                {
                    LogLog.Warn(declaringType, "Exception while disposing cached connection object", ex);
                }
                m_dbConnection = null;
            }
        }

        #endregion // Private Instance Methods

        #region Protected Instance Fields

        /// <summary>
		/// Flag to indicate if we are using a command object
		/// </summary>
		/// <remarks>
		/// <para>
		/// Set to <c>true</c> when the appender is to use a prepared
		/// statement or stored procedure to insert into the database.
		/// </para>
		/// </remarks>
		protected bool m_usePreparedCommand;

		/// <summary>
		/// The list of <see cref="AdoNetAppenderParameter"/> objects.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The list of <see cref="AdoNetAppenderParameter"/> objects.
		/// </para>
		/// </remarks>
		protected ArrayList m_parameters;

		#endregion // Protected Instance Fields

		#region Private Instance Fields

		/// <summary>
		/// The security context to use for privileged calls
		/// </summary>
		private SecurityContext m_securityContext;

		/// <summary>
		/// The <see cref="IDbConnection" /> that will be used
		/// to insert logging events into a database.
		/// </summary>
		private IDbConnection m_dbConnection;

		/// <summary>
		/// The database command.
		/// </summary>
		private IDbCommand m_dbCommand;

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
		/// Indicates whether to use transactions when writing to the database.
		/// </summary>
		private bool m_reconnectOnError;

		#endregion // Private Instance Fields

        #region Private Static Fields

        /// <summary>
        /// The fully qualified type of the AdoNetAppender class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private readonly static Type declaringType = typeof(AdoNetAppender);

        #endregion Private Static Fields
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
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="AdoNetAppenderParameter" /> class.
		/// </summary>
		/// <remarks>
		/// Default constructor for the AdoNetAppenderParameter class.
		/// </remarks>
		public AdoNetAppenderParameter()
		{
			m_precision = 0;
			m_scale = 0;
			m_size = 0;
		}

		#endregion // Public Instance Constructors

		#region Public Instance Properties

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
			get { return m_parameterName; }
			set { m_parameterName = value; }
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
			get { return m_dbType; }
			set 
			{ 
				m_dbType = value; 
				m_inferType = false;
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
			get { return m_precision; } 
			set { m_precision = value; }
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
			get { return m_scale; }
			set { m_scale = value; }
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
		/// </remarks>
		/// <seealso cref="IDbDataParameter.Size" />
		public int Size 
		{
			get { return m_size; }
			set { m_size = value; }
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
			get { return m_layout; }
			set { m_layout = value; }
		}

		#endregion // Public Instance Properties

		#region Public Instance Methods

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
		virtual public void Prepare(IDbCommand command)
		{
			// Create a new parameter
			IDbDataParameter param = command.CreateParameter();

			// Set the parameter properties
			param.ParameterName = m_parameterName;

			if (!m_inferType)
			{
				param.DbType = m_dbType;
			}
			if (m_precision != 0)
			{
				param.Precision = m_precision;
			}
			if (m_scale != 0)
			{
				param.Scale = m_scale;
			}
			if (m_size != 0)
			{
				param.Size = m_size;
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
		virtual public void FormatValue(IDbCommand command, LoggingEvent loggingEvent)
		{
			// Lookup the parameter
			IDbDataParameter param = (IDbDataParameter)command.Parameters[m_parameterName];

			// Format the value
			object formattedValue = Layout.Format(loggingEvent);

			// If the value is null then convert to a DBNull
			if (formattedValue == null)
			{
				formattedValue = DBNull.Value;
			}

			param.Value = formattedValue;
		}

		#endregion // Public Instance Methods

		#region Private Instance Fields

		/// <summary>
		/// The name of this parameter.
		/// </summary>
		private string m_parameterName;

		/// <summary>
		/// The database type for this parameter.
		/// </summary>
		private DbType m_dbType;

		/// <summary>
		/// Flag to infer type rather than use the DbType
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

		#endregion // Private Instance Fields
	}
}

#endif // !SSCLI