using log4net.Core;
using System;
using System.Collections;
using System.Threading;

namespace log4net.Appender
{
	public class RemotingAppender : BufferingAppenderSkeleton
	{
		private string m_sinkUrl;

		private RemotingAppender.IRemoteLoggingSink m_sinkObj;

		private int m_queuedCallbackCount;

		private ManualResetEvent m_workQueueEmptyEvent = new ManualResetEvent(true);

		public string Sink
		{
			get
			{
				return this.m_sinkUrl;
			}
			set
			{
				this.m_sinkUrl = value;
			}
		}

		public RemotingAppender()
		{
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			IDictionary hashtables = new Hashtable();
			hashtables["typeFilterLevel"] = "Full";
			this.m_sinkObj = (RemotingAppender.IRemoteLoggingSink)Activator.GetObject(typeof(RemotingAppender.IRemoteLoggingSink), this.m_sinkUrl, hashtables);
		}

		private void BeginAsyncSend()
		{
			this.m_workQueueEmptyEvent.Reset();
			Interlocked.Increment(ref this.m_queuedCallbackCount);
		}

		private void EndAsyncSend()
		{
			if (Interlocked.Decrement(ref this.m_queuedCallbackCount) <= 0)
			{
				this.m_workQueueEmptyEvent.Set();
			}
		}

		protected override void OnClose()
		{
			base.OnClose();
			if (!this.m_workQueueEmptyEvent.WaitOne(30000, false))
			{
				this.ErrorHandler.Error(string.Concat("RemotingAppender [", base.Name, "] failed to send all queued events before close, in OnClose."));
			}
		}

		protected override void SendBuffer(LoggingEvent[] events)
		{
			this.BeginAsyncSend();
			if (!ThreadPool.QueueUserWorkItem(new WaitCallback(this.SendBufferCallback), events))
			{
				this.EndAsyncSend();
				this.ErrorHandler.Error(string.Concat("RemotingAppender [", base.Name, "] failed to ThreadPool.QueueUserWorkItem logging events in SendBuffer."));
			}
		}

		private void SendBufferCallback(object state)
		{
			try
			{
				try
				{
					LoggingEvent[] loggingEventArray = (LoggingEvent[])state;
					this.m_sinkObj.LogEvents(loggingEventArray);
				}
				catch (Exception exception)
				{
					this.ErrorHandler.Error("Failed in SendBufferCallback", exception);
				}
			}
			finally
			{
				this.EndAsyncSend();
			}
		}

		public interface IRemoteLoggingSink
		{
			void LogEvents(LoggingEvent[] events);
		}
	}
}