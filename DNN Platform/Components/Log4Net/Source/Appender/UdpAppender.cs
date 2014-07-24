using log4net.Core;
using log4net.Util;
using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace log4net.Appender
{
	public class UdpAppender : AppenderSkeleton
	{
		private IPAddress m_remoteAddress;

		private int m_remotePort;

		private IPEndPoint m_remoteEndPoint;

		private int m_localPort;

		private UdpClient m_client;

		private System.Text.Encoding m_encoding = System.Text.Encoding.Default;

		protected UdpClient Client
		{
			get
			{
				return this.m_client;
			}
			set
			{
				this.m_client = value;
			}
		}

		public System.Text.Encoding Encoding
		{
			get
			{
				return this.m_encoding;
			}
			set
			{
				this.m_encoding = value;
			}
		}

		public int LocalPort
		{
			get
			{
				return this.m_localPort;
			}
			set
			{
				if (value != 0 && (value < 0 || value > 65535))
				{
					object obj = value;
					string[] str = new string[] { "The value specified is less than ", null, null, null, null };
					str[1] = 0.ToString(NumberFormatInfo.InvariantInfo);
					str[2] = " or greater than ";
					str[3] = 65535.ToString(NumberFormatInfo.InvariantInfo);
					str[4] = ".";
					throw SystemInfo.CreateArgumentOutOfRangeException("value", obj, string.Concat(str));
				}
				this.m_localPort = value;
			}
		}

		public IPAddress RemoteAddress
		{
			get
			{
				return this.m_remoteAddress;
			}
			set
			{
				this.m_remoteAddress = value;
			}
		}

		protected IPEndPoint RemoteEndPoint
		{
			get
			{
				return this.m_remoteEndPoint;
			}
			set
			{
				this.m_remoteEndPoint = value;
			}
		}

		public int RemotePort
		{
			get
			{
				return this.m_remotePort;
			}
			set
			{
				if (value < 0 || value > 65535)
				{
					object obj = value;
					string[] str = new string[] { "The value specified is less than ", null, null, null, null };
					str[1] = 0.ToString(NumberFormatInfo.InvariantInfo);
					str[2] = " or greater than ";
					str[3] = 65535.ToString(NumberFormatInfo.InvariantInfo);
					str[4] = ".";
					throw SystemInfo.CreateArgumentOutOfRangeException("value", obj, string.Concat(str));
				}
				this.m_remotePort = value;
			}
		}

		protected override bool RequiresLayout
		{
			get
			{
				return true;
			}
		}

		public UdpAppender()
		{
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			if (this.RemoteAddress == null)
			{
				throw new ArgumentNullException("The required property 'Address' was not specified.");
			}
			if (this.RemotePort < 0 || this.RemotePort > 65535)
			{
				object remotePort = this.RemotePort;
				string[] str = new string[] { "The RemotePort is less than ", null, null, null, null };
				str[1] = 0.ToString(NumberFormatInfo.InvariantInfo);
				str[2] = " or greater than ";
				str[3] = 65535.ToString(NumberFormatInfo.InvariantInfo);
				str[4] = ".";
				throw SystemInfo.CreateArgumentOutOfRangeException("this.RemotePort", remotePort, string.Concat(str));
			}
			if (this.LocalPort != 0 && (this.LocalPort < 0 || this.LocalPort > 65535))
			{
				object localPort = this.LocalPort;
				string[] strArrays = new string[] { "The LocalPort is less than ", null, null, null, null };
				strArrays[1] = 0.ToString(NumberFormatInfo.InvariantInfo);
				strArrays[2] = " or greater than ";
				strArrays[3] = 65535.ToString(NumberFormatInfo.InvariantInfo);
				strArrays[4] = ".";
				throw SystemInfo.CreateArgumentOutOfRangeException("this.LocalPort", localPort, string.Concat(strArrays));
			}
			this.RemoteEndPoint = new IPEndPoint(this.RemoteAddress, this.RemotePort);
			this.InitializeClientConnection();
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			try
			{
				byte[] bytes = this.m_encoding.GetBytes(base.RenderLoggingEvent(loggingEvent).ToCharArray());
				this.Client.Send(bytes, (int)bytes.Length, this.RemoteEndPoint);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				IErrorHandler errorHandler = this.ErrorHandler;
				object[] str = new object[] { "Unable to send logging event to remote host ", this.RemoteAddress.ToString(), " on port ", this.RemotePort, "." };
				errorHandler.Error(string.Concat(str), exception, ErrorCode.WriteFailure);
			}
		}

		protected virtual void InitializeClientConnection()
		{
			try
			{
				if (this.LocalPort != 0)
				{
					this.Client = new UdpClient(this.LocalPort, this.RemoteAddress.AddressFamily);
				}
				else
				{
					this.Client = new UdpClient(this.RemoteAddress.AddressFamily);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				IErrorHandler errorHandler = this.ErrorHandler;
				int localPort = this.LocalPort;
				errorHandler.Error(string.Concat("Could not initialize the UdpClient connection on port ", localPort.ToString(NumberFormatInfo.InvariantInfo), "."), exception, ErrorCode.GenericFailure);
				this.Client = null;
			}
		}

		protected override void OnClose()
		{
			base.OnClose();
			if (this.Client != null)
			{
				this.Client.Close();
				this.Client = null;
			}
		}
	}
}