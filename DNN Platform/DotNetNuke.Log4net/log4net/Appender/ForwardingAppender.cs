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

using System;

using log4net.Util;
using log4net.Layout;
using log4net.Core;

namespace log4net.Appender
{
	/// <summary>
	/// This appender forwards logging events to attached appenders.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The forwarding appender can be used to specify different thresholds
	/// and filters for the same appender at different locations within the hierarchy.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class ForwardingAppender : AppenderSkeleton, IAppenderAttachable
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ForwardingAppender" /> class.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Default constructor.
		/// </para>
		/// </remarks>
		public ForwardingAppender()
		{
		}

		#endregion Public Instance Constructors

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// Closes the appender and releases resources.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Releases any resources allocated within the appender such as file handles, 
		/// network connections, etc.
		/// </para>
		/// <para>
		/// It is a programming error to append to a closed appender.
		/// </para>
		/// </remarks>
		override protected void OnClose()
		{
			// Remove all the attached appenders
			lock(this)
			{
				if (m_appenderAttachedImpl != null)
				{
					m_appenderAttachedImpl.RemoveAllAppenders();
				}
			}
		}

		/// <summary>
		/// Forward the logging event to the attached appenders 
		/// </summary>
		/// <param name="loggingEvent">The event to log.</param>
		/// <remarks>
		/// <para>
		/// Delivers the logging event to all the attached appenders.
		/// </para>
		/// </remarks>
		override protected void Append(LoggingEvent loggingEvent) 
		{
			// Pass the logging event on the the attached appenders
			if (m_appenderAttachedImpl != null)
			{
				m_appenderAttachedImpl.AppendLoopOnAppenders(loggingEvent);
			}
		} 

		/// <summary>
		/// Forward the logging events to the attached appenders 
		/// </summary>
		/// <param name="loggingEvents">The array of events to log.</param>
		/// <remarks>
		/// <para>
		/// Delivers the logging events to all the attached appenders.
		/// </para>
		/// </remarks>
		override protected void Append(LoggingEvent[] loggingEvents) 
		{
			// Pass the logging event on the the attached appenders
			if (m_appenderAttachedImpl != null)
			{
				m_appenderAttachedImpl.AppendLoopOnAppenders(loggingEvents);
			}
		} 

		#endregion Override implementation of AppenderSkeleton

		#region Implementation of IAppenderAttachable

		/// <summary>
		/// Adds an <see cref="IAppender" /> to the list of appenders of this
		/// instance.
		/// </summary>
		/// <param name="newAppender">The <see cref="IAppender" /> to add to this appender.</param>
		/// <remarks>
		/// <para>
		/// If the specified <see cref="IAppender" /> is already in the list of
		/// appenders, then it won't be added again.
		/// </para>
		/// </remarks>
		virtual public void AddAppender(IAppender newAppender) 
		{
			if (newAppender == null)
			{
				throw new ArgumentNullException("newAppender");
			}
			lock(this)
			{
				if (m_appenderAttachedImpl == null) 
				{
					m_appenderAttachedImpl = new log4net.Util.AppenderAttachedImpl();
				}
				m_appenderAttachedImpl.AddAppender(newAppender);
			}
		}

		/// <summary>
		/// Gets the appenders contained in this appender as an 
		/// <see cref="System.Collections.ICollection"/>.
		/// </summary>
		/// <remarks>
		/// If no appenders can be found, then an <see cref="EmptyCollection"/> 
		/// is returned.
		/// </remarks>
		/// <returns>
		/// A collection of the appenders in this appender.
		/// </returns>
		virtual public AppenderCollection Appenders 
		{
			get
			{
				lock(this)
				{
					if (m_appenderAttachedImpl == null)
					{
						return AppenderCollection.EmptyCollection;
					}
					else 
					{
						return m_appenderAttachedImpl.Appenders;
					}
				}
			}
		}

		/// <summary>
		/// Looks for the appender with the specified name.
		/// </summary>
		/// <param name="name">The name of the appender to lookup.</param>
		/// <returns>
		/// The appender with the specified name, or <c>null</c>.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Get the named appender attached to this appender.
		/// </para>
		/// </remarks>
		virtual public IAppender GetAppender(string name) 
		{
			lock(this)
			{
				if (m_appenderAttachedImpl == null || name == null)
				{
					return null;
				}

				return m_appenderAttachedImpl.GetAppender(name);
			}
		}

		/// <summary>
		/// Removes all previously added appenders from this appender.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is useful when re-reading configuration information.
		/// </para>
		/// </remarks>
		virtual public void RemoveAllAppenders() 
		{
			lock(this)
			{
				if (m_appenderAttachedImpl != null) 
				{
					m_appenderAttachedImpl.RemoveAllAppenders();
					m_appenderAttachedImpl = null;
				}
			}
		}

		/// <summary>
		/// Removes the specified appender from the list of appenders.
		/// </summary>
		/// <param name="appender">The appender to remove.</param>
		/// <returns>The appender removed from the list</returns>
		/// <remarks>
		/// The appender removed is not closed.
		/// If you are discarding the appender you must call
		/// <see cref="IAppender.Close"/> on the appender removed.
		/// </remarks>
		virtual public IAppender RemoveAppender(IAppender appender) 
		{
			lock(this)
			{
				if (appender != null && m_appenderAttachedImpl != null) 
				{
					return m_appenderAttachedImpl.RemoveAppender(appender);
				}
			}
			return null;
		}

		/// <summary>
		/// Removes the appender with the specified name from the list of appenders.
		/// </summary>
		/// <param name="name">The name of the appender to remove.</param>
		/// <returns>The appender removed from the list</returns>
		/// <remarks>
		/// The appender removed is not closed.
		/// If you are discarding the appender you must call
		/// <see cref="IAppender.Close"/> on the appender removed.
		/// </remarks>
		virtual public IAppender RemoveAppender(string name) 
		{
			lock(this)
			{
				if (name != null && m_appenderAttachedImpl != null)
				{
					return m_appenderAttachedImpl.RemoveAppender(name);
				}
			}
			return null;
		}
  
		#endregion Implementation of IAppenderAttachable

		#region Private Instance Fields

		/// <summary>
		/// Implementation of the <see cref="IAppenderAttachable"/> interface
		/// </summary>
		private AppenderAttachedImpl m_appenderAttachedImpl;

		#endregion Private Instance Fields
	}
}
