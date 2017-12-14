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
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

using log4net.Layout;
using log4net.Core;
using log4net.Util;

namespace log4net.Appender 
{
	/// <summary>
	/// Sends logging events as connectionless UDP datagrams to a remote host or a 
	/// multicast group using an <see cref="UdpClient" />.
	/// </summary>
	/// <remarks>
	/// <para>
	/// UDP guarantees neither that messages arrive, nor that they arrive in the correct order.
	/// </para>
	/// <para>
	/// To view the logging results, a custom application can be developed that listens for logging 
	/// events.
	/// </para>
	/// <para>
	/// When decoding events send via this appender remember to use the same encoding
	/// to decode the events as was used to send the events. See the <see cref="Encoding"/>
	/// property to specify the encoding to use.
	/// </para>
	/// </remarks>
	/// <example>
	/// This example shows how to log receive logging events that are sent 
	/// on IP address 244.0.0.1 and port 8080 to the console. The event is 
	/// encoded in the packet as a unicode string and it is decoded as such. 
	/// <code lang="C#">
	/// IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
	/// UdpClient udpClient;
	/// byte[] buffer;
	/// string loggingEvent;
	/// 
	/// try 
	/// {
	///     udpClient = new UdpClient(8080);
	///     
	///     while(true) 
	///     {
	///         buffer = udpClient.Receive(ref remoteEndPoint);
	///         loggingEvent = System.Text.Encoding.Unicode.GetString(buffer);
	///         Console.WriteLine(loggingEvent);
	///     }
	/// } 
	/// catch(Exception e) 
	/// {
	///     Console.WriteLine(e.ToString());
	/// }
	/// </code>
	/// <code lang="Visual Basic">
	/// Dim remoteEndPoint as IPEndPoint
	/// Dim udpClient as UdpClient
	/// Dim buffer as Byte()
	/// Dim loggingEvent as String
	/// 
	/// Try 
	///     remoteEndPoint = new IPEndPoint(IPAddress.Any, 0)
	///     udpClient = new UdpClient(8080)
	///
	///     While True
	///         buffer = udpClient.Receive(ByRef remoteEndPoint)
	///         loggingEvent = System.Text.Encoding.Unicode.GetString(buffer)
	///         Console.WriteLine(loggingEvent)
	///     Wend
	/// Catch e As Exception
	///     Console.WriteLine(e.ToString())
	/// End Try
	/// </code>
	/// <para>
	/// An example configuration section to log information using this appender to the 
	/// IP 224.0.0.1 on port 8080:
	/// </para>
	/// <code lang="XML" escaped="true">
	/// <appender name="UdpAppender" type="log4net.Appender.UdpAppender">
	///     <remoteAddress value="224.0.0.1" />
	///     <remotePort value="8080" />
	///     <layout type="log4net.Layout.PatternLayout" value="%-5level %logger [%ndc] - %message%newline" />
	/// </appender>
	/// </code>
	/// </example>
	/// <author>Gert Driesen</author>
	/// <author>Nicko Cadell</author>
	public class UdpAppender : AppenderSkeleton
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="UdpAppender" /> class.
		/// </summary>
		/// <remarks>
		/// The default constructor initializes all fields to their default values.
		/// </remarks>
		public UdpAppender() 
		{
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets the IP address of the remote host or multicast group to which
		/// the underlying <see cref="UdpClient" /> should sent the logging event.
		/// </summary>
		/// <value>
		/// The IP address of the remote host or multicast group to which the logging event 
		/// will be sent.
		/// </value>
		/// <remarks>
		/// <para>
		/// Multicast addresses are identified by IP class <b>D</b> addresses (in the range 224.0.0.0 to
		/// 239.255.255.255).  Multicast packets can pass across different networks through routers, so
		/// it is possible to use multicasts in an Internet scenario as long as your network provider 
		/// supports multicasting.
		/// </para>
		/// <para>
		/// Hosts that want to receive particular multicast messages must register their interest by joining
		/// the multicast group.  Multicast messages are not sent to networks where no host has joined
		/// the multicast group.  Class <b>D</b> IP addresses are used for multicast groups, to differentiate
		/// them from normal host addresses, allowing nodes to easily detect if a message is of interest.
		/// </para>
		/// <para>
		/// Static multicast addresses that are needed globally are assigned by IANA.  A few examples are listed in the table below:
		/// </para>
		/// <para>
		/// <list type="table">
		///     <listheader>
		///         <term>IP Address</term>
		///         <description>Description</description>
		///     </listheader>
		///     <item>
		///         <term>224.0.0.1</term>
		///         <description>
		///             <para>
		///             Sends a message to all system on the subnet.
		///             </para>
		///         </description>
		///     </item>
		///     <item>
		///         <term>224.0.0.2</term>
		///         <description>
		///             <para>
		///             Sends a message to all routers on the subnet.
		///             </para>
		///         </description>
		///     </item>
		///     <item>
		///         <term>224.0.0.12</term>
		///         <description>
		///             <para>
		///             The DHCP server answers messages on the IP address 224.0.0.12, but only on a subnet.
		///             </para>
		///         </description>
		///     </item>
		/// </list>
		/// </para>
		/// <para>
		/// A complete list of actually reserved multicast addresses and their owners in the ranges
		/// defined by RFC 3171 can be found at the <A href="http://www.iana.org/assignments/multicast-addresses">IANA web site</A>. 
		/// </para>
		/// <para>
		/// The address range 239.0.0.0 to 239.255.255.255 is reserved for administrative scope-relative 
		/// addresses.  These addresses can be reused with other local groups.  Routers are typically 
		/// configured with filters to prevent multicast traffic in this range from flowing outside
		/// of the local network.
		/// </para>
		/// </remarks>
		public IPAddress RemoteAddress
		{
			get { return m_remoteAddress; }
			set { m_remoteAddress = value; }
		}

		/// <summary>
		/// Gets or sets the TCP port number of the remote host or multicast group to which 
		/// the underlying <see cref="UdpClient" /> should sent the logging event.
		/// </summary>
		/// <value>
		/// An integer value in the range <see cref="IPEndPoint.MinPort" /> to <see cref="IPEndPoint.MaxPort" /> 
		/// indicating the TCP port number of the remote host or multicast group to which the logging event 
		/// will be sent.
		/// </value>
		/// <remarks>
		/// The underlying <see cref="UdpClient" /> will send messages to this TCP port number
		/// on the remote host or multicast group.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">The value specified is less than <see cref="IPEndPoint.MinPort" /> or greater than <see cref="IPEndPoint.MaxPort" />.</exception>
		public int RemotePort
		{
			get { return m_remotePort; }
			set 
			{
				if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort) 
				{
					throw log4net.Util.SystemInfo.CreateArgumentOutOfRangeException("value", (object)value,
						"The value specified is less than " + 
						IPEndPoint.MinPort.ToString(NumberFormatInfo.InvariantInfo) + 
						" or greater than " + 
						IPEndPoint.MaxPort.ToString(NumberFormatInfo.InvariantInfo) + ".");
				} 
				else 
				{
					m_remotePort = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the TCP port number from which the underlying <see cref="UdpClient" /> will communicate.
		/// </summary>
		/// <value>
		/// An integer value in the range <see cref="IPEndPoint.MinPort" /> to <see cref="IPEndPoint.MaxPort" /> 
		/// indicating the TCP port number from which the underlying <see cref="UdpClient" /> will communicate.
		/// </value>
		/// <remarks>
		/// <para>
		/// The underlying <see cref="UdpClient" /> will bind to this port for sending messages.
		/// </para>
		/// <para>
		/// Setting the value to 0 (the default) will cause the udp client not to bind to
		/// a local port.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">The value specified is less than <see cref="IPEndPoint.MinPort" /> or greater than <see cref="IPEndPoint.MaxPort" />.</exception>
		public int LocalPort
		{
			get { return m_localPort; }
			set 
			{
				if (value != 0 && (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort))
				{
					throw log4net.Util.SystemInfo.CreateArgumentOutOfRangeException("value", (object)value,
						"The value specified is less than " + 
						IPEndPoint.MinPort.ToString(NumberFormatInfo.InvariantInfo) + 
						" or greater than " + 
						IPEndPoint.MaxPort.ToString(NumberFormatInfo.InvariantInfo) + ".");
				} 
				else 
				{
					m_localPort = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets <see cref="Encoding"/> used to write the packets.
		/// </summary>
		/// <value>
		/// The <see cref="Encoding"/> used to write the packets.
		/// </value>
		/// <remarks>
		/// <para>
		/// The <see cref="Encoding"/> used to write the packets.
		/// </para>
		/// </remarks>
		public Encoding Encoding
		{
			get { return m_encoding; }
			set { m_encoding = value; }
		}

		#endregion Public Instance Properties

		#region Protected Instance Properties

		/// <summary>
		/// Gets or sets the underlying <see cref="UdpClient" />.
		/// </summary>
		/// <value>
		/// The underlying <see cref="UdpClient" />.
		/// </value>
		/// <remarks>
		/// <see cref="UdpAppender" /> creates a <see cref="UdpClient" /> to send logging events 
		/// over a network.  Classes deriving from <see cref="UdpAppender" /> can use this
		/// property to get or set this <see cref="UdpClient" />.  Use the underlying <see cref="UdpClient" />
		/// returned from <see cref="Client" /> if you require access beyond that which 
		/// <see cref="UdpAppender" /> provides.
		/// </remarks>
		protected UdpClient Client
		{
			get { return this.m_client; }
			set { this.m_client = value; }
		}

		/// <summary>
		/// Gets or sets the cached remote endpoint to which the logging events should be sent.
		/// </summary>
		/// <value>
		/// The cached remote endpoint to which the logging events will be sent.
		/// </value>
		/// <remarks>
		/// The <see cref="ActivateOptions" /> method will initialize the remote endpoint 
		/// with the values of the <see cref="RemoteAddress" /> and <see cref="RemotePort"/>
		/// properties.
		/// </remarks>
		protected IPEndPoint RemoteEndPoint
		{
			get { return this.m_remoteEndPoint; }
			set { this.m_remoteEndPoint = value; }
		}

		#endregion Protected Instance Properties

		#region Implementation of IOptionHandler

		/// <summary>
		/// Initialize the appender based on the options set.
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
		/// <para>
		/// The appender will be ignored if no <see cref="RemoteAddress" /> was specified or 
		/// an invalid remote or local TCP port number was specified.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">The required property <see cref="RemoteAddress" /> was not specified.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The TCP port number assigned to <see cref="LocalPort" /> or <see cref="RemotePort" /> is less than <see cref="IPEndPoint.MinPort" /> or greater than <see cref="IPEndPoint.MaxPort" />.</exception>
		public override void ActivateOptions()
		{
			base.ActivateOptions();

			if (this.RemoteAddress == null) 
			{
				throw new ArgumentNullException("The required property 'Address' was not specified.");
			} 
			else if (this.RemotePort < IPEndPoint.MinPort || this.RemotePort > IPEndPoint.MaxPort) 
			{
				throw log4net.Util.SystemInfo.CreateArgumentOutOfRangeException("this.RemotePort", (object)this.RemotePort,
					"The RemotePort is less than " + 
					IPEndPoint.MinPort.ToString(NumberFormatInfo.InvariantInfo) + 
					" or greater than " + 
					IPEndPoint.MaxPort.ToString(NumberFormatInfo.InvariantInfo) + ".");
			} 
			else if (this.LocalPort != 0 && (this.LocalPort < IPEndPoint.MinPort || this.LocalPort > IPEndPoint.MaxPort))
			{
				throw log4net.Util.SystemInfo.CreateArgumentOutOfRangeException("this.LocalPort", (object)this.LocalPort,
					"The LocalPort is less than " + 
					IPEndPoint.MinPort.ToString(NumberFormatInfo.InvariantInfo) + 
					" or greater than " + 
					IPEndPoint.MaxPort.ToString(NumberFormatInfo.InvariantInfo) + ".");
			} 
			else 
			{
				this.RemoteEndPoint = new IPEndPoint(this.RemoteAddress, this.RemotePort);
				this.InitializeClientConnection();
			}
		}

		#endregion

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// This method is called by the <see cref="M:AppenderSkeleton.DoAppend(LoggingEvent)"/> method.
		/// </summary>
		/// <param name="loggingEvent">The event to log.</param>
		/// <remarks>
		/// <para>
		/// Sends the event using an UDP datagram.
		/// </para>
		/// <para>
		/// Exceptions are passed to the <see cref="AppenderSkeleton.ErrorHandler"/>.
		/// </para>
		/// </remarks>
		protected override void Append(LoggingEvent loggingEvent) 
		{
			try 
			{
				Byte [] buffer = m_encoding.GetBytes(RenderLoggingEvent(loggingEvent).ToCharArray());
#if NETSTANDARD1_3
				Client.SendAsync(buffer, buffer.Length, RemoteEndPoint).Wait();
#else
				this.Client.Send(buffer, buffer.Length, this.RemoteEndPoint);
#endif
			} 
			catch (Exception ex) 
			{
				ErrorHandler.Error(
					"Unable to send logging event to remote host " + 
					this.RemoteAddress.ToString() + 
					" on port " + 
					this.RemotePort + ".", 
					ex, 
					ErrorCode.WriteFailure);
			}
		}

		/// <summary>
		/// This appender requires a <see cref="Layout"/> to be set.
		/// </summary>
		/// <value><c>true</c></value>
		/// <remarks>
		/// <para>
		/// This appender requires a <see cref="Layout"/> to be set.
		/// </para>
		/// </remarks>
		override protected bool RequiresLayout
		{
			get { return true; }
		}

		/// <summary>
		/// Closes the UDP connection and releases all resources associated with 
		/// this <see cref="UdpAppender" /> instance.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Disables the underlying <see cref="UdpClient" /> and releases all managed 
		/// and unmanaged resources associated with the <see cref="UdpAppender" />.
		/// </para>
		/// </remarks>
		override protected void OnClose() 
		{
			base.OnClose();

			if (this.Client != null) 
			{
				this.Client.Close();
				this.Client = null;
			}
		}

		#endregion Override implementation of AppenderSkeleton

		#region Protected Instance Methods

		/// <summary>
		/// Initializes the underlying  <see cref="UdpClient" /> connection.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The underlying <see cref="UdpClient"/> is initialized and binds to the 
		/// port number from which you intend to communicate.
		/// </para>
		/// <para>
		/// Exceptions are passed to the <see cref="AppenderSkeleton.ErrorHandler"/>.
		/// </para>
		/// </remarks>
		protected virtual void InitializeClientConnection() 
		{
			try 
			{
				if (this.LocalPort == 0)
				{
#if NETCF || NET_1_0 || SSCLI_1_0 || CLI_1_0
					this.Client = new UdpClient();
#else
					this.Client = new UdpClient(RemoteAddress.AddressFamily);
#endif
				}
				else
				{
#if NETCF || NET_1_0 || SSCLI_1_0 || CLI_1_0
					this.Client = new UdpClient(this.LocalPort);
#else
					this.Client = new UdpClient(this.LocalPort, RemoteAddress.AddressFamily);
#endif
				}
			} 
			catch (Exception ex) 
			{
				ErrorHandler.Error(
					"Could not initialize the UdpClient connection on port " + 
					this.LocalPort.ToString(NumberFormatInfo.InvariantInfo) + ".", 
					ex, 
					ErrorCode.GenericFailure);

				this.Client = null;
			}
		}

		#endregion Protected Instance Methods

		#region Private Instance Fields

		/// <summary>
		/// The IP address of the remote host or multicast group to which 
		/// the logging event will be sent.
		/// </summary>
		private IPAddress m_remoteAddress;
		
		/// <summary>
		/// The TCP port number of the remote host or multicast group to 
		/// which the logging event will be sent.
		/// </summary>
		private int m_remotePort;

		/// <summary>
		/// The cached remote endpoint to which the logging events will be sent.
		/// </summary>
		private IPEndPoint m_remoteEndPoint;

		/// <summary>
		/// The TCP port number from which the <see cref="UdpClient" /> will communicate.
		/// </summary>
		private int m_localPort;

		/// <summary>
		/// The <see cref="UdpClient" /> instance that will be used for sending the 
		/// logging events.
		/// </summary>
		private UdpClient m_client;

		/// <summary>
		/// The encoding to use for the packet.
		/// </summary>
#if NETSTANDARD1_3
		private Encoding m_encoding = Encoding.Unicode;
#else
		private Encoding m_encoding = Encoding.Default;
#endif

		#endregion Private Instance Fields
	}
}
