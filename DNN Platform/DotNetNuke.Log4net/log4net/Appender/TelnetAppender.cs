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
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;

using log4net.Layout;
using log4net.Core;
using log4net.Util;

namespace log4net.Appender 
{
	/// <summary>
	/// Appender that allows clients to connect via Telnet to receive log messages
	/// </summary>
	/// <remarks>	
	/// <para>
	/// The TelnetAppender accepts socket connections and streams logging messages
	/// back to the client.  
	/// The output is provided in a telnet-friendly way so that a log can be monitored 
	/// over a TCP/IP socket.
	/// This allows simple remote monitoring of application logging.
	/// </para>
	/// <para>
	/// The default <see cref="Port"/> is 23 (the telnet port).
	/// </para>
	/// </remarks>
	/// <author>Keith Long</author>
	/// <author>Nicko Cadell</author>
	public class TelnetAppender : AppenderSkeleton 
	{
		private SocketHandler m_handler;
		private int m_listeningPort = 23;

		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Default constructor
		/// </para>
		/// </remarks>
		public TelnetAppender()
		{
		}

		#endregion

	    #region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the TelnetAppender class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
	    private readonly static Type declaringType = typeof(TelnetAppender);

	    #endregion Private Static Fields

		/// <summary>
		/// Gets or sets the TCP port number on which this <see cref="TelnetAppender"/> will listen for connections.
		/// </summary>
		/// <value>
		/// An integer value in the range <see cref="IPEndPoint.MinPort" /> to <see cref="IPEndPoint.MaxPort" /> 
		/// indicating the TCP port number on which this <see cref="TelnetAppender"/> will listen for connections.
		/// </value>
		/// <remarks>
		/// <para>
		/// The default value is 23 (the telnet port).
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">The value specified is less than <see cref="IPEndPoint.MinPort" /> 
		/// or greater than <see cref="IPEndPoint.MaxPort" />.</exception>
		public int Port
		{
			get
			{
				return m_listeningPort;
			}
			set
			{
				if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort)
				{
					throw log4net.Util.SystemInfo.CreateArgumentOutOfRangeException("value", (object)value,
						"The value specified for Port is less than " + 
						IPEndPoint.MinPort.ToString(NumberFormatInfo.InvariantInfo) + 
						" or greater than " + 
						IPEndPoint.MaxPort.ToString(NumberFormatInfo.InvariantInfo) + ".");
				}
				else
				{
					m_listeningPort = value;
				}
			}
		}

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// Overrides the parent method to close the socket handler
		/// </summary>
		/// <remarks>
		/// <para>
		/// Closes all the outstanding connections.
		/// </para>
		/// </remarks>
		protected override void OnClose()  
		{
			base.OnClose();

			if (m_handler != null)
			{
				m_handler.Dispose();
				m_handler = null;
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
		protected override bool RequiresLayout
		{
			get { return true; }
		}

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
		/// Create the socket handler and wait for connections
		/// </para>
		/// </remarks>
		public override void ActivateOptions() 
		{
			base.ActivateOptions();
			try 
			{
				LogLog.Debug(declaringType, "Creating SocketHandler to listen on port ["+m_listeningPort+"]");
				m_handler = new SocketHandler(m_listeningPort);
			}
			catch(Exception ex) 
			{
				LogLog.Error(declaringType, "Failed to create SocketHandler", ex);
				throw;
			}
		}

		/// <summary>
		/// Writes the logging event to each connected client.
		/// </summary>
		/// <param name="loggingEvent">The event to log.</param>
		/// <remarks>
		/// <para>
		/// Writes the logging event to each connected client.
		/// </para>
		/// </remarks>
		protected override void Append(LoggingEvent loggingEvent) 
		{
			if (m_handler != null && m_handler.HasConnections)
			{
				m_handler.Send(RenderLoggingEvent(loggingEvent));
			}
		}

		#endregion

		#region SocketHandler helper class

		/// <summary>
		/// Helper class to manage connected clients
		/// </summary>
		/// <remarks>
		/// <para>
		/// The SocketHandler class is used to accept connections from
		/// clients.  It is threaded so that clients can connect/disconnect
		/// asynchronously.
		/// </para>
		/// </remarks>
		protected class SocketHandler : IDisposable
		{			
			private const int MAX_CONNECTIONS = 20;

			private Socket m_serverSocket;
			private ArrayList m_clients = new ArrayList();

			/// <summary>
			/// Class that represents a client connected to this handler
			/// </summary>
			/// <remarks>
			/// <para>
			/// Class that represents a client connected to this handler
			/// </para>
			/// </remarks>
			protected class SocketClient : IDisposable
			{
				private Socket m_socket;
				private StreamWriter m_writer;

				/// <summary>
				/// Create this <see cref="SocketClient"/> for the specified <see cref="Socket"/>
				/// </summary>
				/// <param name="socket">the client's socket</param>
				/// <remarks>
				/// <para>
				/// Opens a stream writer on the socket.
				/// </para>
				/// </remarks>
				public SocketClient(Socket socket)
				{
					m_socket = socket;

					try
					{
						m_writer = new StreamWriter(new NetworkStream(socket));
					}
					catch
					{
						Dispose();
						throw;
					}
				}

				/// <summary>
				/// Write a string to the client
				/// </summary>
				/// <param name="message">string to send</param>
				/// <remarks>
				/// <para>
				/// Write a string to the client
				/// </para>
				/// </remarks>
				public void Send(String message)
				{
					m_writer.Write(message);
					m_writer.Flush();
				}

				#region IDisposable Members

				/// <summary>
				/// Cleanup the clients connection
				/// </summary>
				/// <remarks>
				/// <para>
				/// Close the socket connection.
				/// </para>
				/// </remarks>
				public void Dispose()
				{
					try
					{
						if (m_writer != null)
						{
							m_writer.Close();
							m_writer = null;
						}
					}
					catch { }

					if (m_socket != null)
					{
						try
						{
							m_socket.Shutdown(SocketShutdown.Both);
						}
						catch { }

						try
						{
							m_socket.Close();
						}
						catch { }

						m_socket = null;
					}
				}

				#endregion
			}
		
			/// <summary>
			/// Opens a new server port on <paramref ref="port"/>
			/// </summary>
			/// <param name="port">the local port to listen on for connections</param>
			/// <remarks>
			/// <para>
			/// Creates a socket handler on the specified local server port.
			/// </para>
			/// </remarks>
			public SocketHandler(int port)
			{
				m_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				m_serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
				m_serverSocket.Listen(5);	
				m_serverSocket.BeginAccept(new AsyncCallback(OnConnect), null);
			}

			/// <summary>
			/// Sends a string message to each of the connected clients
			/// </summary>
			/// <param name="message">the text to send</param>
			/// <remarks>
			/// <para>
			/// Sends a string message to each of the connected clients
			/// </para>
			/// </remarks>
			public void Send(String message)
			{
				ArrayList localClients = m_clients;

				foreach (SocketClient client in localClients)
				{
					try
					{
						client.Send(message);
					}
					catch (Exception)
					{
						// The client has closed the connection, remove it from our list
						client.Dispose();
						RemoveClient(client);
					}
				}
			}

			/// <summary>
			/// Add a client to the internal clients list
			/// </summary>
			/// <param name="client">client to add</param>
			private void AddClient(SocketClient client)
			{
				lock(this)
				{
					ArrayList clientsCopy = (ArrayList)m_clients.Clone();
					clientsCopy.Add(client);
					m_clients = clientsCopy;
				}
			}

			/// <summary>
			/// Remove a client from the internal clients list
			/// </summary>
			/// <param name="client">client to remove</param>
			private void RemoveClient(SocketClient client)
			{
				lock(this)
				{
					ArrayList clientsCopy = (ArrayList)m_clients.Clone();
					clientsCopy.Remove(client);
					m_clients = clientsCopy;
				}
			}

			/// <summary>
			/// Test if this handler has active connections
			/// </summary>
			/// <value>
			/// <c>true</c> if this handler has active connections
			/// </value>
			/// <remarks>
			/// <para>
			/// This property will be <c>true</c> while this handler has
			/// active connections, that is at least one connection that 
			/// the handler will attempt to send a message to.
			/// </para>
			/// </remarks>
			public bool HasConnections
			{
				get
				{
					ArrayList localClients = m_clients;

					return (localClients != null && localClients.Count > 0);
				}
			}
			
			/// <summary>
			/// Callback used to accept a connection on the server socket
			/// </summary>
			/// <param name="asyncResult">The result of the asynchronous operation</param>
			/// <remarks>
			/// <para>
			/// On connection adds to the list of connections 
			/// if there are two many open connections you will be disconnected
			/// </para>
			/// </remarks>
			private void OnConnect(IAsyncResult asyncResult)
			{
				try
				{
					// Block until a client connects
					Socket socket = m_serverSocket.EndAccept(asyncResult);

					LogLog.Debug(declaringType, "Accepting connection from ["+socket.RemoteEndPoint.ToString()+"]");
					SocketClient client = new SocketClient(socket);

					int currentActiveConnectionsCount = m_clients.Count;
					if (currentActiveConnectionsCount < MAX_CONNECTIONS) 
					{
						try
						{
							client.Send("TelnetAppender v1.0 (" + (currentActiveConnectionsCount + 1) + " active connections)\r\n\r\n");
							AddClient(client);
						}
						catch
						{
							client.Dispose();
						}
					}
					else 
					{
						client.Send("Sorry - Too many connections.\r\n");
						client.Dispose();
					}
				}
				catch
				{
				}
				finally
				{
					if (m_serverSocket != null)
					{
						m_serverSocket.BeginAccept(new AsyncCallback(OnConnect), null);
					}
				}
			}

			#region IDisposable Members

			/// <summary>
			/// Close all network connections
			/// </summary>
			/// <remarks>
			/// <para>
			/// Make sure we close all network connections
			/// </para>
			/// </remarks>
			public void Dispose()
			{
				ArrayList localClients = m_clients;

				foreach (SocketClient client in localClients)
				{
					client.Dispose();
				}
				m_clients.Clear();

				Socket localSocket = m_serverSocket;
				m_serverSocket = null;
				try 
				{
					localSocket.Shutdown(SocketShutdown.Both);
				} 
				catch 
				{ 
				}

				try
				{
					localSocket.Close();
				}
				catch 
				{ 
				}			
			}

			#endregion
		}

		#endregion
	}
}
