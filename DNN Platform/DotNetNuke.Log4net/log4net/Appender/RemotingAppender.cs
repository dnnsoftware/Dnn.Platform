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

// .NET Compact Framework 1.0 has no support for System.Runtime.Remoting
#if !NETCF

using System;
using System.Collections;
using System.Threading;

using System.Runtime.Remoting.Messaging;

using log4net.Layout;
using log4net.Core;
using log4net.Util;

namespace log4net.Appender
{
	/// <summary>
	/// Delivers logging events to a remote logging sink. 
	/// </summary>
	/// <remarks>
	/// <para>
	/// This Appender is designed to deliver events to a remote sink. 
	/// That is any object that implements the <see cref="IRemoteLoggingSink"/>
	/// interface. It delivers the events using .NET remoting. The
	/// object to deliver events to is specified by setting the
	/// appenders <see cref="RemotingAppender.Sink"/> property.</para>
	/// <para>
	/// The RemotingAppender buffers events before sending them. This allows it to 
	/// make more efficient use of the remoting infrastructure.</para>
	/// <para>
	/// Once the buffer is full the events are still not sent immediately. 
	/// They are scheduled to be sent using a pool thread. The effect is that 
	/// the send occurs asynchronously. This is very important for a 
	/// number of non obvious reasons. The remoting infrastructure will 
	/// flow thread local variables (stored in the <see cref="CallContext"/>),
	/// if they are marked as <see cref="ILogicalThreadAffinative"/>, across the 
	/// remoting boundary. If the server is not contactable then
	/// the remoting infrastructure will clear the <see cref="ILogicalThreadAffinative"/>
	/// objects from the <see cref="CallContext"/>. To prevent a logging failure from
	/// having side effects on the calling application the remoting call must be made
	/// from a separate thread to the one used by the application. A <see cref="ThreadPool"/>
	/// thread is used for this. If no <see cref="ThreadPool"/> thread is available then
	/// the events will block in the thread pool manager until a thread is available.</para>
	/// <para>
	/// Because the events are sent asynchronously using pool threads it is possible to close 
	/// this appender before all the queued events have been sent.
	/// When closing the appender attempts to wait until all the queued events have been sent, but 
	/// this will timeout after 30 seconds regardless.</para>
	/// <para>
	/// If this appender is being closed because the <see cref="AppDomain.ProcessExit"/>
	/// event has fired it may not be possible to send all the queued events. During process
	/// exit the runtime limits the time that a <see cref="AppDomain.ProcessExit"/>
	/// event handler is allowed to run for. If the runtime terminates the threads before
	/// the queued events have been sent then they will be lost. To ensure that all events
	/// are sent the appender must be closed before the application exits. See 
	/// <see cref="log4net.Core.LoggerManager.Shutdown"/> for details on how to shutdown
	/// log4net programmatically.</para>
	/// </remarks>
	/// <seealso cref="IRemoteLoggingSink" />
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	/// <author>Daniel Cazzulino</author>
	public class RemotingAppender : BufferingAppenderSkeleton
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="RemotingAppender" /> class.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Default constructor.
		/// </para>
		/// </remarks>
		public RemotingAppender()
		{
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets the URL of the well-known object that will accept 
		/// the logging events.
		/// </summary>
		/// <value>
		/// The well-known URL of the remote sink.
		/// </value>
		/// <remarks>
		/// <para>
		/// The URL of the remoting sink that will accept logging events.
		/// The sink must implement the <see cref="IRemoteLoggingSink"/>
		/// interface.
		/// </para>
		/// </remarks>
		public string Sink
		{
			get { return m_sinkUrl; }
			set { m_sinkUrl = value; }
		}

		#endregion Public Instance Properties

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
#if NET_4_0
        [System.Security.SecuritySafeCritical]
#endif
        override public void ActivateOptions() 
		{
			base.ActivateOptions();

			IDictionary channelProperties = new Hashtable(); 
			channelProperties["typeFilterLevel"] = "Full";

			m_sinkObj = (IRemoteLoggingSink)Activator.GetObject(typeof(IRemoteLoggingSink), m_sinkUrl, channelProperties);
		}

		#endregion

		#region Override implementation of BufferingAppenderSkeleton

		/// <summary>
		/// Send the contents of the buffer to the remote sink.
		/// </summary>
		/// <remarks>
		/// The events are not sent immediately. They are scheduled to be sent
		/// using a pool thread. The effect is that the send occurs asynchronously.
		/// This is very important for a number of non obvious reasons. The remoting
		/// infrastructure will flow thread local variables (stored in the <see cref="CallContext"/>),
		/// if they are marked as <see cref="ILogicalThreadAffinative"/>, across the 
		/// remoting boundary. If the server is not contactable then
		/// the remoting infrastructure will clear the <see cref="ILogicalThreadAffinative"/>
		/// objects from the <see cref="CallContext"/>. To prevent a logging failure from
		/// having side effects on the calling application the remoting call must be made
		/// from a separate thread to the one used by the application. A <see cref="ThreadPool"/>
		/// thread is used for this. If no <see cref="ThreadPool"/> thread is available then
		/// the events will block in the thread pool manager until a thread is available.
		/// </remarks>
		/// <param name="events">The events to send.</param>
		override protected void SendBuffer(LoggingEvent[] events)
		{
			// Setup for an async send
			BeginAsyncSend();

			// Send the events
			if (!ThreadPool.QueueUserWorkItem(new WaitCallback(SendBufferCallback), events))
			{
				// Cancel the async send
				EndAsyncSend();

				ErrorHandler.Error("RemotingAppender ["+Name+"] failed to ThreadPool.QueueUserWorkItem logging events in SendBuffer.");
			}
		}

		/// <summary>
		/// Override base class close.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method waits while there are queued work items. The events are
		/// sent asynchronously using <see cref="ThreadPool"/> work items. These items
		/// will be sent once a thread pool thread is available to send them, therefore
		/// it is possible to close the appender before all the queued events have been
		/// sent.</para>
		/// <para>
		/// This method attempts to wait until all the queued events have been sent, but this 
		/// method will timeout after 30 seconds regardless.</para>
		/// <para>
		/// If the appender is being closed because the <see cref="AppDomain.ProcessExit"/>
		/// event has fired it may not be possible to send all the queued events. During process
		/// exit the runtime limits the time that a <see cref="AppDomain.ProcessExit"/>
		/// event handler is allowed to run for.</para>
		/// </remarks>
		override protected void OnClose()
		{
			base.OnClose();

			// Wait for the work queue to become empty before closing, timeout 30 seconds
			if (!m_workQueueEmptyEvent.WaitOne(30 * 1000, false))
			{
				ErrorHandler.Error("RemotingAppender ["+Name+"] failed to send all queued events before close, in OnClose.");
			}
		}

		#endregion

		/// <summary>
		/// A work item is being queued into the thread pool
		/// </summary>
		private void BeginAsyncSend()
		{
			// The work queue is not empty
			m_workQueueEmptyEvent.Reset();

			// Increment the queued count
			Interlocked.Increment(ref m_queuedCallbackCount);
		}

		/// <summary>
		/// A work item from the thread pool has completed
		/// </summary>
		private void EndAsyncSend()
		{
			// Decrement the queued count
			if (Interlocked.Decrement(ref m_queuedCallbackCount) <= 0)
			{
				// If the work queue is empty then set the event
				m_workQueueEmptyEvent.Set();
			}
		}

		/// <summary>
		/// Send the contents of the buffer to the remote sink.
		/// </summary>
		/// <remarks>
		/// This method is designed to be used with the <see cref="ThreadPool"/>.
		/// This method expects to be passed an array of <see cref="LoggingEvent"/>
		/// objects in the state param.
		/// </remarks>
		/// <param name="state">the logging events to send</param>
		private void SendBufferCallback(object state)
		{
			try
			{
				LoggingEvent[] events = (LoggingEvent[])state;

				// Send the events
				m_sinkObj.LogEvents(events);
			}
			catch(Exception ex)
			{
				ErrorHandler.Error("Failed in SendBufferCallback", ex);
			}
			finally
			{
				EndAsyncSend();
			}
		}

		#region Private Instance Fields

		/// <summary>
		/// The URL of the remote sink.
		/// </summary>
		private string m_sinkUrl;

		/// <summary>
		/// The local proxy (.NET remoting) for the remote logging sink.
		/// </summary>
		private IRemoteLoggingSink m_sinkObj;

		/// <summary>
		/// The number of queued callbacks currently waiting or executing
		/// </summary>
		private int m_queuedCallbackCount = 0;

		/// <summary>
		/// Event used to signal when there are no queued work items
		/// </summary>
		/// <remarks>
		/// This event is set when there are no queued work items. In this
		/// state it is safe to close the appender.
		/// </remarks>
		private ManualResetEvent m_workQueueEmptyEvent = new ManualResetEvent(true);

		#endregion Private Instance Fields

		/// <summary>
		/// Interface used to deliver <see cref="LoggingEvent"/> objects to a remote sink.
		/// </summary>
		/// <remarks>
		/// This interface must be implemented by a remoting sink
		/// if the <see cref="RemotingAppender"/> is to be used
		/// to deliver logging events to the sink.
		/// </remarks>
		public interface IRemoteLoggingSink
		{
			/// <summary>
			/// Delivers logging events to the remote sink
			/// </summary>
			/// <param name="events">Array of events to log.</param>
			/// <remarks>
			/// <para>
			/// Delivers logging events to the remote sink
			/// </para>
			/// </remarks>
			void LogEvents(LoggingEvent[] events);
		}
	}
}

#endif // !NETCF