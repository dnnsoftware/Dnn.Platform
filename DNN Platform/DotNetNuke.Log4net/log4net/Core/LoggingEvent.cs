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
using System.Collections;
using System.IO;
#if (!NETCF)
using System.Runtime.Serialization;
using System.Security.Principal;
#endif

using log4net.Util;
using log4net.Repository;

namespace log4net.Core
{
	/// <summary>
	/// Portable data structure used by <see cref="LoggingEvent"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Portable data structure used by <see cref="LoggingEvent"/>
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public struct LoggingEventData
	{
		#region Public Instance Fields

		/// <summary>
		/// The logger name.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The logger name.
		/// </para>
		/// </remarks>
		public string LoggerName;

		/// <summary>
		/// Level of logging event.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Level of logging event. Level cannot be Serializable
		/// because it is a flyweight.  Due to its special serialization it
		/// cannot be declared final either.
		/// </para>
		/// </remarks>
		public Level Level;

		/// <summary>
		/// The application supplied message.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The application supplied message of logging event.
		/// </para>
		/// </remarks>
		public string Message;

		/// <summary>
		/// The name of thread
		/// </summary>
		/// <remarks>
		/// <para>
		/// The name of thread in which this logging event was generated
		/// </para>
		/// </remarks>
		public string ThreadName;

		/// <summary>
		/// The time the event was logged
		/// </summary>
		/// <remarks>
		/// <para>
		/// The TimeStamp is stored in the local time zone for this computer.
		/// </para>
		/// </remarks>
		public DateTime TimeStamp;

		/// <summary>
		/// Location information for the caller.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Location information for the caller.
		/// </para>
		/// </remarks>
		public LocationInfo LocationInfo;

		/// <summary>
		/// String representation of the user
		/// </summary>
		/// <remarks>
		/// <para>
		/// String representation of the user's windows name,
		/// like DOMAIN\username
		/// </para>
		/// </remarks>
		public string UserName;

		/// <summary>
		/// String representation of the identity.
		/// </summary>
		/// <remarks>
		/// <para>
		/// String representation of the current thread's principal identity.
		/// </para>
		/// </remarks>
		public string Identity;

		/// <summary>
		/// The string representation of the exception
		/// </summary>
		/// <remarks>
		/// <para>
		/// The string representation of the exception
		/// </para>
		/// </remarks>
		public string ExceptionString;

		/// <summary>
		/// String representation of the AppDomain.
		/// </summary>
		/// <remarks>
		/// <para>
		/// String representation of the AppDomain.
		/// </para>
		/// </remarks>
		public string Domain;

		/// <summary>
		/// Additional event specific properties
		/// </summary>
		/// <remarks>
		/// <para>
		/// A logger or an appender may attach additional
		/// properties to specific events. These properties
		/// have a string key and an object value.
		/// </para>
		/// </remarks>
		public PropertiesDictionary Properties;

		#endregion Public Instance Fields
	}

	/// <summary>
	/// Flags passed to the <see cref="LoggingEvent.Fix"/> property
	/// </summary>
	/// <remarks>
	/// <para>
	/// Flags passed to the <see cref="LoggingEvent.Fix"/> property
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	[Flags] public enum FixFlags
	{
		/// <summary>
		/// Fix the MDC
		/// </summary>
		[Obsolete("Replaced by composite Properties")]
		Mdc = 0x01,

		/// <summary>
		/// Fix the NDC
		/// </summary>
		Ndc = 0x02,

		/// <summary>
		/// Fix the rendered message
		/// </summary>
		Message = 0x04,

		/// <summary>
		/// Fix the thread name
		/// </summary>
		ThreadName = 0x08,

		/// <summary>
		/// Fix the callers location information
		/// </summary>
		/// <remarks>
		/// CAUTION: Very slow to generate
		/// </remarks>
		LocationInfo = 0x10,

		/// <summary>
		/// Fix the callers windows user name
		/// </summary>
		/// <remarks>
		/// CAUTION: Slow to generate
		/// </remarks>
		UserName = 0x20,

		/// <summary>
		/// Fix the domain friendly name
		/// </summary>
		Domain = 0x40,

		/// <summary>
		/// Fix the callers principal name
		/// </summary>
		/// <remarks>
		/// CAUTION: May be slow to generate
		/// </remarks>
		Identity = 0x80,

		/// <summary>
		/// Fix the exception text
		/// </summary>
		Exception = 0x100,

		/// <summary>
		/// Fix the event properties. Active properties must implement <see cref="IFixingRequired"/> in order to be eligible for fixing.
		/// </summary>
		Properties = 0x200,

		/// <summary>
		/// No fields fixed
		/// </summary>
		None = 0x0,

		/// <summary>
		/// All fields fixed
		/// </summary>
		All = 0xFFFFFFF,

		/// <summary>
		/// Partial fields fixed
		/// </summary>
		/// <remarks>
		/// <para>
		/// This set of partial fields gives good performance. The following fields are fixed:
		/// </para>
		/// <list type="bullet">
		/// <item><description><see cref="Message"/></description></item>
		/// <item><description><see cref="ThreadName"/></description></item>
		/// <item><description><see cref="Exception"/></description></item>
		/// <item><description><see cref="Domain"/></description></item>
		/// <item><description><see cref="Properties"/></description></item>
		/// </list>
		/// </remarks>
		Partial = Message | ThreadName | Exception | Domain | Properties,
	}

	/// <summary>
	/// The internal representation of logging events. 
	/// </summary>
	/// <remarks>
	/// <para>
	/// When an affirmative decision is made to log then a 
	/// <see cref="LoggingEvent"/> instance is created. This instance 
	/// is passed around to the different log4net components.
	/// </para>
	/// <para>
	/// This class is of concern to those wishing to extend log4net.
	/// </para>
	/// <para>
	/// Some of the values in instances of <see cref="LoggingEvent"/>
	/// are considered volatile, that is the values are correct at the
	/// time the event is delivered to appenders, but will not be consistent
	/// at any time afterwards. If an event is to be stored and then processed
	/// at a later time these volatile values must be fixed by calling
	/// <see cref="FixVolatileData()"/>. There is a performance penalty
	/// for incurred by calling <see cref="FixVolatileData()"/> but it
	/// is essential to maintaining data consistency.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	/// <author>Douglas de la Torre</author>
	/// <author>Daniel Cazzulino</author>
#if !NETCF
	[Serializable]
#endif
	public class LoggingEvent 
#if !NETCF
		: ISerializable
#endif
	{
	    private readonly static Type declaringType = typeof(LoggingEvent);

		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingEvent" /> class
		/// from the supplied parameters.
		/// </summary>
		/// <param name="callerStackBoundaryDeclaringType">The declaring type of the method that is
		/// the stack boundary into the logging system for this call.</param>
		/// <param name="repository">The repository this event is logged in.</param>
		/// <param name="loggerName">The name of the logger of this event.</param>
		/// <param name="level">The level of this event.</param>
		/// <param name="message">The message of this event.</param>
		/// <param name="exception">The exception for this event.</param>
		/// <remarks>
		/// <para>
		/// Except <see cref="TimeStamp"/>, <see cref="Level"/> and <see cref="LoggerName"/>, 
		/// all fields of <c>LoggingEvent</c> are filled when actually needed. Call
		/// <see cref="FixVolatileData()"/> to cache all data locally
		/// to prevent inconsistencies.
		/// </para>
		/// <para>This method is called by the log4net framework
		/// to create a logging event.
		/// </para>
		/// </remarks>
		public LoggingEvent(Type callerStackBoundaryDeclaringType, log4net.Repository.ILoggerRepository repository, string loggerName, Level level, object message, Exception exception) 
		{
			m_callerStackBoundaryDeclaringType = callerStackBoundaryDeclaringType;
			m_message = message;
			m_repository = repository;
			m_thrownException = exception;

			m_data.LoggerName = loggerName;
			m_data.Level = level;

			// Store the event creation time
			m_data.TimeStamp = DateTime.Now;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingEvent" /> class 
		/// using specific data.
		/// </summary>
		/// <param name="callerStackBoundaryDeclaringType">The declaring type of the method that is
		/// the stack boundary into the logging system for this call.</param>
		/// <param name="repository">The repository this event is logged in.</param>
		/// <param name="data">Data used to initialize the logging event.</param>
		/// <param name="fixedData">The fields in the <paranref name="data"/> struct that have already been fixed.</param>
		/// <remarks>
		/// <para>
		/// This constructor is provided to allow a <see cref="LoggingEvent" />
		/// to be created independently of the log4net framework. This can
		/// be useful if you require a custom serialization scheme.
		/// </para>
		/// <para>
		/// Use the <see cref="GetLoggingEventData(FixFlags)"/> method to obtain an 
		/// instance of the <see cref="LoggingEventData"/> class.
		/// </para>
		/// <para>
		/// The <paramref name="fixedData"/> parameter should be used to specify which fields in the
		/// <paramref name="data"/> struct have been preset. Fields not specified in the <paramref name="fixedData"/>
		/// will be captured from the environment if requested or fixed.
		/// </para>
		/// </remarks>
		public LoggingEvent(Type callerStackBoundaryDeclaringType, log4net.Repository.ILoggerRepository repository, LoggingEventData data, FixFlags fixedData) 
		{
			m_callerStackBoundaryDeclaringType = callerStackBoundaryDeclaringType;
			m_repository = repository;

			m_data = data;
			m_fixFlags = fixedData;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingEvent" /> class 
		/// using specific data.
		/// </summary>
		/// <param name="callerStackBoundaryDeclaringType">The declaring type of the method that is
		/// the stack boundary into the logging system for this call.</param>
		/// <param name="repository">The repository this event is logged in.</param>
		/// <param name="data">Data used to initialize the logging event.</param>
		/// <remarks>
		/// <para>
		/// This constructor is provided to allow a <see cref="LoggingEvent" />
		/// to be created independently of the log4net framework. This can
		/// be useful if you require a custom serialization scheme.
		/// </para>
		/// <para>
		/// Use the <see cref="GetLoggingEventData(FixFlags)"/> method to obtain an 
		/// instance of the <see cref="LoggingEventData"/> class.
		/// </para>
		/// <para>
		/// This constructor sets this objects <see cref="Fix"/> flags to <see cref="FixFlags.All"/>,
		/// this assumes that all the data relating to this event is passed in via the <paramref name="data"/>
		/// parameter and no other data should be captured from the environment.
		/// </para>
		/// </remarks>
		public LoggingEvent(Type callerStackBoundaryDeclaringType, log4net.Repository.ILoggerRepository repository, LoggingEventData data) : this(callerStackBoundaryDeclaringType, repository, data, FixFlags.All)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingEvent" /> class 
		/// using specific data.
		/// </summary>
		/// <param name="data">Data used to initialize the logging event.</param>
		/// <remarks>
		/// <para>
		/// This constructor is provided to allow a <see cref="LoggingEvent" />
		/// to be created independently of the log4net framework. This can
		/// be useful if you require a custom serialization scheme.
		/// </para>
		/// <para>
		/// Use the <see cref="GetLoggingEventData(FixFlags)"/> method to obtain an 
		/// instance of the <see cref="LoggingEventData"/> class.
		/// </para>
		/// <para>
		/// This constructor sets this objects <see cref="Fix"/> flags to <see cref="FixFlags.All"/>,
		/// this assumes that all the data relating to this event is passed in via the <paramref name="data"/>
		/// parameter and no other data should be captured from the environment.
		/// </para>
		/// </remarks>
		public LoggingEvent(LoggingEventData data) : this(null, null, data)
		{
		}

		#endregion Public Instance Constructors

		#region Protected Instance Constructors

#if !NETCF

		/// <summary>
		/// Serialization constructor
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data.</param>
		/// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="LoggingEvent" /> class 
		/// with serialized data.
		/// </para>
		/// </remarks>
		protected LoggingEvent(SerializationInfo info, StreamingContext context) 
		{
			m_data.LoggerName = info.GetString("LoggerName");

			// Note we are deserializing the whole level object. That is the
			// name and the value. This value is correct for the source 
			// hierarchy but may not be for the target hierarchy that this
			// event may be re-logged into. If it is to be re-logged it may
			// be necessary to re-lookup the level based only on the name.
			m_data.Level = (Level)info.GetValue("Level", typeof(Level));

			m_data.Message = info.GetString("Message");
			m_data.ThreadName = info.GetString("ThreadName");
			m_data.TimeStamp = info.GetDateTime("TimeStamp");
			m_data.LocationInfo = (LocationInfo) info.GetValue("LocationInfo", typeof(LocationInfo));
			m_data.UserName = info.GetString("UserName");
			m_data.ExceptionString = info.GetString("ExceptionString");
			m_data.Properties = (PropertiesDictionary) info.GetValue("Properties", typeof(PropertiesDictionary));
			m_data.Domain = info.GetString("Domain");
			m_data.Identity = info.GetString("Identity");

			// We have restored all the values of this instance, i.e. all the values are fixed
			// Set the fix flags otherwise the data values may be overwritten from the current environment.
			m_fixFlags = FixFlags.All;
		}

#endif

		#endregion Protected Instance Constructors

		#region Public Instance Properties
	
		/// <summary>
		/// Gets the time when the current process started.
		/// </summary>
		/// <value>
		/// This is the time when this process started.
		/// </value>
		/// <remarks>
		/// <para>
		/// The TimeStamp is stored in the local time zone for this computer.
		/// </para>
		/// <para>
		/// Tries to get the start time for the current process.
		/// Failing that it returns the time of the first call to
		/// this property.
		/// </para>
		/// <para>
		/// Note that AppDomains may be loaded and unloaded within the
		/// same process without the process terminating and therefore
		/// without the process start time being reset.
		/// </para>
		/// </remarks>
		public static DateTime StartTime
		{
			get { return SystemInfo.ProcessStartTime; }
		}

		/// <summary>
		/// Gets the <see cref="Level" /> of the logging event.
		/// </summary>
		/// <value>
		/// The <see cref="Level" /> of the logging event.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets the <see cref="Level" /> of the logging event.
		/// </para>
		/// </remarks>
		public Level Level
		{
			get { return m_data.Level; } 
		}

		/// <summary>
		/// Gets the time of the logging event.
		/// </summary>
		/// <value>
		/// The time of the logging event.
		/// </value>
		/// <remarks>
		/// <para>
		/// The TimeStamp is stored in the local time zone for this computer.
		/// </para>
		/// </remarks>
		public DateTime TimeStamp
		{
			get { return m_data.TimeStamp; }
		}

		/// <summary>
		/// Gets the name of the logger that logged the event.
		/// </summary>
		/// <value>
		/// The name of the logger that logged the event.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets the name of the logger that logged the event.
		/// </para>
		/// </remarks>
		public string LoggerName
		{
			get { return m_data.LoggerName; }
		}

		/// <summary>
		/// Gets the location information for this logging event.
		/// </summary>
		/// <value>
		/// The location information for this logging event.
		/// </value>
		/// <remarks>
		/// <para>
		/// The collected information is cached for future use.
		/// </para>
		/// <para>
		/// See the <see cref="LocationInfo"/> class for more information on
		/// supported frameworks and the different behavior in Debug and
		/// Release builds.
		/// </para>
		/// </remarks>
		public LocationInfo LocationInformation
		{
			get
			{
				if (m_data.LocationInfo == null  && this.m_cacheUpdatable) 
				{
					m_data.LocationInfo = new LocationInfo(m_callerStackBoundaryDeclaringType);
				}
				return m_data.LocationInfo;
			}
		}

		/// <summary>
		/// Gets the message object used to initialize this event.
		/// </summary>
		/// <value>
		/// The message object used to initialize this event.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets the message object used to initialize this event.
		/// Note that this event may not have a valid message object.
		/// If the event is serialized the message object will not 
		/// be transferred. To get the text of the message the
		/// <see cref="RenderedMessage"/> property must be used 
		/// not this property.
		/// </para>
		/// <para>
		/// If there is no defined message object for this event then
		/// null will be returned.
		/// </para>
		/// </remarks>
		public object MessageObject
		{
			get { return m_message; }
		} 

		/// <summary>
		/// Gets the exception object used to initialize this event.
		/// </summary>
		/// <value>
		/// The exception object used to initialize this event.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets the exception object used to initialize this event.
		/// Note that this event may not have a valid exception object.
		/// If the event is serialized the exception object will not 
		/// be transferred. To get the text of the exception the
		/// <see cref="GetExceptionString"/> method must be used 
		/// not this property.
		/// </para>
		/// <para>
		/// If there is no defined exception object for this event then
		/// null will be returned.
		/// </para>
		/// </remarks>
		public Exception ExceptionObject
		{
			get { return m_thrownException; }
		} 

		/// <summary>
		/// The <see cref="ILoggerRepository"/> that this event was created in.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The <see cref="ILoggerRepository"/> that this event was created in.
		/// </para>
		/// </remarks>
		public ILoggerRepository Repository
		{
			get { return m_repository; }
		}

		/// <summary>
		/// Ensure that the repository is set.
		/// </summary>
		/// <param name="repository">the value for the repository</param>
		internal void EnsureRepository(ILoggerRepository repository)
		{
			if (repository != null)
			{
				m_repository = repository;
			}
		}

		/// <summary>
		/// Gets the message, rendered through the <see cref="ILoggerRepository.RendererMap" />.
		/// </summary>
		/// <value>
		/// The message rendered through the <see cref="ILoggerRepository.RendererMap" />.
		/// </value>
		/// <remarks>
		/// <para>
		/// The collected information is cached for future use.
		/// </para>
		/// </remarks>
		public string RenderedMessage
		{
			get 
			{ 
				if (m_data.Message == null && this.m_cacheUpdatable)
				{
					if (m_message == null)
					{
						m_data.Message = "";
					}
					else if (m_message is string)
					{
						m_data.Message = (m_message as string);
					}
					else if (m_repository != null)
					{
						m_data.Message = m_repository.RendererMap.FindAndRender(m_message);
					}
					else
					{
						// Very last resort
						m_data.Message = m_message.ToString();
					}
				}
				return m_data.Message; 
			}
		}

		/// <summary>
		/// Write the rendered message to a TextWriter
		/// </summary>
		/// <param name="writer">the writer to write the message to</param>
		/// <remarks>
		/// <para>
		/// Unlike the <see cref="RenderedMessage"/> property this method
		/// does store the message data in the internal cache. Therefore 
		/// if called only once this method should be faster than the
		/// <see cref="RenderedMessage"/> property, however if the message is
		/// to be accessed multiple times then the property will be more efficient.
		/// </para>
		/// </remarks>
		public void WriteRenderedMessage(TextWriter writer)
		{
			if (m_data.Message != null)
			{
				writer.Write(m_data.Message); 
			}
			else
			{
				if (m_message != null)
				{
					if (m_message is string)
					{
						writer.Write(m_message as string);
					}
					else if (m_repository != null)
					{
						m_repository.RendererMap.FindAndRender(m_message, writer);
					}
					else
					{
						// Very last resort
						writer.Write(m_message.ToString());
					}
				}
			}
		}

		/// <summary>
		/// Gets the name of the current thread.  
		/// </summary>
		/// <value>
		/// The name of the current thread, or the thread ID when 
		/// the name is not available.
		/// </value>
		/// <remarks>
		/// <para>
		/// The collected information is cached for future use.
		/// </para>
		/// </remarks>
		public string ThreadName
		{
			get
			{
				if (m_data.ThreadName == null && this.m_cacheUpdatable)
				{
#if NETCF
					// Get thread ID only
					m_data.ThreadName = SystemInfo.CurrentThreadId.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
#else
					m_data.ThreadName = System.Threading.Thread.CurrentThread.Name;
					if (m_data.ThreadName == null || m_data.ThreadName.Length == 0)
					{
						// The thread name is not available. Therefore we
						// go the the AppDomain to get the ID of the 
						// current thread. (Why don't Threads know their own ID?)
						try
						{
							m_data.ThreadName = SystemInfo.CurrentThreadId.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
						}
						catch(System.Security.SecurityException)
						{
							// This security exception will occur if the caller does not have 
							// some undefined set of SecurityPermission flags.
							LogLog.Debug(declaringType, "Security exception while trying to get current thread ID. Error Ignored. Empty thread name.");

							// As a last resort use the hash code of the Thread object
							m_data.ThreadName = System.Threading.Thread.CurrentThread.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture);
						}
					}
#endif
				}
				return m_data.ThreadName;
			}
		}

		/// <summary>
		/// Gets the name of the current user.
		/// </summary>
		/// <value>
		/// The name of the current user, or <c>NOT AVAILABLE</c> when the 
		/// underlying runtime has no support for retrieving the name of the 
		/// current user.
		/// </value>
		/// <remarks>
		/// <para>
		/// Calls <c>WindowsIdentity.GetCurrent().Name</c> to get the name of
		/// the current windows user.
		/// </para>
		/// <para>
		/// To improve performance, we could cache the string representation of 
		/// the name, and reuse that as long as the identity stayed constant.  
		/// Once the identity changed, we would need to re-assign and re-render 
		/// the string.
		/// </para>
		/// <para>
		/// However, the <c>WindowsIdentity.GetCurrent()</c> call seems to 
		/// return different objects every time, so the current implementation 
		/// doesn't do this type of caching.
		/// </para>
		/// <para>
		/// Timing for these operations:
		/// </para>
		/// <list type="table">
		///   <listheader>
		///     <term>Method</term>
		///     <description>Results</description>
		///   </listheader>
		///   <item>
		///	    <term><c>WindowsIdentity.GetCurrent()</c></term>
		///	    <description>10000 loops, 00:00:00.2031250 seconds</description>
		///   </item>
		///   <item>
		///	    <term><c>WindowsIdentity.GetCurrent().Name</c></term>
		///	    <description>10000 loops, 00:00:08.0468750 seconds</description>
		///   </item>
		/// </list>
		/// <para>
		/// This means we could speed things up almost 40 times by caching the 
		/// value of the <c>WindowsIdentity.GetCurrent().Name</c> property, since 
		/// this takes (8.04-0.20) = 7.84375 seconds.
		/// </para>
		/// </remarks>
		public string UserName
		{
			get
			{
				if (m_data.UserName == null  && this.m_cacheUpdatable) 
				{
#if (NETCF || SSCLI)
					// On compact framework there's no notion of current Windows user
					m_data.UserName = SystemInfo.NotAvailableText;
#else
					try
					{
						WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
						if (windowsIdentity != null && windowsIdentity.Name != null)
						{
							m_data.UserName = windowsIdentity.Name;
						}
						else
						{
							m_data.UserName = "";
						}
					}
					catch(System.Security.SecurityException)
					{
						// This security exception will occur if the caller does not have 
						// some undefined set of SecurityPermission flags.
						LogLog.Debug(declaringType, "Security exception while trying to get current windows identity. Error Ignored. Empty user name.");

						m_data.UserName = "";
					}
#endif
				}
				return m_data.UserName;
			}
		}

		/// <summary>
		/// Gets the identity of the current thread principal.
		/// </summary>
		/// <value>
		/// The string name of the identity of the current thread principal.
		/// </value>
		/// <remarks>
		/// <para>
		/// Calls <c>System.Threading.Thread.CurrentPrincipal.Identity.Name</c> to get
		/// the name of the current thread principal.
		/// </para>
		/// </remarks>
		public string Identity
		{
			get
			{
				if (m_data.Identity == null  && this.m_cacheUpdatable)
				{
#if (NETCF || SSCLI)
					// On compact framework there's no notion of current thread principals
					m_data.Identity = SystemInfo.NotAvailableText;
#else
					try
					{
						if (System.Threading.Thread.CurrentPrincipal != null && 
							System.Threading.Thread.CurrentPrincipal.Identity != null &&
							System.Threading.Thread.CurrentPrincipal.Identity.Name != null)
						{
							m_data.Identity = System.Threading.Thread.CurrentPrincipal.Identity.Name;
						}
						else
						{
							m_data.Identity = "";
						}
					}
					catch(System.Security.SecurityException)
					{
						// This security exception will occur if the caller does not have 
						// some undefined set of SecurityPermission flags.
						LogLog.Debug(declaringType, "Security exception while trying to get current thread principal. Error Ignored. Empty identity name.");

						m_data.Identity = "";
					}
#endif
				}
				return m_data.Identity;
			}
		}

		/// <summary>
		/// Gets the AppDomain friendly name.
		/// </summary>
		/// <value>
		/// The AppDomain friendly name.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets the AppDomain friendly name.
		/// </para>
		/// </remarks>
		public string Domain
		{
			get 
			{ 
				if (m_data.Domain == null  && this.m_cacheUpdatable)
				{
					m_data.Domain = SystemInfo.ApplicationFriendlyName;
				}
				return m_data.Domain; 
			}
		}

		/// <summary>
		/// Additional event specific properties.
		/// </summary>
		/// <value>
		/// Additional event specific properties.
		/// </value>
		/// <remarks>
		/// <para>
		/// A logger or an appender may attach additional
		/// properties to specific events. These properties
		/// have a string key and an object value.
		/// </para>
		/// <para>
		/// This property is for events that have been added directly to
		/// this event. The aggregate properties (which include these
		/// event properties) can be retrieved using <see cref="LookupProperty"/>
		/// and <see cref="GetProperties"/>.
		/// </para>
		/// <para>
		/// Once the properties have been fixed <see cref="Fix"/> this property
		/// returns the combined cached properties. This ensures that updates to
		/// this property are always reflected in the underlying storage. When
		/// returning the combined properties there may be more keys in the
		/// Dictionary than expected.
		/// </para>
		/// </remarks>
		public PropertiesDictionary Properties
		{
			get 
			{ 
				// If we have cached properties then return that otherwise changes will be lost
				if (m_data.Properties != null)
				{
					return m_data.Properties;
				}

				if (m_eventProperties == null)
				{
					m_eventProperties = new PropertiesDictionary();
				}
				return m_eventProperties; 
			}
		}

		/// <summary>
		/// The fixed fields in this event
		/// </summary>
		/// <value>
		/// The set of fields that are fixed in this event
		/// </value>
		/// <remarks>
		/// <para>
		/// Fields will not be fixed if they have previously been fixed.
		/// It is not possible to 'unfix' a field.
		/// </para>
		/// </remarks>
		public FixFlags Fix
		{
			get { return m_fixFlags; }
			set { this.FixVolatileData(value); }
		}

		#endregion Public Instance Properties

		#region Implementation of ISerializable

#if !NETCF

		/// <summary>
		/// Serializes this object into the <see cref="SerializationInfo" /> provided.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> to populate with data.</param>
		/// <param name="context">The destination for this serialization.</param>
		/// <remarks>
		/// <para>
		/// The data in this event must be fixed before it can be serialized.
		/// </para>
		/// <para>
		/// The <see cref="FixVolatileData()"/> method must be called during the
		/// <see cref="log4net.Appender.IAppender.DoAppend"/> method call if this event 
		/// is to be used outside that method.
		/// </para>
		/// </remarks>
#if NET_4_0
        [System.Security.SecurityCritical]
#else
		[System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.Demand, SerializationFormatter=true)]
#endif
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			// The caller must call FixVolatileData before this object
			// can be serialized.

			info.AddValue("LoggerName", m_data.LoggerName);
			info.AddValue("Level", m_data.Level);
			info.AddValue("Message", m_data.Message);
			info.AddValue("ThreadName", m_data.ThreadName);
			info.AddValue("TimeStamp", m_data.TimeStamp);
			info.AddValue("LocationInfo", m_data.LocationInfo);
			info.AddValue("UserName", m_data.UserName);
			info.AddValue("ExceptionString", m_data.ExceptionString);
			info.AddValue("Properties", m_data.Properties);
			info.AddValue("Domain", m_data.Domain);
			info.AddValue("Identity", m_data.Identity);
		}

#endif

		#endregion Implementation of ISerializable

		#region Public Instance Methods

		/// <summary>
		/// Gets the portable data for this <see cref="LoggingEvent" />.
		/// </summary>
		/// <returns>The <see cref="LoggingEventData"/> for this event.</returns>
		/// <remarks>
		/// <para>
		/// A new <see cref="LoggingEvent"/> can be constructed using a
		/// <see cref="LoggingEventData"/> instance.
		/// </para>
		/// <para>
		/// Does a <see cref="FixFlags.Partial"/> fix of the data
		/// in the logging event before returning the event data.
		/// </para>
		/// </remarks>
		public LoggingEventData GetLoggingEventData()
		{
			return GetLoggingEventData(FixFlags.Partial);
		}

		/// <summary>
		/// Gets the portable data for this <see cref="LoggingEvent" />.
		/// </summary>
		/// <param name="fixFlags">The set of data to ensure is fixed in the LoggingEventData</param>
		/// <returns>The <see cref="LoggingEventData"/> for this event.</returns>
		/// <remarks>
		/// <para>
		/// A new <see cref="LoggingEvent"/> can be constructed using a
		/// <see cref="LoggingEventData"/> instance.
		/// </para>
		/// </remarks>
		public LoggingEventData GetLoggingEventData(FixFlags fixFlags)
		{
			Fix = fixFlags;
			return m_data;
		}

		/// <summary>
		/// Returns this event's exception's rendered using the 
		/// <see cref="ILoggerRepository.RendererMap" />.
		/// </summary>
		/// <returns>
		/// This event's exception's rendered using the <see cref="ILoggerRepository.RendererMap" />.
		/// </returns>
		/// <remarks>
		/// <para>
		/// <b>Obsolete. Use <see cref="GetExceptionString"/> instead.</b>
		/// </para>
		/// </remarks>
		[Obsolete("Use GetExceptionString instead")]
		public string GetExceptionStrRep() 
		{
			return GetExceptionString();
		}

		/// <summary>
		/// Returns this event's exception's rendered using the 
		/// <see cref="ILoggerRepository.RendererMap" />.
		/// </summary>
		/// <returns>
		/// This event's exception's rendered using the <see cref="ILoggerRepository.RendererMap" />.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Returns this event's exception's rendered using the 
		/// <see cref="ILoggerRepository.RendererMap" />.
		/// </para>
		/// </remarks>
		public string GetExceptionString() 
		{
			if (m_data.ExceptionString == null  && this.m_cacheUpdatable)
			{
				if (m_thrownException != null)
				{
					if (m_repository != null)
					{
						// Render exception using the repositories renderer map
						m_data.ExceptionString = m_repository.RendererMap.FindAndRender(m_thrownException);
					}
					else
					{
						// Very last resort
						m_data.ExceptionString = m_thrownException.ToString();
					}
				}
				else
				{
					m_data.ExceptionString = "";
				}
			}
			return m_data.ExceptionString;
		}

		/// <summary>
		/// Fix instance fields that hold volatile data.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Some of the values in instances of <see cref="LoggingEvent"/>
		/// are considered volatile, that is the values are correct at the
		/// time the event is delivered to appenders, but will not be consistent
		/// at any time afterwards. If an event is to be stored and then processed
		/// at a later time these volatile values must be fixed by calling
		/// <see cref="FixVolatileData()"/>. There is a performance penalty
		/// incurred by calling <see cref="FixVolatileData()"/> but it
		/// is essential to maintaining data consistency.
		/// </para>
		/// <para>
		/// Calling <see cref="FixVolatileData()"/> is equivalent to
		/// calling <see cref="FixVolatileData(bool)"/> passing the parameter
		/// <c>false</c>.
		/// </para>
		/// <para>
		/// See <see cref="FixVolatileData(bool)"/> for more
		/// information.
		/// </para>
		/// </remarks>
		[Obsolete("Use Fix property")]
		public void FixVolatileData()
		{
			Fix = FixFlags.All;
		}

		/// <summary>
		/// Fixes instance fields that hold volatile data.
		/// </summary>
		/// <param name="fastButLoose">Set to <c>true</c> to not fix data that takes a long time to fix.</param>
		/// <remarks>
		/// <para>
		/// Some of the values in instances of <see cref="LoggingEvent"/>
		/// are considered volatile, that is the values are correct at the
		/// time the event is delivered to appenders, but will not be consistent
		/// at any time afterwards. If an event is to be stored and then processed
		/// at a later time these volatile values must be fixed by calling
		/// <see cref="FixVolatileData()"/>. There is a performance penalty
		/// for incurred by calling <see cref="FixVolatileData()"/> but it
		/// is essential to maintaining data consistency.
		/// </para>
		/// <para>
		/// The <paramref name="fastButLoose"/> param controls the data that
		/// is fixed. Some of the data that can be fixed takes a long time to 
		/// generate, therefore if you do not require those settings to be fixed
		/// they can be ignored by setting the <paramref name="fastButLoose"/> param
		/// to <c>true</c>. This setting will ignore the <see cref="LocationInformation"/>
		/// and <see cref="UserName"/> settings.
		/// </para>
		/// <para>
		/// Set <paramref name="fastButLoose"/> to <c>false</c> to ensure that all 
		/// settings are fixed.
		/// </para>
		/// </remarks>
		[Obsolete("Use Fix property")]
		public void FixVolatileData(bool fastButLoose)
		{
			if (fastButLoose)
			{
				Fix = FixFlags.Partial;
			}
			else
			{
				Fix = FixFlags.All;
			}
		}

		/// <summary>
		/// Fix the fields specified by the <see cref="FixFlags"/> parameter
		/// </summary>
		/// <param name="flags">the fields to fix</param>
		/// <remarks>
		/// <para>
		/// Only fields specified in the <paramref name="flags"/> will be fixed.
		/// Fields will not be fixed if they have previously been fixed.
		/// It is not possible to 'unfix' a field.
		/// </para>
		/// </remarks>
		protected void FixVolatileData(FixFlags flags)
		{
			object forceCreation = null;

			//Unlock the cache so that new values can be stored
			//This may not be ideal if we are no longer in the correct context
			//and someone calls fix. 
			m_cacheUpdatable=true;

			// determine the flags that we are actually fixing
			FixFlags updateFlags = (FixFlags)((flags ^ m_fixFlags) & flags);

			if (updateFlags > 0) 
			{
				if ((updateFlags & FixFlags.Message) != 0)
				{
					// Force the message to be rendered
					forceCreation = this.RenderedMessage;

					m_fixFlags |= FixFlags.Message;
				}
				if ((updateFlags & FixFlags.ThreadName) != 0)
				{
					// Grab the thread name
					forceCreation = this.ThreadName;

					m_fixFlags |= FixFlags.ThreadName;
				}

				if ((updateFlags & FixFlags.LocationInfo) != 0)
				{
					// Force the location information to be loaded
					forceCreation = this.LocationInformation;

					m_fixFlags |= FixFlags.LocationInfo;
				}
				if ((updateFlags & FixFlags.UserName) != 0)
				{
					// Grab the user name
					forceCreation = this.UserName;

					m_fixFlags |= FixFlags.UserName;
				}
				if ((updateFlags & FixFlags.Domain) != 0)
				{
					// Grab the domain name
					forceCreation = this.Domain;

					m_fixFlags |= FixFlags.Domain;
				}
				if ((updateFlags & FixFlags.Identity) != 0)
				{
					// Grab the identity
					forceCreation = this.Identity;

					m_fixFlags |= FixFlags.Identity;
				}

				if ((updateFlags & FixFlags.Exception) != 0)
				{
					// Force the exception text to be loaded
					forceCreation = GetExceptionString();

					m_fixFlags |= FixFlags.Exception;
				}

				if ((updateFlags & FixFlags.Properties) != 0)
				{
					CacheProperties();

					m_fixFlags |= FixFlags.Properties;
				}
			}

			// avoid warning CS0219
			if (forceCreation != null) 
			{
			}

			//Finaly lock everything we've cached.
			m_cacheUpdatable=false;
		}

		#endregion Public Instance Methods

		#region Protected Instance Methods

		private void CreateCompositeProperties()
		{
			m_compositeProperties = new CompositeProperties();

			if (m_eventProperties != null)
			{
				m_compositeProperties.Add(m_eventProperties);
			}
#if !NETCF
			PropertiesDictionary logicalThreadProperties = LogicalThreadContext.Properties.GetProperties(false);
			if (logicalThreadProperties != null)
			{
				m_compositeProperties.Add(logicalThreadProperties);
			}
#endif
			PropertiesDictionary threadProperties = ThreadContext.Properties.GetProperties(false);
			if (threadProperties != null)
			{
				m_compositeProperties.Add(threadProperties);
			}

			// TODO: Add Repository Properties

			m_compositeProperties.Add(GlobalContext.Properties.GetReadOnlyProperties());
		}

		private void CacheProperties()
		{
			if (m_data.Properties == null  && this.m_cacheUpdatable)
			{
				if (m_compositeProperties == null)
				{
					CreateCompositeProperties();
				}

				PropertiesDictionary flattenedProperties = m_compositeProperties.Flatten();

				PropertiesDictionary fixedProperties = new PropertiesDictionary();

				// Validate properties
				foreach(DictionaryEntry entry in flattenedProperties)
				{
					string key = entry.Key as string;

					if (key != null)
					{
						object val = entry.Value;

						// Fix any IFixingRequired objects
						IFixingRequired fixingRequired = val as IFixingRequired;
						if (fixingRequired != null)
						{
							val = fixingRequired.GetFixedObject();
						}

						// Strip keys with null values
						if (val != null)
						{
							fixedProperties[key] = val;
						}
					}
				}

				m_data.Properties = fixedProperties;
			}
		}

		/// <summary>
		/// Lookup a composite property in this event
		/// </summary>
		/// <param name="key">the key for the property to lookup</param>
		/// <returns>the value for the property</returns>
		/// <remarks>
		/// <para>
		/// This event has composite properties that combine together properties from
		/// several different contexts in the following order:
		/// <list type="definition">
		///		<item>
		/// 		<term>this events properties</term>
		/// 		<description>
		/// 		This event has <see cref="Properties"/> that can be set. These 
		/// 		properties are specific to this event only.
		/// 		</description>
		/// 	</item>
		/// 	<item>
		/// 		<term>the thread properties</term>
		/// 		<description>
		/// 		The <see cref="ThreadContext.Properties"/> that are set on the current
		/// 		thread. These properties are shared by all events logged on this thread.
		/// 		</description>
		/// 	</item>
		/// 	<item>
		/// 		<term>the global properties</term>
		/// 		<description>
		/// 		The <see cref="GlobalContext.Properties"/> that are set globally. These 
		/// 		properties are shared by all the threads in the AppDomain.
		/// 		</description>
		/// 	</item>
		/// </list>
		/// </para>
		/// </remarks>
		public object LookupProperty(string key)
		{
			if (m_data.Properties != null)
			{
				return m_data.Properties[key];
			}
			if (m_compositeProperties == null)
			{
				CreateCompositeProperties();
			}
			return m_compositeProperties[key];
		}

		/// <summary>
		/// Get all the composite properties in this event
		/// </summary>
		/// <returns>the <see cref="PropertiesDictionary"/> containing all the properties</returns>
		/// <remarks>
		/// <para>
		/// See <see cref="LookupProperty"/> for details of the composite properties 
		/// stored by the event.
		/// </para>
		/// <para>
		/// This method returns a single <see cref="PropertiesDictionary"/> containing all the
		/// properties defined for this event.
		/// </para>
		/// </remarks>
		public PropertiesDictionary GetProperties()
		{
			if (m_data.Properties != null)
			{
				return m_data.Properties;
			}
			if (m_compositeProperties == null)
			{
				CreateCompositeProperties();
			}
			return m_compositeProperties.Flatten();
		}

		#endregion Public Instance Methods

		#region Private Instance Fields

		/// <summary>
		/// The internal logging event data.
		/// </summary>
		private LoggingEventData m_data;

		/// <summary>
		/// The internal logging event data.
		/// </summary>
		private CompositeProperties m_compositeProperties;

		/// <summary>
		/// The internal logging event data.
		/// </summary>
		private PropertiesDictionary m_eventProperties;

		/// <summary>
		/// The fully qualified Type of the calling 
		/// logger class in the stack frame (i.e. the declaring type of the method).
		/// </summary>
		private readonly Type m_callerStackBoundaryDeclaringType;

		/// <summary>
		/// The application supplied message of logging event.
		/// </summary>
		private readonly object m_message;

		/// <summary>
		/// The exception that was thrown.
		/// </summary>
		/// <remarks>
		/// This is not serialized. The string representation
		/// is serialized instead.
		/// </remarks>
		private readonly Exception m_thrownException;

		/// <summary>
		/// The repository that generated the logging event
		/// </summary>
		/// <remarks>
		/// This is not serialized.
		/// </remarks>
		private ILoggerRepository m_repository = null;

		/// <summary>
		/// The fix state for this event
		/// </summary>
		/// <remarks>
		/// These flags indicate which fields have been fixed.
		/// Not serialized.
		/// </remarks>
		private FixFlags m_fixFlags = FixFlags.None;

		/// <summary>
		/// Indicated that the internal cache is updateable (ie not fixed)
		/// </summary>
		/// <remarks>
		/// This is a seperate flag to m_fixFlags as it allows incrementel fixing and simpler
		/// changes in the caching strategy.
		/// </remarks>
		private bool m_cacheUpdatable = true;

		#endregion Private Instance Fields

		#region Constants

		/// <summary>
		/// The key into the Properties map for the host name value.
		/// </summary>
		public const string HostNameProperty = "log4net:HostName";

		/// <summary>
		/// The key into the Properties map for the thread identity value.
		/// </summary>
		public const string IdentityProperty = "log4net:Identity";

		/// <summary>
		/// The key into the Properties map for the user name value.
		/// </summary>
		public const string UserNameProperty = "log4net:UserName";

		#endregion
	}
}
