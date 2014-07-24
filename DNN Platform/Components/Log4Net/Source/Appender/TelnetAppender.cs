using log4net.Core;
using log4net.Util;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace log4net.Appender
{
	public class TelnetAppender : AppenderSkeleton
	{
		private TelnetAppender.SocketHandler m_handler;

		private int m_listeningPort = 23;

		private readonly static Type declaringType;

		public int Port
		{
			get
			{
				return this.m_listeningPort;
			}
			set
			{
				if (value < 0 || value > 65535)
				{
					object obj = value;
					string[] str = new string[] { "The value specified for Port is less than ", null, null, null, null };
					str[1] = 0.ToString(NumberFormatInfo.InvariantInfo);
					str[2] = " or greater than ";
					str[3] = 65535.ToString(NumberFormatInfo.InvariantInfo);
					str[4] = ".";
					throw SystemInfo.CreateArgumentOutOfRangeException("value", obj, string.Concat(str));
				}
				this.m_listeningPort = value;
			}
		}

		protected override bool RequiresLayout
		{
			get
			{
				return true;
			}
		}

		static TelnetAppender()
		{
			TelnetAppender.declaringType = typeof(TelnetAppender);
		}

		public TelnetAppender()
		{
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			try
			{
				LogLog.Debug(TelnetAppender.declaringType, string.Concat("Creating SocketHandler to listen on port [", this.m_listeningPort, "]"));
				this.m_handler = new TelnetAppender.SocketHandler(this.m_listeningPort);
			}
			catch (Exception exception)
			{
				LogLog.Error(TelnetAppender.declaringType, "Failed to create SocketHandler", exception);
				throw;
			}
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			if (this.m_handler != null && this.m_handler.HasConnections)
			{
				this.m_handler.Send(base.RenderLoggingEvent(loggingEvent));
			}
		}

		protected override void OnClose()
		{
			base.OnClose();
			if (this.m_handler != null)
			{
				this.m_handler.Dispose();
				this.m_handler = null;
			}
		}

		protected class SocketHandler : IDisposable
		{
			private const int MAX_CONNECTIONS = 20;

			private Socket m_serverSocket;

			private ArrayList m_clients;

			public bool HasConnections
			{
				get
				{
					ArrayList mClients = this.m_clients;
					if (mClients == null)
					{
						return false;
					}
					return mClients.Count > 0;
				}
			}

			public SocketHandler(int port)
			{
				this.m_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				this.m_serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
				this.m_serverSocket.Listen(5);
				this.m_serverSocket.BeginAccept(new AsyncCallback(this.OnConnect), null);
			}

			private void AddClient(TelnetAppender.SocketHandler.SocketClient client)
			{
				lock (this)
				{
					ArrayList arrayLists = (ArrayList)this.m_clients.Clone();
					arrayLists.Add(client);
					this.m_clients = arrayLists;
				}
			}

			public void Dispose()
			{
				foreach (TelnetAppender.SocketHandler.SocketClient mClient in this.m_clients)
				{
					mClient.Dispose();
				}
				this.m_clients.Clear();
				Socket mServerSocket = this.m_serverSocket;
				this.m_serverSocket = null;
				try
				{
					mServerSocket.Shutdown(SocketShutdown.Both);
				}
				catch
				{
				}
				try
				{
					mServerSocket.Close();
				}
				catch
				{
				}
			}

			private void OnConnect(IAsyncResult asyncResult)
			{
				try
				{
					try
					{
						Socket socket = this.m_serverSocket.EndAccept(asyncResult);
						LogLog.Debug(TelnetAppender.declaringType, string.Concat("Accepting connection from [", socket.RemoteEndPoint.ToString(), "]"));
						TelnetAppender.SocketHandler.SocketClient socketClient = new TelnetAppender.SocketHandler.SocketClient(socket);
						int count = this.m_clients.Count;
						if (count >= 20)
						{
							socketClient.Send("Sorry - Too many connections.\r\n");
							socketClient.Dispose();
						}
						else
						{
							try
							{
								socketClient.Send(string.Concat("TelnetAppender v1.0 (", count + 1, " active connections)\r\n\r\n"));
								this.AddClient(socketClient);
							}
							catch
							{
								socketClient.Dispose();
							}
						}
					}
					catch
					{
					}
				}
				finally
				{
					if (this.m_serverSocket != null)
					{
						this.m_serverSocket.BeginAccept(new AsyncCallback(this.OnConnect), null);
					}
				}
			}

			private void RemoveClient(TelnetAppender.SocketHandler.SocketClient client)
			{
				lock (this)
				{
					ArrayList arrayLists = (ArrayList)this.m_clients.Clone();
					arrayLists.Remove(client);
					this.m_clients = arrayLists;
				}
			}

			public void Send(string message)
			{
				foreach (TelnetAppender.SocketHandler.SocketClient mClient in this.m_clients)
				{
					try
					{
						mClient.Send(message);
					}
					catch (Exception exception)
					{
						mClient.Dispose();
						this.RemoveClient(mClient);
					}
				}
			}

			protected class SocketClient : IDisposable
			{
				private Socket m_socket;

				private StreamWriter m_writer;

				public SocketClient(Socket socket)
				{
					this.m_socket = socket;
					try
					{
						this.m_writer = new StreamWriter(new NetworkStream(socket));
					}
					catch
					{
						this.Dispose();
						throw;
					}
				}

				public void Dispose()
				{
					try
					{
						if (this.m_writer != null)
						{
							this.m_writer.Close();
							this.m_writer = null;
						}
					}
					catch
					{
					}
					if (this.m_socket != null)
					{
						try
						{
							this.m_socket.Shutdown(SocketShutdown.Both);
						}
						catch
						{
						}
						try
						{
							this.m_socket.Close();
						}
						catch
						{
						}
						this.m_socket = null;
					}
				}

				public void Send(string message)
				{
					this.m_writer.Write(message);
					this.m_writer.Flush();
				}
			}
		}
	}
}