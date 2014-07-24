using log4net.Core;
using log4net.Util;
using System;
using System.Runtime.InteropServices;

namespace log4net.Appender
{
	public class NetSendAppender : AppenderSkeleton
	{
		private string m_server;

		private string m_sender;

		private string m_recipient;

		private log4net.Core.SecurityContext m_securityContext;

		public string Recipient
		{
			get
			{
				return this.m_recipient;
			}
			set
			{
				this.m_recipient = value;
			}
		}

		protected override bool RequiresLayout
		{
			get
			{
				return true;
			}
		}

		public log4net.Core.SecurityContext SecurityContext
		{
			get
			{
				return this.m_securityContext;
			}
			set
			{
				this.m_securityContext = value;
			}
		}

		public string Sender
		{
			get
			{
				return this.m_sender;
			}
			set
			{
				this.m_sender = value;
			}
		}

		public string Server
		{
			get
			{
				return this.m_server;
			}
			set
			{
				this.m_server = value;
			}
		}

		public NetSendAppender()
		{
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			if (this.Recipient == null)
			{
				throw new ArgumentNullException("Recipient", "The required property 'Recipient' was not specified.");
			}
			if (this.m_securityContext == null)
			{
				this.m_securityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
			}
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			NativeError error = null;
			string str = base.RenderLoggingEvent(loggingEvent);
			using (IDisposable disposable = this.m_securityContext.Impersonate(this))
			{
				int num = NetSendAppender.NetMessageBufferSend(this.Server, this.Recipient, this.Sender, str, str.Length * Marshal.SystemDefaultCharSize);
				if (num != 0)
				{
					error = NativeError.GetError(num);
				}
			}
			if (error != null)
			{
				IErrorHandler errorHandler = this.ErrorHandler;
				string[] strArrays = new string[] { error.ToString(), " (Params: Server=", this.Server, ", Recipient=", this.Recipient, ", Sender=", this.Sender, ")" };
				errorHandler.Error(string.Concat(strArrays));
			}
		}

		[DllImport("netapi32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
		protected static extern int NetMessageBufferSend(string serverName, string msgName, string fromName, string buffer, int bufferSize);
	}
}