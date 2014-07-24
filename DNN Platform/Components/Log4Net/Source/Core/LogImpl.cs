using log4net.Repository;
using log4net.Util;
using System;
using System.Globalization;

namespace log4net.Core
{
	public class LogImpl : LoggerWrapperImpl, ILog, ILoggerWrapper
	{
		private readonly static Type ThisDeclaringType;

		private Level m_levelDebug;

		private Level m_levelInfo;

		private Level m_levelWarn;

		private Level m_levelError;

		private Level m_levelFatal;

		public virtual bool IsDebugEnabled
		{
			get
			{
				return this.Logger.IsEnabledFor(this.m_levelDebug);
			}
		}

		public virtual bool IsErrorEnabled
		{
			get
			{
				return this.Logger.IsEnabledFor(this.m_levelError);
			}
		}

		public virtual bool IsFatalEnabled
		{
			get
			{
				return this.Logger.IsEnabledFor(this.m_levelFatal);
			}
		}

		public virtual bool IsInfoEnabled
		{
			get
			{
				return this.Logger.IsEnabledFor(this.m_levelInfo);
			}
		}

		public virtual bool IsWarnEnabled
		{
			get
			{
				return this.Logger.IsEnabledFor(this.m_levelWarn);
			}
		}

		static LogImpl()
		{
			LogImpl.ThisDeclaringType = typeof(LogImpl);
		}

		public LogImpl(ILogger logger) : base(logger)
		{
			logger.Repository.ConfigurationChanged += new LoggerRepositoryConfigurationChangedEventHandler(this.LoggerRepositoryConfigurationChanged);
			this.ReloadLevels(logger.Repository);
		}

		public virtual void Debug(object message)
		{
			this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelDebug, message, null);
		}

		public virtual void Debug(object message, Exception exception)
		{
			this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelDebug, message, exception);
		}

		public virtual void DebugFormat(string format, params object[] args)
		{
			if (this.IsDebugEnabled)
			{
				this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelDebug, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		public virtual void DebugFormat(string format, object arg0)
		{
			if (this.IsDebugEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelDebug = this.m_levelDebug;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0 };
				logger.Log(thisDeclaringType, mLevelDebug, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void DebugFormat(string format, object arg0, object arg1)
		{
			if (this.IsDebugEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelDebug = this.m_levelDebug;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0, arg1 };
				logger.Log(thisDeclaringType, mLevelDebug, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void DebugFormat(string format, object arg0, object arg1, object arg2)
		{
			if (this.IsDebugEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelDebug = this.m_levelDebug;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0, arg1, arg2 };
				logger.Log(thisDeclaringType, mLevelDebug, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void DebugFormat(IFormatProvider provider, string format, params object[] args)
		{
			if (this.IsDebugEnabled)
			{
				this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelDebug, new SystemStringFormat(provider, format, args), null);
			}
		}

		public virtual void Error(object message)
		{
			this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelError, message, null);
		}

		public virtual void Error(object message, Exception exception)
		{
			this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelError, message, exception);
		}

		public virtual void ErrorFormat(string format, params object[] args)
		{
			if (this.IsErrorEnabled)
			{
				this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelError, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		public virtual void ErrorFormat(string format, object arg0)
		{
			if (this.IsErrorEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelError = this.m_levelError;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0 };
				logger.Log(thisDeclaringType, mLevelError, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void ErrorFormat(string format, object arg0, object arg1)
		{
			if (this.IsErrorEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelError = this.m_levelError;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0, arg1 };
				logger.Log(thisDeclaringType, mLevelError, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void ErrorFormat(string format, object arg0, object arg1, object arg2)
		{
			if (this.IsErrorEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelError = this.m_levelError;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0, arg1, arg2 };
				logger.Log(thisDeclaringType, mLevelError, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void ErrorFormat(IFormatProvider provider, string format, params object[] args)
		{
			if (this.IsErrorEnabled)
			{
				this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelError, new SystemStringFormat(provider, format, args), null);
			}
		}

		public virtual void Fatal(object message)
		{
			this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelFatal, message, null);
		}

		public virtual void Fatal(object message, Exception exception)
		{
			this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelFatal, message, exception);
		}

		public virtual void FatalFormat(string format, params object[] args)
		{
			if (this.IsFatalEnabled)
			{
				this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelFatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		public virtual void FatalFormat(string format, object arg0)
		{
			if (this.IsFatalEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelFatal = this.m_levelFatal;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0 };
				logger.Log(thisDeclaringType, mLevelFatal, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void FatalFormat(string format, object arg0, object arg1)
		{
			if (this.IsFatalEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelFatal = this.m_levelFatal;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0, arg1 };
				logger.Log(thisDeclaringType, mLevelFatal, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void FatalFormat(string format, object arg0, object arg1, object arg2)
		{
			if (this.IsFatalEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelFatal = this.m_levelFatal;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0, arg1, arg2 };
				logger.Log(thisDeclaringType, mLevelFatal, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void FatalFormat(IFormatProvider provider, string format, params object[] args)
		{
			if (this.IsFatalEnabled)
			{
				this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelFatal, new SystemStringFormat(provider, format, args), null);
			}
		}

		public virtual void Info(object message)
		{
			this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelInfo, message, null);
		}

		public virtual void Info(object message, Exception exception)
		{
			this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelInfo, message, exception);
		}

		public virtual void InfoFormat(string format, params object[] args)
		{
			if (this.IsInfoEnabled)
			{
				this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelInfo, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		public virtual void InfoFormat(string format, object arg0)
		{
			if (this.IsInfoEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelInfo = this.m_levelInfo;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0 };
				logger.Log(thisDeclaringType, mLevelInfo, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void InfoFormat(string format, object arg0, object arg1)
		{
			if (this.IsInfoEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelInfo = this.m_levelInfo;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0, arg1 };
				logger.Log(thisDeclaringType, mLevelInfo, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void InfoFormat(string format, object arg0, object arg1, object arg2)
		{
			if (this.IsInfoEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelInfo = this.m_levelInfo;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0, arg1, arg2 };
				logger.Log(thisDeclaringType, mLevelInfo, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void InfoFormat(IFormatProvider provider, string format, params object[] args)
		{
			if (this.IsInfoEnabled)
			{
				this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelInfo, new SystemStringFormat(provider, format, args), null);
			}
		}

		private void LoggerRepositoryConfigurationChanged(object sender, EventArgs e)
		{
			ILoggerRepository loggerRepository = sender as ILoggerRepository;
			if (loggerRepository != null)
			{
				this.ReloadLevels(loggerRepository);
			}
		}

		protected virtual void ReloadLevels(ILoggerRepository repository)
		{
			LevelMap levelMap = repository.LevelMap;
			this.m_levelDebug = levelMap.LookupWithDefault(Level.Debug);
			this.m_levelInfo = levelMap.LookupWithDefault(Level.Info);
			this.m_levelWarn = levelMap.LookupWithDefault(Level.Warn);
			this.m_levelError = levelMap.LookupWithDefault(Level.Error);
			this.m_levelFatal = levelMap.LookupWithDefault(Level.Fatal);
		}

		public virtual void Warn(object message)
		{
			this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelWarn, message, null);
		}

		public virtual void Warn(object message, Exception exception)
		{
			this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelWarn, message, exception);
		}

		public virtual void WarnFormat(string format, params object[] args)
		{
			if (this.IsWarnEnabled)
			{
				this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelWarn, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		public virtual void WarnFormat(string format, object arg0)
		{
			if (this.IsWarnEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelWarn = this.m_levelWarn;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0 };
				logger.Log(thisDeclaringType, mLevelWarn, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void WarnFormat(string format, object arg0, object arg1)
		{
			if (this.IsWarnEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelWarn = this.m_levelWarn;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0, arg1 };
				logger.Log(thisDeclaringType, mLevelWarn, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void WarnFormat(string format, object arg0, object arg1, object arg2)
		{
			if (this.IsWarnEnabled)
			{
				ILogger logger = this.Logger;
				Type thisDeclaringType = LogImpl.ThisDeclaringType;
				Level mLevelWarn = this.m_levelWarn;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { arg0, arg1, arg2 };
				logger.Log(thisDeclaringType, mLevelWarn, new SystemStringFormat(invariantCulture, format, objArray), null);
			}
		}

		public virtual void WarnFormat(IFormatProvider provider, string format, params object[] args)
		{
			if (this.IsWarnEnabled)
			{
				this.Logger.Log(LogImpl.ThisDeclaringType, this.m_levelWarn, new SystemStringFormat(provider, format, args), null);
			}
		}
	}
}