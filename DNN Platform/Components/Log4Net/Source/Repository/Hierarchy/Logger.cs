using log4net.Appender;
using log4net.Core;
using log4net.Util;
using System;
using System.Security;

namespace log4net.Repository.Hierarchy
{
	public abstract class Logger : IAppenderAttachable, ILogger
	{
		private readonly static Type declaringType;

		private readonly string m_name;

		private log4net.Core.Level m_level;

		private Logger m_parent;

		private log4net.Repository.Hierarchy.Hierarchy m_hierarchy;

		private AppenderAttachedImpl m_appenderAttachedImpl;

		private bool m_additive = true;

		private readonly ReaderWriterLock m_appenderLock = new ReaderWriterLock();

		public virtual bool Additivity
		{
			get
			{
				return this.m_additive;
			}
			set
			{
				this.m_additive = value;
			}
		}

		public virtual AppenderCollection Appenders
		{
			get
			{
				AppenderCollection appenderCollections;
				this.m_appenderLock.AcquireReaderLock();
				try
				{
					appenderCollections = (this.m_appenderAttachedImpl != null ? this.m_appenderAttachedImpl.Appenders : AppenderCollection.EmptyCollection);
				}
				finally
				{
					this.m_appenderLock.ReleaseReaderLock();
				}
				return appenderCollections;
			}
		}

		public virtual log4net.Core.Level EffectiveLevel
		{
			get
			{
				for (Logger i = this; i != null; i = i.m_parent)
				{
					log4net.Core.Level mLevel = i.m_level;
					if (mLevel != null)
					{
						return mLevel;
					}
				}
				return null;
			}
		}

		public virtual log4net.Repository.Hierarchy.Hierarchy Hierarchy
		{
			get
			{
				return this.m_hierarchy;
			}
			set
			{
				this.m_hierarchy = value;
			}
		}

		public virtual log4net.Core.Level Level
		{
			get
			{
				return this.m_level;
			}
			set
			{
				this.m_level = value;
			}
		}

		public virtual string Name
		{
			get
			{
				return this.m_name;
			}
		}

		public virtual Logger Parent
		{
			get
			{
				return this.m_parent;
			}
			set
			{
				this.m_parent = value;
			}
		}

		public ILoggerRepository Repository
		{
			get
			{
				return this.m_hierarchy;
			}
		}

		static Logger()
		{
			Logger.declaringType = typeof(Logger);
		}

		protected Logger(string name)
		{
			this.m_name = string.Intern(name);
		}

		public virtual void AddAppender(IAppender newAppender)
		{
			if (newAppender == null)
			{
				throw new ArgumentNullException("newAppender");
			}
			this.m_appenderLock.AcquireWriterLock();
			try
			{
				if (this.m_appenderAttachedImpl == null)
				{
					this.m_appenderAttachedImpl = new AppenderAttachedImpl();
				}
				this.m_appenderAttachedImpl.AddAppender(newAppender);
			}
			finally
			{
				this.m_appenderLock.ReleaseWriterLock();
			}
		}

		protected virtual void CallAppenders(LoggingEvent loggingEvent)
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}
			int num = 0;
			for (Logger i = this; i != null; i = i.m_parent)
			{
				if (i.m_appenderAttachedImpl != null)
				{
					i.m_appenderLock.AcquireReaderLock();
					try
					{
						if (i.m_appenderAttachedImpl != null)
						{
							num = num + i.m_appenderAttachedImpl.AppendLoopOnAppenders(loggingEvent);
						}
					}
					finally
					{
						i.m_appenderLock.ReleaseReaderLock();
					}
				}
				if (!i.m_additive)
				{
					break;
				}
			}
			if (!this.m_hierarchy.EmittedNoAppenderWarning && num == 0)
			{
				Type type = Logger.declaringType;
				string[] name = new string[] { "No appenders could be found for logger [", this.Name, "] repository [", this.Repository.Name, "]" };
				LogLog.Debug(type, string.Concat(name));
				LogLog.Debug(Logger.declaringType, "Please initialize the log4net system properly.");
				try
				{
					LogLog.Debug(Logger.declaringType, "    Current AppDomain context information: ");
					LogLog.Debug(Logger.declaringType, string.Concat("       BaseDirectory   : ", SystemInfo.ApplicationBaseDirectory));
					LogLog.Debug(Logger.declaringType, string.Concat("       FriendlyName    : ", AppDomain.CurrentDomain.FriendlyName));
					LogLog.Debug(Logger.declaringType, string.Concat("       DynamicDirectory: ", AppDomain.CurrentDomain.DynamicDirectory));
				}
				catch (SecurityException securityException)
				{
				}
				this.m_hierarchy.EmittedNoAppenderWarning = true;
			}
		}

		public virtual void CloseNestedAppenders()
		{
			this.m_appenderLock.AcquireWriterLock();
			try
			{
				if (this.m_appenderAttachedImpl != null)
				{
					foreach (IAppender appender in this.m_appenderAttachedImpl.Appenders)
					{
						if (!(appender is IAppenderAttachable))
						{
							continue;
						}
						appender.Close();
					}
				}
			}
			finally
			{
				this.m_appenderLock.ReleaseWriterLock();
			}
		}

		protected virtual void ForcedLog(Type callerStackBoundaryDeclaringType, log4net.Core.Level level, object message, Exception exception)
		{
			this.CallAppenders(new LoggingEvent(callerStackBoundaryDeclaringType, this.Hierarchy, this.Name, level, message, exception));
		}

		protected virtual void ForcedLog(LoggingEvent logEvent)
		{
			logEvent.EnsureRepository(this.Hierarchy);
			this.CallAppenders(logEvent);
		}

		public virtual IAppender GetAppender(string name)
		{
			IAppender appender;
			this.m_appenderLock.AcquireReaderLock();
			try
			{
				if (this.m_appenderAttachedImpl == null || name == null)
				{
					appender = null;
				}
				else
				{
					appender = this.m_appenderAttachedImpl.GetAppender(name);
				}
			}
			finally
			{
				this.m_appenderLock.ReleaseReaderLock();
			}
			return appender;
		}

		public virtual bool IsEnabledFor(log4net.Core.Level level)
		{
			bool flag;
			try
			{
				if (level != null)
				{
					if (!this.m_hierarchy.IsDisabled(level))
					{
						flag = level >= this.EffectiveLevel;
						return flag;
					}
					else
					{
						flag = false;
						return flag;
					}
				}
			}
			catch (Exception exception)
			{
				LogLog.Error(Logger.declaringType, "Exception while logging", exception);
			}
			catch
			{
				LogLog.Error(Logger.declaringType, "Exception while logging");
			}
			return false;
		}

		public virtual void Log(Type callerStackBoundaryDeclaringType, log4net.Core.Level level, object message, Exception exception)
		{
			try
			{
				if (this.IsEnabledFor(level))
				{
					this.ForcedLog((callerStackBoundaryDeclaringType != null ? callerStackBoundaryDeclaringType : Logger.declaringType), level, message, exception);
				}
			}
			catch (Exception exception1)
			{
				LogLog.Error(Logger.declaringType, "Exception while logging", exception1);
			}
			catch
			{
				LogLog.Error(Logger.declaringType, "Exception while logging");
			}
		}

		public virtual void Log(LoggingEvent logEvent)
		{
			try
			{
				if (logEvent != null && this.IsEnabledFor(logEvent.Level))
				{
					this.ForcedLog(logEvent);
				}
			}
			catch (Exception exception)
			{
				LogLog.Error(Logger.declaringType, "Exception while logging", exception);
			}
			catch
			{
				LogLog.Error(Logger.declaringType, "Exception while logging");
			}
		}

		public virtual void Log(log4net.Core.Level level, object message, Exception exception)
		{
			if (this.IsEnabledFor(level))
			{
				this.ForcedLog(Logger.declaringType, level, message, exception);
			}
		}

		public virtual void RemoveAllAppenders()
		{
			this.m_appenderLock.AcquireWriterLock();
			try
			{
				if (this.m_appenderAttachedImpl != null)
				{
					this.m_appenderAttachedImpl.RemoveAllAppenders();
					this.m_appenderAttachedImpl = null;
				}
			}
			finally
			{
				this.m_appenderLock.ReleaseWriterLock();
			}
		}

		public virtual IAppender RemoveAppender(IAppender appender)
		{
			IAppender appender1;
			this.m_appenderLock.AcquireWriterLock();
			try
			{
				if (appender == null || this.m_appenderAttachedImpl == null)
				{
					return null;
				}
				else
				{
					appender1 = this.m_appenderAttachedImpl.RemoveAppender(appender);
				}
			}
			finally
			{
				this.m_appenderLock.ReleaseWriterLock();
			}
			return appender1;
		}

		public virtual IAppender RemoveAppender(string name)
		{
			IAppender appender;
			this.m_appenderLock.AcquireWriterLock();
			try
			{
				if (name == null || this.m_appenderAttachedImpl == null)
				{
					return null;
				}
				else
				{
					appender = this.m_appenderAttachedImpl.RemoveAppender(name);
				}
			}
			finally
			{
				this.m_appenderLock.ReleaseWriterLock();
			}
			return appender;
		}
	}
}