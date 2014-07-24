using log4net.Repository;
using log4net.Util;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

namespace log4net.Core
{
	[Serializable]
	public class LoggingEvent : ISerializable
	{
		public const string HostNameProperty = "log4net:HostName";

		public const string IdentityProperty = "log4net:Identity";

		public const string UserNameProperty = "log4net:UserName";

		private readonly static Type declaringType;

		private LoggingEventData m_data;

		private CompositeProperties m_compositeProperties;

		private PropertiesDictionary m_eventProperties;

		private readonly Type m_callerStackBoundaryDeclaringType;

		private readonly object m_message;

		private readonly Exception m_thrownException;

		private ILoggerRepository m_repository;

		private FixFlags m_fixFlags;

		private bool m_cacheUpdatable = true;

		public string Domain
		{
			get
			{
				if (this.m_data.Domain == null && this.m_cacheUpdatable)
				{
					this.m_data.Domain = SystemInfo.ApplicationFriendlyName;
				}
				return this.m_data.Domain;
			}
		}

		public Exception ExceptionObject
		{
			get
			{
				return this.m_thrownException;
			}
		}

		public FixFlags Fix
		{
			get
			{
				return this.m_fixFlags;
			}
			set
			{
				this.FixVolatileData(value);
			}
		}

		public string Identity
		{
			get
			{
				if (this.m_data.Identity == null && this.m_cacheUpdatable)
				{
					try
					{
						if (Thread.CurrentPrincipal == null || Thread.CurrentPrincipal.Identity == null || Thread.CurrentPrincipal.Identity.Name == null)
						{
							this.m_data.Identity = "";
						}
						else
						{
							this.m_data.Identity = Thread.CurrentPrincipal.Identity.Name;
						}
					}
					catch (SecurityException securityException)
					{
						LogLog.Debug(LoggingEvent.declaringType, "Security exception while trying to get current thread principal. Error Ignored. Empty identity name.");
						this.m_data.Identity = "";
					}
				}
				return this.m_data.Identity;
			}
		}

		public log4net.Core.Level Level
		{
			get
			{
				return this.m_data.Level;
			}
		}

		public LocationInfo LocationInformation
		{
			get
			{
				if (this.m_data.LocationInfo == null && this.m_cacheUpdatable)
				{
					this.m_data.LocationInfo = new LocationInfo(this.m_callerStackBoundaryDeclaringType);
				}
				return this.m_data.LocationInfo;
			}
		}

		public string LoggerName
		{
			get
			{
				return this.m_data.LoggerName;
			}
		}

		public object MessageObject
		{
			get
			{
				return this.m_message;
			}
		}

		public PropertiesDictionary Properties
		{
			get
			{
				if (this.m_data.Properties != null)
				{
					return this.m_data.Properties;
				}
				if (this.m_eventProperties == null)
				{
					this.m_eventProperties = new PropertiesDictionary();
				}
				return this.m_eventProperties;
			}
		}

		public string RenderedMessage
		{
			get
			{
				if (this.m_data.Message == null && this.m_cacheUpdatable)
				{
					if (this.m_message == null)
					{
						this.m_data.Message = "";
					}
					else if (this.m_message is string)
					{
						this.m_data.Message = this.m_message as string;
					}
					else if (this.m_repository == null)
					{
						this.m_data.Message = this.m_message.ToString();
					}
					else
					{
						this.m_data.Message = this.m_repository.RendererMap.FindAndRender(this.m_message);
					}
				}
				return this.m_data.Message;
			}
		}

		public ILoggerRepository Repository
		{
			get
			{
				return this.m_repository;
			}
		}

		public static DateTime StartTime
		{
			get
			{
				return SystemInfo.ProcessStartTime;
			}
		}

		public string ThreadName
		{
			get
			{
				if (this.m_data.ThreadName == null && this.m_cacheUpdatable)
				{
					this.m_data.ThreadName = Thread.CurrentThread.Name;
					if (this.m_data.ThreadName == null || this.m_data.ThreadName.Length == 0)
					{
						try
						{
							int currentThreadId = SystemInfo.CurrentThreadId;
							this.m_data.ThreadName = currentThreadId.ToString(NumberFormatInfo.InvariantInfo);
						}
						catch (SecurityException securityException)
						{
							LogLog.Debug(LoggingEvent.declaringType, "Security exception while trying to get current thread ID. Error Ignored. Empty thread name.");
							int hashCode = Thread.CurrentThread.GetHashCode();
							this.m_data.ThreadName = hashCode.ToString(CultureInfo.InvariantCulture);
						}
					}
				}
				return this.m_data.ThreadName;
			}
		}

		public DateTime TimeStamp
		{
			get
			{
				return this.m_data.TimeStamp;
			}
		}

		public string UserName
		{
			get
			{
				if (this.m_data.UserName == null && this.m_cacheUpdatable)
				{
					try
					{
						WindowsIdentity current = WindowsIdentity.GetCurrent();
						if (current == null || current.Name == null)
						{
							this.m_data.UserName = "";
						}
						else
						{
							this.m_data.UserName = current.Name;
						}
					}
					catch (SecurityException securityException)
					{
						LogLog.Debug(LoggingEvent.declaringType, "Security exception while trying to get current windows identity. Error Ignored. Empty user name.");
						this.m_data.UserName = "";
					}
				}
				return this.m_data.UserName;
			}
		}

		static LoggingEvent()
		{
			LoggingEvent.declaringType = typeof(LoggingEvent);
		}

		public LoggingEvent(Type callerStackBoundaryDeclaringType, ILoggerRepository repository, string loggerName, log4net.Core.Level level, object message, Exception exception)
		{
			this.m_callerStackBoundaryDeclaringType = callerStackBoundaryDeclaringType;
			this.m_message = message;
			this.m_repository = repository;
			this.m_thrownException = exception;
			this.m_data.LoggerName = loggerName;
			this.m_data.Level = level;
			this.m_data.TimeStamp = DateTime.Now;
		}

		public LoggingEvent(Type callerStackBoundaryDeclaringType, ILoggerRepository repository, LoggingEventData data, FixFlags fixedData)
		{
			this.m_callerStackBoundaryDeclaringType = callerStackBoundaryDeclaringType;
			this.m_repository = repository;
			this.m_data = data;
			this.m_fixFlags = fixedData;
		}

		public LoggingEvent(Type callerStackBoundaryDeclaringType, ILoggerRepository repository, LoggingEventData data) : this(callerStackBoundaryDeclaringType, repository, data, FixFlags.All)
		{
		}

		public LoggingEvent(LoggingEventData data) : this(null, null, data)
		{
		}

		protected LoggingEvent(SerializationInfo info, StreamingContext context)
		{
			this.m_data.LoggerName = info.GetString("LoggerName");
			this.m_data.Level = (log4net.Core.Level)info.GetValue("Level", typeof(log4net.Core.Level));
			this.m_data.Message = info.GetString("Message");
			this.m_data.ThreadName = info.GetString("ThreadName");
			this.m_data.TimeStamp = info.GetDateTime("TimeStamp");
			this.m_data.LocationInfo = (LocationInfo)info.GetValue("LocationInfo", typeof(LocationInfo));
			this.m_data.UserName = info.GetString("UserName");
			this.m_data.ExceptionString = info.GetString("ExceptionString");
			this.m_data.Properties = (PropertiesDictionary)info.GetValue("Properties", typeof(PropertiesDictionary));
			this.m_data.Domain = info.GetString("Domain");
			this.m_data.Identity = info.GetString("Identity");
			this.m_fixFlags = FixFlags.All;
		}

		private void CacheProperties()
		{
			if (this.m_data.Properties == null && this.m_cacheUpdatable)
			{
				if (this.m_compositeProperties == null)
				{
					this.CreateCompositeProperties();
				}
				PropertiesDictionary propertiesDictionaries = this.m_compositeProperties.Flatten();
				PropertiesDictionary propertiesDictionaries1 = new PropertiesDictionary();
				foreach (DictionaryEntry dictionaryEntry in (IEnumerable)propertiesDictionaries)
				{
					string key = dictionaryEntry.Key as string;
					if (key == null)
					{
						continue;
					}
					object value = dictionaryEntry.Value;
					IFixingRequired fixingRequired = value as IFixingRequired;
					if (fixingRequired != null)
					{
						value = fixingRequired.GetFixedObject();
					}
					if (value == null)
					{
						continue;
					}
					propertiesDictionaries1[key] = value;
				}
				this.m_data.Properties = propertiesDictionaries1;
			}
		}

		private void CreateCompositeProperties()
		{
			this.m_compositeProperties = new CompositeProperties();
			if (this.m_eventProperties != null)
			{
				this.m_compositeProperties.Add(this.m_eventProperties);
			}
			PropertiesDictionary properties = LogicalThreadContext.Properties.GetProperties(false);
			if (properties != null)
			{
				this.m_compositeProperties.Add(properties);
			}
			PropertiesDictionary propertiesDictionaries = ThreadContext.Properties.GetProperties(false);
			if (propertiesDictionaries != null)
			{
				this.m_compositeProperties.Add(propertiesDictionaries);
			}
			this.m_compositeProperties.Add(GlobalContext.Properties.GetReadOnlyProperties());
		}

		internal void EnsureRepository(ILoggerRepository repository)
		{
			if (repository != null)
			{
				this.m_repository = repository;
			}
		}

		[Obsolete("Use Fix property")]
		public void FixVolatileData()
		{
			this.Fix = FixFlags.All;
		}

		[Obsolete("Use Fix property")]
		public void FixVolatileData(bool fastButLoose)
		{
			if (fastButLoose)
			{
				this.Fix = FixFlags.Partial;
				return;
			}
			this.Fix = FixFlags.All;
		}

		protected void FixVolatileData(FixFlags flags)
		{
			object renderedMessage = null;
			this.m_cacheUpdatable = true;
			FixFlags fixFlag = (flags ^ this.m_fixFlags) & flags;
			if (fixFlag > FixFlags.None)
			{
				if ((fixFlag & FixFlags.Message) != FixFlags.None)
				{
					renderedMessage = this.RenderedMessage;
					LoggingEvent mFixFlags = this;
					mFixFlags.m_fixFlags = mFixFlags.m_fixFlags | FixFlags.Message;
				}
				if ((fixFlag & FixFlags.ThreadName) != FixFlags.None)
				{
					renderedMessage = this.ThreadName;
					LoggingEvent loggingEvent = this;
					loggingEvent.m_fixFlags = loggingEvent.m_fixFlags | FixFlags.ThreadName;
				}
				if ((fixFlag & FixFlags.LocationInfo) != FixFlags.None)
				{
					renderedMessage = this.LocationInformation;
					LoggingEvent mFixFlags1 = this;
					mFixFlags1.m_fixFlags = mFixFlags1.m_fixFlags | FixFlags.LocationInfo;
				}
				if ((fixFlag & FixFlags.UserName) != FixFlags.None)
				{
					renderedMessage = this.UserName;
					LoggingEvent loggingEvent1 = this;
					loggingEvent1.m_fixFlags = loggingEvent1.m_fixFlags | FixFlags.UserName;
				}
				if ((fixFlag & FixFlags.Domain) != FixFlags.None)
				{
					renderedMessage = this.Domain;
					LoggingEvent mFixFlags2 = this;
					mFixFlags2.m_fixFlags = mFixFlags2.m_fixFlags | FixFlags.Domain;
				}
				if ((fixFlag & FixFlags.Identity) != FixFlags.None)
				{
					renderedMessage = this.Identity;
					LoggingEvent loggingEvent2 = this;
					loggingEvent2.m_fixFlags = loggingEvent2.m_fixFlags | FixFlags.Identity;
				}
				if ((fixFlag & FixFlags.Exception) != FixFlags.None)
				{
					renderedMessage = this.GetExceptionString();
					LoggingEvent mFixFlags3 = this;
					mFixFlags3.m_fixFlags = mFixFlags3.m_fixFlags | FixFlags.Exception;
				}
				if ((fixFlag & FixFlags.Properties) != FixFlags.None)
				{
					this.CacheProperties();
					LoggingEvent loggingEvent3 = this;
					loggingEvent3.m_fixFlags = loggingEvent3.m_fixFlags | FixFlags.Properties;
				}
			}
			this.m_cacheUpdatable = false;
		}

		public string GetExceptionString()
		{
			if (this.m_data.ExceptionString == null && this.m_cacheUpdatable)
			{
				if (this.m_thrownException == null)
				{
					this.m_data.ExceptionString = "";
				}
				else if (this.m_repository == null)
				{
					this.m_data.ExceptionString = this.m_thrownException.ToString();
				}
				else
				{
					this.m_data.ExceptionString = this.m_repository.RendererMap.FindAndRender(this.m_thrownException);
				}
			}
			return this.m_data.ExceptionString;
		}

		[Obsolete("Use GetExceptionString instead")]
		public string GetExceptionStrRep()
		{
			return this.GetExceptionString();
		}

		public LoggingEventData GetLoggingEventData()
		{
			return this.GetLoggingEventData(FixFlags.Partial);
		}

		public LoggingEventData GetLoggingEventData(FixFlags fixFlags)
		{
			this.Fix = fixFlags;
			return this.m_data;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("LoggerName", this.m_data.LoggerName);
			info.AddValue("Level", this.m_data.Level);
			info.AddValue("Message", this.m_data.Message);
			info.AddValue("ThreadName", this.m_data.ThreadName);
			info.AddValue("TimeStamp", this.m_data.TimeStamp);
			info.AddValue("LocationInfo", this.m_data.LocationInfo);
			info.AddValue("UserName", this.m_data.UserName);
			info.AddValue("ExceptionString", this.m_data.ExceptionString);
			info.AddValue("Properties", this.m_data.Properties);
			info.AddValue("Domain", this.m_data.Domain);
			info.AddValue("Identity", this.m_data.Identity);
		}

		public PropertiesDictionary GetProperties()
		{
			if (this.m_data.Properties != null)
			{
				return this.m_data.Properties;
			}
			if (this.m_compositeProperties == null)
			{
				this.CreateCompositeProperties();
			}
			return this.m_compositeProperties.Flatten();
		}

		public object LookupProperty(string key)
		{
			if (this.m_data.Properties != null)
			{
				return this.m_data.Properties[key];
			}
			if (this.m_compositeProperties == null)
			{
				this.CreateCompositeProperties();
			}
			return this.m_compositeProperties[key];
		}

		public void WriteRenderedMessage(TextWriter writer)
		{
			if (this.m_data.Message != null)
			{
				writer.Write(this.m_data.Message);
				return;
			}
			if (this.m_message != null)
			{
				if (this.m_message is string)
				{
					writer.Write(this.m_message as string);
					return;
				}
				if (this.m_repository != null)
				{
					this.m_repository.RendererMap.FindAndRender(this.m_message, writer);
					return;
				}
				writer.Write(this.m_message.ToString());
			}
		}
	}
}