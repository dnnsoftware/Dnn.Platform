using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Util;
using System;
using System.Collections;
using System.Globalization;
using System.IO;

namespace log4net.Appender
{
	public abstract class AppenderSkeleton : IBulkAppender, IAppender, IOptionHandler
	{
		private const int c_renderBufferSize = 256;

		private const int c_renderBufferMaxCapacity = 1024;

		private ILayout m_layout;

		private string m_name;

		private Level m_threshold;

		private IErrorHandler m_errorHandler;

		private IFilter m_headFilter;

		private IFilter m_tailFilter;

		private bool m_closed;

		private bool m_recursiveGuard;

		private ReusableStringWriter m_renderWriter;

		private readonly static Type declaringType;

		public virtual IErrorHandler ErrorHandler
		{
			get
			{
				return this.m_errorHandler;
			}
			set
			{
				lock (this)
				{
					if (value != null)
					{
						this.m_errorHandler = value;
					}
					else
					{
						LogLog.Warn(AppenderSkeleton.declaringType, "You have tried to set a null error-handler.");
					}
				}
			}
		}

		public virtual IFilter FilterHead
		{
			get
			{
				return this.m_headFilter;
			}
		}

		public virtual ILayout Layout
		{
			get
			{
				return this.m_layout;
			}
			set
			{
				this.m_layout = value;
			}
		}

		public string Name
		{
			get
			{
				return this.m_name;
			}
			set
			{
				this.m_name = value;
			}
		}

		protected virtual bool RequiresLayout
		{
			get
			{
				return false;
			}
		}

		public Level Threshold
		{
			get
			{
				return this.m_threshold;
			}
			set
			{
				this.m_threshold = value;
			}
		}

		static AppenderSkeleton()
		{
			AppenderSkeleton.declaringType = typeof(AppenderSkeleton);
		}

		protected AppenderSkeleton()
		{
			this.m_errorHandler = new OnlyOnceErrorHandler(this.GetType().Name);
		}

		public virtual void ActivateOptions()
		{
		}

		public virtual void AddFilter(IFilter filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException("filter param must not be null");
			}
			if (this.m_headFilter != null)
			{
				this.m_tailFilter.Next = filter;
				this.m_tailFilter = filter;
				return;
			}
			IFilter filter1 = filter;
			IFilter filter2 = filter1;
			this.m_tailFilter = filter1;
			this.m_headFilter = filter2;
		}

		protected abstract void Append(LoggingEvent loggingEvent);

		protected virtual void Append(LoggingEvent[] loggingEvents)
		{
			LoggingEvent[] loggingEventArray = loggingEvents;
			for (int i = 0; i < (int)loggingEventArray.Length; i++)
			{
				this.Append(loggingEventArray[i]);
			}
		}

		public virtual void ClearFilters()
		{
			object obj = null;
			IFilter filter = (IFilter)obj;
			this.m_tailFilter = (IFilter)obj;
			this.m_headFilter = filter;
		}

		public void Close()
		{
			lock (this)
			{
				if (!this.m_closed)
				{
					this.OnClose();
					this.m_closed = true;
				}
			}
		}

		public void DoAppend(LoggingEvent loggingEvent)
		{
			lock (this)
			{
				if (this.m_closed)
				{
					this.ErrorHandler.Error(string.Concat("Attempted to append to closed appender named [", this.m_name, "]."));
				}
				else if (!this.m_recursiveGuard)
				{
					try
					{
						try
						{
							this.m_recursiveGuard = true;
							if (this.FilterEvent(loggingEvent) && this.PreAppendCheck())
							{
								this.Append(loggingEvent);
							}
						}
						catch (Exception exception)
						{
							this.ErrorHandler.Error("Failed in DoAppend", exception);
						}
					}
					finally
					{
						this.m_recursiveGuard = false;
					}
				}
			}
		}

		public void DoAppend(LoggingEvent[] loggingEvents)
		{
			lock (this)
			{
				if (this.m_closed)
				{
					this.ErrorHandler.Error(string.Concat("Attempted to append to closed appender named [", this.m_name, "]."));
				}
				else if (!this.m_recursiveGuard)
				{
					try
					{
						try
						{
							this.m_recursiveGuard = true;
							ArrayList arrayLists = new ArrayList((int)loggingEvents.Length);
							LoggingEvent[] loggingEventArray = loggingEvents;
							for (int i = 0; i < (int)loggingEventArray.Length; i++)
							{
								LoggingEvent loggingEvent = loggingEventArray[i];
								if (this.FilterEvent(loggingEvent))
								{
									arrayLists.Add(loggingEvent);
								}
							}
							if (arrayLists.Count > 0 && this.PreAppendCheck())
							{
								this.Append((LoggingEvent[])arrayLists.ToArray(typeof(LoggingEvent)));
							}
						}
						catch (Exception exception)
						{
							this.ErrorHandler.Error("Failed in Bulk DoAppend", exception);
						}
					}
					finally
					{
						this.m_recursiveGuard = false;
					}
				}
			}
		}

		protected virtual bool FilterEvent(LoggingEvent loggingEvent)
		{
			if (!this.IsAsSevereAsThreshold(loggingEvent.Level))
			{
				return false;
			}
			IFilter filterHead = this.FilterHead;
			while (filterHead != null)
			{
				switch (filterHead.Decide(loggingEvent))
				{
					case FilterDecision.Deny:
					{
						return false;
					}
					case FilterDecision.Neutral:
					{
						filterHead = filterHead.Next;
						continue;
					}
					case FilterDecision.Accept:
					{
						filterHead = null;
						continue;
					}
					default:
					{
						continue;
					}
				}
			}
			return true;
		}

		~AppenderSkeleton()
		{
			if (!this.m_closed)
			{
				LogLog.Debug(AppenderSkeleton.declaringType, string.Concat("Finalizing appender named [", this.m_name, "]."));
				this.Close();
			}
		}

		protected virtual bool IsAsSevereAsThreshold(Level level)
		{
			if (this.m_threshold == null)
			{
				return true;
			}
			return level >= this.m_threshold;
		}

		protected virtual void OnClose()
		{
		}

		protected virtual bool PreAppendCheck()
		{
			if (this.m_layout != null || !this.RequiresLayout)
			{
				return true;
			}
			this.ErrorHandler.Error(string.Concat("AppenderSkeleton: No layout set for the appender named [", this.m_name, "]."));
			return false;
		}

		protected string RenderLoggingEvent(LoggingEvent loggingEvent)
		{
			if (this.m_renderWriter == null)
			{
				this.m_renderWriter = new ReusableStringWriter(CultureInfo.InvariantCulture);
			}
			this.m_renderWriter.Reset(1024, 256);
			this.RenderLoggingEvent(this.m_renderWriter, loggingEvent);
			return this.m_renderWriter.ToString();
		}

		protected void RenderLoggingEvent(TextWriter writer, LoggingEvent loggingEvent)
		{
			if (this.m_layout == null)
			{
				throw new InvalidOperationException("A layout must be set");
			}
			if (!this.m_layout.IgnoresException)
			{
				this.m_layout.Format(writer, loggingEvent);
				return;
			}
			string exceptionString = loggingEvent.GetExceptionString();
			if (exceptionString == null || exceptionString.Length <= 0)
			{
				this.m_layout.Format(writer, loggingEvent);
				return;
			}
			this.m_layout.Format(writer, loggingEvent);
			writer.WriteLine(exceptionString);
		}
	}
}