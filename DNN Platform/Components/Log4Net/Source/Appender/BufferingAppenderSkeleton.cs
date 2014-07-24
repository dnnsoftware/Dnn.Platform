using log4net.Core;
using log4net.Util;
using System;
using System.Collections;

namespace log4net.Appender
{
	public abstract class BufferingAppenderSkeleton : AppenderSkeleton
	{
		private const int DEFAULT_BUFFER_SIZE = 512;

		private int m_bufferSize = 512;

		private CyclicBuffer m_cb;

		private ITriggeringEventEvaluator m_evaluator;

		private bool m_lossy;

		private ITriggeringEventEvaluator m_lossyEvaluator;

		private FixFlags m_fixFlags = FixFlags.All;

		private readonly bool m_eventMustBeFixed;

		public int BufferSize
		{
			get
			{
				return this.m_bufferSize;
			}
			set
			{
				this.m_bufferSize = value;
			}
		}

		public ITriggeringEventEvaluator Evaluator
		{
			get
			{
				return this.m_evaluator;
			}
			set
			{
				this.m_evaluator = value;
			}
		}

		public virtual FixFlags Fix
		{
			get
			{
				return this.m_fixFlags;
			}
			set
			{
				this.m_fixFlags = value;
			}
		}

		public bool Lossy
		{
			get
			{
				return this.m_lossy;
			}
			set
			{
				this.m_lossy = value;
			}
		}

		public ITriggeringEventEvaluator LossyEvaluator
		{
			get
			{
				return this.m_lossyEvaluator;
			}
			set
			{
				this.m_lossyEvaluator = value;
			}
		}

		[Obsolete("Use Fix property")]
		public virtual bool OnlyFixPartialEventData
		{
			get
			{
				return this.Fix == FixFlags.Partial;
			}
			set
			{
				if (value)
				{
					this.Fix = FixFlags.Partial;
					return;
				}
				this.Fix = FixFlags.All;
			}
		}

		protected BufferingAppenderSkeleton() : this(true)
		{
		}

		protected BufferingAppenderSkeleton(bool eventMustBeFixed)
		{
			this.m_eventMustBeFixed = eventMustBeFixed;
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			if (this.m_lossy && this.m_evaluator == null)
			{
				this.ErrorHandler.Error(string.Concat("Appender [", base.Name, "] is Lossy but has no Evaluator. The buffer will never be sent!"));
			}
			if (this.m_bufferSize <= 1)
			{
				this.m_cb = null;
				return;
			}
			this.m_cb = new CyclicBuffer(this.m_bufferSize);
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			if (this.m_cb != null && this.m_bufferSize > 1)
			{
				loggingEvent.Fix = this.Fix;
				LoggingEvent loggingEvent1 = this.m_cb.Append(loggingEvent);
				if (loggingEvent1 != null)
				{
					if (!this.m_lossy)
					{
						this.SendFromBuffer(loggingEvent1, this.m_cb);
						return;
					}
					if (this.m_lossyEvaluator == null || !this.m_lossyEvaluator.IsTriggeringEvent(loggingEvent1))
					{
						loggingEvent1 = null;
					}
					if (this.m_evaluator != null && this.m_evaluator.IsTriggeringEvent(loggingEvent))
					{
						this.SendFromBuffer(loggingEvent1, this.m_cb);
						return;
					}
					if (loggingEvent1 != null)
					{
						this.SendBuffer(new LoggingEvent[] { loggingEvent1 });
						return;
					}
				}
				else if (this.m_evaluator != null && this.m_evaluator.IsTriggeringEvent(loggingEvent))
				{
					this.SendFromBuffer(null, this.m_cb);
				}
			}
			else if (!this.m_lossy || this.m_evaluator != null && this.m_evaluator.IsTriggeringEvent(loggingEvent) || this.m_lossyEvaluator != null && this.m_lossyEvaluator.IsTriggeringEvent(loggingEvent))
			{
				if (this.m_eventMustBeFixed)
				{
					loggingEvent.Fix = this.Fix;
				}
				this.SendBuffer(new LoggingEvent[] { loggingEvent });
				return;
			}
		}

		public virtual void Flush()
		{
			this.Flush(false);
		}

		public virtual void Flush(bool flushLossyBuffer)
		{
			lock (this)
			{
				if (this.m_cb != null && this.m_cb.Length > 0)
				{
					if (!this.m_lossy)
					{
						this.SendFromBuffer(null, this.m_cb);
					}
					else if (flushLossyBuffer)
					{
						if (this.m_lossyEvaluator == null)
						{
							this.m_cb.Clear();
						}
						else
						{
							LoggingEvent[] loggingEventArray = this.m_cb.PopAll();
							ArrayList arrayLists = new ArrayList((int)loggingEventArray.Length);
							LoggingEvent[] loggingEventArray1 = loggingEventArray;
							for (int i = 0; i < (int)loggingEventArray1.Length; i++)
							{
								LoggingEvent loggingEvent = loggingEventArray1[i];
								if (this.m_lossyEvaluator.IsTriggeringEvent(loggingEvent))
								{
									arrayLists.Add(loggingEvent);
								}
							}
							if (arrayLists.Count > 0)
							{
								this.SendBuffer((LoggingEvent[])arrayLists.ToArray(typeof(LoggingEvent)));
							}
						}
					}
				}
			}
		}

		protected override void OnClose()
		{
			this.Flush(true);
		}

		protected abstract void SendBuffer(LoggingEvent[] events);

		protected virtual void SendFromBuffer(LoggingEvent firstLoggingEvent, CyclicBuffer buffer)
		{
			LoggingEvent[] loggingEventArray = buffer.PopAll();
			if (firstLoggingEvent == null)
			{
				this.SendBuffer(loggingEventArray);
				return;
			}
			if ((int)loggingEventArray.Length == 0)
			{
				this.SendBuffer(new LoggingEvent[] { firstLoggingEvent });
				return;
			}
			LoggingEvent[] loggingEventArray1 = new LoggingEvent[(int)loggingEventArray.Length + 1];
			Array.Copy(loggingEventArray, 0, loggingEventArray1, 1, (int)loggingEventArray.Length);
			loggingEventArray1[0] = firstLoggingEvent;
			this.SendBuffer(loggingEventArray1);
		}
	}
}