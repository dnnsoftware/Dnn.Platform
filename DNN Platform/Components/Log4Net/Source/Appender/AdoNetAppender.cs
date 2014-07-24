using log4net.Core;
using log4net.Util;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;

namespace log4net.Appender
{
	public class AdoNetAppender : BufferingAppenderSkeleton
	{
		protected bool m_usePreparedCommand;

		protected ArrayList m_parameters;

		private log4net.Core.SecurityContext m_securityContext;

		private IDbConnection m_dbConnection;

		private IDbCommand m_dbCommand;

		private string m_connectionString;

		private string m_appSettingsKey;

		private string m_connectionStringName;

		private string m_connectionType;

		private string m_commandText;

		private System.Data.CommandType m_commandType;

		private bool m_useTransactions;

		private bool m_reconnectOnError;

		private readonly static Type declaringType;

		public string AppSettingsKey
		{
			get
			{
				return this.m_appSettingsKey;
			}
			set
			{
				this.m_appSettingsKey = value;
			}
		}

		public string CommandText
		{
			get
			{
				return this.m_commandText;
			}
			set
			{
				this.m_commandText = value;
			}
		}

		public System.Data.CommandType CommandType
		{
			get
			{
				return this.m_commandType;
			}
			set
			{
				this.m_commandType = value;
			}
		}

		protected IDbConnection Connection
		{
			get
			{
				return this.m_dbConnection;
			}
			set
			{
				this.m_dbConnection = value;
			}
		}

		public string ConnectionString
		{
			get
			{
				return this.m_connectionString;
			}
			set
			{
				this.m_connectionString = value;
			}
		}

		public string ConnectionStringName
		{
			get
			{
				return this.m_connectionStringName;
			}
			set
			{
				this.m_connectionStringName = value;
			}
		}

		public string ConnectionType
		{
			get
			{
				return this.m_connectionType;
			}
			set
			{
				this.m_connectionType = value;
			}
		}

		public bool ReconnectOnError
		{
			get
			{
				return this.m_reconnectOnError;
			}
			set
			{
				this.m_reconnectOnError = value;
			}
		}

		public log4net.Core.SecurityContext SecurityContext
		{
			get
			{
				return this.m_securityContext;
			}
			set
			{
				this.m_securityContext = value;
			}
		}

		public bool UseTransactions
		{
			get
			{
				return this.m_useTransactions;
			}
			set
			{
				this.m_useTransactions = value;
			}
		}

		static AdoNetAppender()
		{
			AdoNetAppender.declaringType = typeof(AdoNetAppender);
		}

		public AdoNetAppender()
		{
			this.m_connectionType = "System.Data.OleDb.OleDbConnection, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
			this.m_useTransactions = true;
			this.m_commandType = System.Data.CommandType.Text;
			this.m_parameters = new ArrayList();
			this.m_reconnectOnError = false;
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			this.m_usePreparedCommand = (this.m_commandText == null ? false : this.m_commandText.Length > 0);
			if (this.m_securityContext == null)
			{
				this.m_securityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
			}
			this.InitializeDatabaseConnection();
			this.InitializeDatabaseCommand();
		}

		public void AddParameter(AdoNetAppenderParameter parameter)
		{
			this.m_parameters.Add(parameter);
		}

		protected virtual IDbConnection CreateConnection(Type connectionType, string connectionString)
		{
			IDbConnection dbConnection = (IDbConnection)Activator.CreateInstance(connectionType);
			dbConnection.ConnectionString = connectionString;
			return dbConnection;
		}

		private void DiposeConnection()
		{
			if (this.m_dbConnection != null)
			{
				try
				{
					this.m_dbConnection.Close();
				}
				catch (Exception exception)
				{
					LogLog.Warn(AdoNetAppender.declaringType, "Exception while disposing cached connection object", exception);
				}
				this.m_dbConnection = null;
			}
		}

		private void DisposeCommand(bool ignoreException)
		{
			if (this.m_dbCommand != null)
			{
				try
				{
					this.m_dbCommand.Dispose();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (!ignoreException)
					{
						LogLog.Warn(AdoNetAppender.declaringType, "Exception while disposing cached command object", exception);
					}
				}
				this.m_dbCommand = null;
			}
		}

		protected virtual string GetLogStatement(LoggingEvent logEvent)
		{
			if (this.Layout == null)
			{
				this.ErrorHandler.Error("AdoNetAppender: No Layout specified.");
				return "";
			}
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			this.Layout.Format(stringWriter, logEvent);
			return stringWriter.ToString();
		}

		private void InitializeDatabaseCommand()
		{
			if (this.m_dbConnection != null && this.m_usePreparedCommand)
			{
				try
				{
					this.DisposeCommand(false);
					this.m_dbCommand = this.m_dbConnection.CreateCommand();
					this.m_dbCommand.CommandText = this.m_commandText;
					this.m_dbCommand.CommandType = this.m_commandType;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.ErrorHandler.Error(string.Concat("Could not create database command [", this.m_commandText, "]"), exception);
					this.DisposeCommand(true);
				}
				if (this.m_dbCommand != null)
				{
					try
					{
						foreach (AdoNetAppenderParameter mParameter in this.m_parameters)
						{
							try
							{
								mParameter.Prepare(this.m_dbCommand);
							}
							catch (Exception exception3)
							{
								Exception exception2 = exception3;
								this.ErrorHandler.Error(string.Concat("Could not add database command parameter [", mParameter.ParameterName, "]"), exception2);
								throw;
							}
						}
					}
					catch
					{
						this.DisposeCommand(true);
					}
				}
				if (this.m_dbCommand != null)
				{
					try
					{
						this.m_dbCommand.Prepare();
					}
					catch (Exception exception5)
					{
						Exception exception4 = exception5;
						this.ErrorHandler.Error(string.Concat("Could not prepare database command [", this.m_commandText, "]"), exception4);
						this.DisposeCommand(true);
					}
				}
			}
		}

		private void InitializeDatabaseConnection()
		{
			string str = "Unable to determine connection string context.";
			string empty = string.Empty;
			try
			{
				this.DisposeCommand(true);
				this.DiposeConnection();
				empty = this.ResolveConnectionString(out str);
				this.m_dbConnection = this.CreateConnection(this.ResolveConnectionType(), empty);
				using (IDisposable disposable = this.SecurityContext.Impersonate(this))
				{
					this.m_dbConnection.Open();
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				IErrorHandler errorHandler = this.ErrorHandler;
				string[] strArrays = new string[] { "Could not open database connection [", empty, "]. Connection string context [", str, "]." };
				errorHandler.Error(string.Concat(strArrays), exception);
				this.m_dbConnection = null;
			}
		}

		protected override void OnClose()
		{
			base.OnClose();
			this.DisposeCommand(false);
			this.DiposeConnection();
		}

		protected virtual string ResolveConnectionString(out string connectionStringContext)
		{
			if (this.m_connectionString != null && this.m_connectionString.Length > 0)
			{
				connectionStringContext = "ConnectionString";
				return this.m_connectionString;
			}
			if (!string.IsNullOrEmpty(this.m_connectionStringName))
			{
				ConnectionStringSettings item = ConfigurationManager.ConnectionStrings[this.m_connectionStringName];
				if (item == null)
				{
					throw new LogException(string.Concat("Unable to find [", this.m_connectionStringName, "] ConfigurationManager.ConnectionStrings item"));
				}
				connectionStringContext = "ConnectionStringName";
				return item.ConnectionString;
			}
			if (this.m_appSettingsKey == null || this.m_appSettingsKey.Length <= 0)
			{
				connectionStringContext = "Unable to resolve connection string from ConnectionString, ConnectionStrings, or AppSettings.";
				return string.Empty;
			}
			connectionStringContext = "AppSettingsKey";
			string appSetting = SystemInfo.GetAppSetting(this.m_appSettingsKey);
			if (appSetting == null || appSetting.Length == 0)
			{
				throw new LogException(string.Concat("Unable to find [", this.m_appSettingsKey, "] AppSettings key."));
			}
			return appSetting;
		}

		protected virtual Type ResolveConnectionType()
		{
			Type typeFromString;
			try
			{
				typeFromString = SystemInfo.GetTypeFromString(this.m_connectionType, true, false);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.ErrorHandler.Error(string.Concat("Failed to load connection type [", this.m_connectionType, "]"), exception);
				throw;
			}
			return typeFromString;
		}

		protected override void SendBuffer(LoggingEvent[] events)
		{
			if (this.m_reconnectOnError && (this.m_dbConnection == null || this.m_dbConnection.State != ConnectionState.Open))
			{
				LogLog.Debug(AdoNetAppender.declaringType, string.Concat("Attempting to reconnect to database. Current Connection State: ", (this.m_dbConnection == null ? SystemInfo.NullText : this.m_dbConnection.State.ToString())));
				this.InitializeDatabaseConnection();
				this.InitializeDatabaseCommand();
			}
			if (this.m_dbConnection != null && this.m_dbConnection.State == ConnectionState.Open)
			{
				if (!this.m_useTransactions)
				{
					this.SendBuffer(null, events);
				}
				else
				{
					IDbTransaction dbTransaction = null;
					try
					{
						dbTransaction = this.m_dbConnection.BeginTransaction();
						this.SendBuffer(dbTransaction, events);
						dbTransaction.Commit();
					}
					catch (Exception exception2)
					{
						Exception exception = exception2;
						if (dbTransaction != null)
						{
							try
							{
								dbTransaction.Rollback();
							}
							catch (Exception exception1)
							{
							}
						}
						this.ErrorHandler.Error("Exception while writing to database", exception);
					}
				}
			}
		}

		protected virtual void SendBuffer(IDbTransaction dbTran, LoggingEvent[] events)
		{
			if (!this.m_usePreparedCommand)
			{
				using (IDbCommand dbCommand = this.m_dbConnection.CreateCommand())
				{
					if (dbTran != null)
					{
						dbCommand.Transaction = dbTran;
					}
					LoggingEvent[] loggingEventArray = events;
					for (int i = 0; i < (int)loggingEventArray.Length; i++)
					{
						string logStatement = this.GetLogStatement(loggingEventArray[i]);
						LogLog.Debug(AdoNetAppender.declaringType, string.Concat("LogStatement [", logStatement, "]"));
						dbCommand.CommandText = logStatement;
						dbCommand.ExecuteNonQuery();
					}
				}
			}
			else if (this.m_dbCommand != null)
			{
				if (dbTran != null)
				{
					this.m_dbCommand.Transaction = dbTran;
				}
				LoggingEvent[] loggingEventArray1 = events;
				for (int j = 0; j < (int)loggingEventArray1.Length; j++)
				{
					LoggingEvent loggingEvent = loggingEventArray1[j];
					foreach (AdoNetAppenderParameter mParameter in this.m_parameters)
					{
						mParameter.FormatValue(this.m_dbCommand, loggingEvent);
					}
					this.m_dbCommand.ExecuteNonQuery();
				}
				return;
			}
		}
	}
}