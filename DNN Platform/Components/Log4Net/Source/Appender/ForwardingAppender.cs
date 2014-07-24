using log4net.Core;
using log4net.Util;
using System;

namespace log4net.Appender
{
	public class ForwardingAppender : AppenderSkeleton, IAppenderAttachable
	{
		private AppenderAttachedImpl m_appenderAttachedImpl;

		public virtual AppenderCollection Appenders
		{
			get
			{
				AppenderCollection appenderCollections;
				lock (this)
				{
					appenderCollections = (this.m_appenderAttachedImpl != null ? this.m_appenderAttachedImpl.Appenders : AppenderCollection.EmptyCollection);
				}
				return appenderCollections;
			}
		}

		public ForwardingAppender()
		{
		}

		public virtual void AddAppender(IAppender newAppender)
		{
			if (newAppender == null)
			{
				throw new ArgumentNullException("newAppender");
			}
			lock (this)
			{
				if (this.m_appenderAttachedImpl == null)
				{
					this.m_appenderAttachedImpl = new AppenderAttachedImpl();
				}
				this.m_appenderAttachedImpl.AddAppender(newAppender);
			}
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			if (this.m_appenderAttachedImpl != null)
			{
				this.m_appenderAttachedImpl.AppendLoopOnAppenders(loggingEvent);
			}
		}

		protected override void Append(LoggingEvent[] loggingEvents)
		{
			if (this.m_appenderAttachedImpl != null)
			{
				this.m_appenderAttachedImpl.AppendLoopOnAppenders(loggingEvents);
			}
		}

		public virtual IAppender GetAppender(string name)
		{
			IAppender appender;
			lock (this)
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
			return appender;
		}

		protected override void OnClose()
		{
			lock (this)
			{
				if (this.m_appenderAttachedImpl != null)
				{
					this.m_appenderAttachedImpl.RemoveAllAppenders();
				}
			}
		}

		public virtual void RemoveAllAppenders()
		{
			lock (this)
			{
				if (this.m_appenderAttachedImpl != null)
				{
					this.m_appenderAttachedImpl.RemoveAllAppenders();
					this.m_appenderAttachedImpl = null;
				}
			}
		}

		public virtual IAppender RemoveAppender(IAppender appender)
		{
			IAppender appender1;
			lock (this)
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
			return appender1;
		}

		public virtual IAppender RemoveAppender(string name)
		{
			IAppender appender;
			lock (this)
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
			return appender;
		}
	}
}