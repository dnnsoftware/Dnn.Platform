using log4net.Appender;
using log4net.Core;
using System;

namespace log4net.Util
{
	public class AppenderAttachedImpl : IAppenderAttachable
	{
		private AppenderCollection m_appenderList;

		private IAppender[] m_appenderArray;

		private readonly static Type declaringType;

		public AppenderCollection Appenders
		{
			get
			{
				if (this.m_appenderList == null)
				{
					return AppenderCollection.EmptyCollection;
				}
				return AppenderCollection.ReadOnly(this.m_appenderList);
			}
		}

		static AppenderAttachedImpl()
		{
			AppenderAttachedImpl.declaringType = typeof(AppenderAttachedImpl);
		}

		public AppenderAttachedImpl()
		{
		}

		public void AddAppender(IAppender newAppender)
		{
			if (newAppender == null)
			{
				throw new ArgumentNullException("newAppender");
			}
			this.m_appenderArray = null;
			if (this.m_appenderList == null)
			{
				this.m_appenderList = new AppenderCollection(1);
			}
			if (!this.m_appenderList.Contains(newAppender))
			{
				this.m_appenderList.Add(newAppender);
			}
		}

		public int AppendLoopOnAppenders(LoggingEvent loggingEvent)
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}
			if (this.m_appenderList == null)
			{
				return 0;
			}
			if (this.m_appenderArray == null)
			{
				this.m_appenderArray = this.m_appenderList.ToArray();
			}
			IAppender[] mAppenderArray = this.m_appenderArray;
			for (int i = 0; i < (int)mAppenderArray.Length; i++)
			{
				IAppender appender = mAppenderArray[i];
				try
				{
					appender.DoAppend(loggingEvent);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					LogLog.Error(AppenderAttachedImpl.declaringType, string.Concat("Failed to append to appender [", appender.Name, "]"), exception);
				}
			}
			return this.m_appenderList.Count;
		}

		public int AppendLoopOnAppenders(LoggingEvent[] loggingEvents)
		{
			if (loggingEvents == null)
			{
				throw new ArgumentNullException("loggingEvents");
			}
			if ((int)loggingEvents.Length == 0)
			{
				throw new ArgumentException("loggingEvents array must not be empty", "loggingEvents");
			}
			if ((int)loggingEvents.Length == 1)
			{
				return this.AppendLoopOnAppenders(loggingEvents[0]);
			}
			if (this.m_appenderList == null)
			{
				return 0;
			}
			if (this.m_appenderArray == null)
			{
				this.m_appenderArray = this.m_appenderList.ToArray();
			}
			IAppender[] mAppenderArray = this.m_appenderArray;
			for (int i = 0; i < (int)mAppenderArray.Length; i++)
			{
				IAppender appender = mAppenderArray[i];
				try
				{
					AppenderAttachedImpl.CallAppend(appender, loggingEvents);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					LogLog.Error(AppenderAttachedImpl.declaringType, string.Concat("Failed to append to appender [", appender.Name, "]"), exception);
				}
			}
			return this.m_appenderList.Count;
		}

		private static void CallAppend(IAppender appender, LoggingEvent[] loggingEvents)
		{
			IBulkAppender bulkAppender = appender as IBulkAppender;
			if (bulkAppender != null)
			{
				bulkAppender.DoAppend(loggingEvents);
				return;
			}
			LoggingEvent[] loggingEventArray = loggingEvents;
			for (int i = 0; i < (int)loggingEventArray.Length; i++)
			{
				appender.DoAppend(loggingEventArray[i]);
			}
		}

		public IAppender GetAppender(string name)
		{
			IAppender appender;
			if (this.m_appenderList != null && name != null)
			{
				AppenderCollection.IAppenderCollectionEnumerator enumerator = this.m_appenderList.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						IAppender current = enumerator.Current;
						if (name != current.Name)
						{
							continue;
						}
						appender = current;
						return appender;
					}
					return null;
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				return appender;
			}
			return null;
		}

		public void RemoveAllAppenders()
		{
			if (this.m_appenderList != null)
			{
				foreach (IAppender mAppenderList in this.m_appenderList)
				{
					try
					{
						mAppenderList.Close();
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						LogLog.Error(AppenderAttachedImpl.declaringType, string.Concat("Failed to Close appender [", mAppenderList.Name, "]"), exception);
					}
				}
				this.m_appenderList = null;
				this.m_appenderArray = null;
			}
		}

		public IAppender RemoveAppender(IAppender appender)
		{
			if (appender != null && this.m_appenderList != null)
			{
				this.m_appenderList.Remove(appender);
				if (this.m_appenderList.Count == 0)
				{
					this.m_appenderList = null;
				}
				this.m_appenderArray = null;
			}
			return appender;
		}

		public IAppender RemoveAppender(string name)
		{
			return this.RemoveAppender(this.GetAppender(name));
		}
	}
}