using log4net.Core;
using log4net.Layout;
using log4net.Util;
using System;
using System.IO;
using System.Threading;

namespace log4net.Appender
{
	public class FileAppender : TextWriterAppender
	{
		private bool m_appendToFile = true;

		private string m_fileName;

		private System.Text.Encoding m_encoding = System.Text.Encoding.Default;

		private log4net.Core.SecurityContext m_securityContext;

		private FileAppender.LockingStream m_stream;

		private FileAppender.LockingModelBase m_lockingModel = new FileAppender.ExclusiveLock();

		private readonly static Type declaringType;

		public bool AppendToFile
		{
			get
			{
				return this.m_appendToFile;
			}
			set
			{
				this.m_appendToFile = value;
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

		public virtual string File
		{
			get
			{
				return this.m_fileName;
			}
			set
			{
				this.m_fileName = value;
			}
		}

		public FileAppender.LockingModelBase LockingModel
		{
			get
			{
				return this.m_lockingModel;
			}
			set
			{
				this.m_lockingModel = value;
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

		static FileAppender()
		{
			FileAppender.declaringType = typeof(FileAppender);
		}

		public FileAppender()
		{
		}

		[Obsolete("Instead use the default constructor and set the Layout, File & AppendToFile properties")]
		public FileAppender(ILayout layout, string filename, bool append)
		{
			this.Layout = layout;
			this.File = filename;
			this.AppendToFile = append;
			this.ActivateOptions();
		}

		[Obsolete("Instead use the default constructor and set the Layout & File properties")]
		public FileAppender(ILayout layout, string filename) : this(layout, filename, true)
		{
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			if (this.m_securityContext == null)
			{
				this.m_securityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
			}
			if (this.m_lockingModel == null)
			{
				this.m_lockingModel = new FileAppender.ExclusiveLock();
			}
			this.m_lockingModel.CurrentAppender = this;
			using (IDisposable disposable = this.SecurityContext.Impersonate(this))
			{
				this.m_fileName = FileAppender.ConvertToFullPath(this.m_fileName.Trim());
			}
			if (this.m_fileName != null)
			{
				this.SafeOpenFile(this.m_fileName, this.m_appendToFile);
				return;
			}
			LogLog.Warn(FileAppender.declaringType, string.Concat("FileAppender: File option not set for appender [", base.Name, "]."));
			LogLog.Warn(FileAppender.declaringType, "FileAppender: Are you using FileAppender instead of ConsoleAppender?");
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			if (this.m_stream.AcquireLock())
			{
				try
				{
					base.Append(loggingEvent);
				}
				finally
				{
					this.m_stream.ReleaseLock();
				}
			}
		}

		protected override void Append(LoggingEvent[] loggingEvents)
		{
			if (this.m_stream.AcquireLock())
			{
				try
				{
					base.Append(loggingEvents);
				}
				finally
				{
					this.m_stream.ReleaseLock();
				}
			}
		}

		protected void CloseFile()
		{
			this.WriteFooterAndCloseWriter();
		}

		protected override void CloseWriter()
		{
			if (this.m_stream != null)
			{
				this.m_stream.AcquireLock();
				try
				{
					base.CloseWriter();
				}
				finally
				{
					this.m_stream.ReleaseLock();
				}
			}
		}

		protected static string ConvertToFullPath(string path)
		{
			return SystemInfo.ConvertToFullPath(path);
		}

		protected virtual void OpenFile(string fileName, bool append)
		{
			if (LogLog.IsErrorEnabled)
			{
				bool flag = false;
				using (IDisposable disposable = this.SecurityContext.Impersonate(this))
				{
					flag = Path.IsPathRooted(fileName);
				}
				if (!flag)
				{
					LogLog.Error(FileAppender.declaringType, string.Concat("INTERNAL ERROR. OpenFile(", fileName, "): File name is not fully qualified."));
				}
			}
			lock (this)
			{
				this.Reset();
				Type type = FileAppender.declaringType;
				object[] objArray = new object[] { "Opening file for writing [", fileName, "] append [", append, "]" };
				LogLog.Debug(type, string.Concat(objArray));
				this.m_fileName = fileName;
				this.m_appendToFile = append;
				this.LockingModel.CurrentAppender = this;
				this.LockingModel.OpenFile(fileName, append, this.m_encoding);
				this.m_stream = new FileAppender.LockingStream(this.LockingModel);
				if (this.m_stream != null)
				{
					this.m_stream.AcquireLock();
					try
					{
						this.SetQWForFiles(new StreamWriter(this.m_stream, this.m_encoding));
					}
					finally
					{
						this.m_stream.ReleaseLock();
					}
				}
				this.WriteHeader();
			}
		}

		protected override void PrepareWriter()
		{
			this.SafeOpenFile(this.m_fileName, this.m_appendToFile);
		}

		protected override void Reset()
		{
			base.Reset();
			this.m_fileName = null;
		}

		protected virtual void SafeOpenFile(string fileName, bool append)
		{
			try
			{
				this.OpenFile(fileName, append);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				IErrorHandler errorHandler = this.ErrorHandler;
				object[] objArray = new object[] { "OpenFile(", fileName, ",", append, ") call failed." };
				errorHandler.Error(string.Concat(objArray), exception, ErrorCode.FileOpenFailure);
			}
		}

		protected virtual void SetQWForFiles(Stream fileStream)
		{
			this.SetQWForFiles(new StreamWriter(fileStream, this.m_encoding));
		}

		protected virtual void SetQWForFiles(TextWriter writer)
		{
			base.QuietWriter = new QuietTextWriter(writer, this.ErrorHandler);
		}

		protected override void WriteFooter()
		{
			if (this.m_stream != null)
			{
				this.m_stream.AcquireLock();
				try
				{
					base.WriteFooter();
				}
				finally
				{
					this.m_stream.ReleaseLock();
				}
			}
		}

		protected override void WriteHeader()
		{
			if (this.m_stream != null && this.m_stream.AcquireLock())
			{
				try
				{
					base.WriteHeader();
				}
				finally
				{
					this.m_stream.ReleaseLock();
				}
			}
		}

		public class ExclusiveLock : FileAppender.LockingModelBase
		{
			private Stream m_stream;

			public ExclusiveLock()
			{
			}

			public override Stream AcquireLock()
			{
				return this.m_stream;
			}

			public override void CloseFile()
			{
				base.CloseStream(this.m_stream);
				this.m_stream = null;
			}

			public override void OpenFile(string filename, bool append, System.Text.Encoding encoding)
			{
				try
				{
					this.m_stream = base.CreateStream(filename, append, FileShare.Read);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					base.CurrentAppender.ErrorHandler.Error(string.Concat("Unable to acquire lock on file ", filename, ". ", exception.Message));
				}
			}

			public override void ReleaseLock()
			{
			}
		}

		public abstract class LockingModelBase
		{
			private FileAppender m_appender;

			public FileAppender CurrentAppender
			{
				get
				{
					return this.m_appender;
				}
				set
				{
					this.m_appender = value;
				}
			}

			protected LockingModelBase()
			{
			}

			public abstract Stream AcquireLock();

			public abstract void CloseFile();

			protected void CloseStream(Stream stream)
			{
				using (IDisposable disposable = this.CurrentAppender.SecurityContext.Impersonate(this))
				{
					stream.Close();
				}
			}

			protected Stream CreateStream(string filename, bool append, FileShare fileShare)
			{
				Stream fileStream;
				using (IDisposable disposable = this.CurrentAppender.SecurityContext.Impersonate(this))
				{
					string directoryName = Path.GetDirectoryName(filename);
					if (!Directory.Exists(directoryName))
					{
						Directory.CreateDirectory(directoryName);
					}
					fileStream = new FileStream(filename, (append ? FileMode.Append : FileMode.Create), FileAccess.Write, FileShare.Read);
				}
				return fileStream;
			}

			public abstract void OpenFile(string filename, bool append, System.Text.Encoding encoding);

			public abstract void ReleaseLock();
		}

		private sealed class LockingStream : Stream, IDisposable
		{
			private Stream m_realStream;

			private FileAppender.LockingModelBase m_lockingModel;

			private int m_readTotal;

			private int m_lockLevel;

			public override bool CanRead
			{
				get
				{
					return false;
				}
			}

			public override bool CanSeek
			{
				get
				{
					this.AssertLocked();
					return this.m_realStream.CanSeek;
				}
			}

			public override bool CanWrite
			{
				get
				{
					this.AssertLocked();
					return this.m_realStream.CanWrite;
				}
			}

			public override long Length
			{
				get
				{
					this.AssertLocked();
					return this.m_realStream.Length;
				}
			}

			public override long Position
			{
				get
				{
					this.AssertLocked();
					return this.m_realStream.Position;
				}
				set
				{
					this.AssertLocked();
					this.m_realStream.Position = value;
				}
			}

			public LockingStream(FileAppender.LockingModelBase locking)
			{
				if (locking == null)
				{
					throw new ArgumentException("Locking model may not be null", "locking");
				}
				this.m_lockingModel = locking;
			}

			public bool AcquireLock()
			{
				bool flag = false;
				lock (this)
				{
					if (this.m_lockLevel == 0)
					{
						this.m_realStream = this.m_lockingModel.AcquireLock();
					}
					if (this.m_realStream != null)
					{
						FileAppender.LockingStream mLockLevel = this;
						mLockLevel.m_lockLevel = mLockLevel.m_lockLevel + 1;
						flag = true;
					}
				}
				return flag;
			}

			private void AssertLocked()
			{
				if (this.m_realStream == null)
				{
					throw new FileAppender.LockingStream.LockStateException("The file is not currently locked");
				}
			}

			public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
			{
				this.AssertLocked();
				IAsyncResult asyncResult = this.m_realStream.BeginRead(buffer, offset, count, callback, state);
				this.m_readTotal = this.EndRead(asyncResult);
				return asyncResult;
			}

			public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
			{
				this.AssertLocked();
				IAsyncResult asyncResult = this.m_realStream.BeginWrite(buffer, offset, count, callback, state);
				this.EndWrite(asyncResult);
				return asyncResult;
			}

			public override void Close()
			{
				this.m_lockingModel.CloseFile();
			}

			public override int EndRead(IAsyncResult asyncResult)
			{
				this.AssertLocked();
				return this.m_readTotal;
			}

			public override void EndWrite(IAsyncResult asyncResult)
			{
			}

			public override void Flush()
			{
				this.AssertLocked();
				this.m_realStream.Flush();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				return this.m_realStream.Read(buffer, offset, count);
			}

			public override int ReadByte()
			{
				return this.m_realStream.ReadByte();
			}

			public void ReleaseLock()
			{
				lock (this)
				{
					FileAppender.LockingStream mLockLevel = this;
					mLockLevel.m_lockLevel = mLockLevel.m_lockLevel - 1;
					if (this.m_lockLevel == 0)
					{
						this.m_lockingModel.ReleaseLock();
						this.m_realStream = null;
					}
				}
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				this.AssertLocked();
				return this.m_realStream.Seek(offset, origin);
			}

			public override void SetLength(long value)
			{
				this.AssertLocked();
				this.m_realStream.SetLength(value);
			}

			void System.IDisposable.Dispose()
			{
				this.Close();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				this.AssertLocked();
				this.m_realStream.Write(buffer, offset, count);
			}

			public override void WriteByte(byte value)
			{
				this.AssertLocked();
				this.m_realStream.WriteByte(value);
			}

			public sealed class LockStateException : LogException
			{
				public LockStateException(string message) : base(message)
				{
				}
			}
		}

		public class MinimalLock : FileAppender.LockingModelBase
		{
			private string m_filename;

			private bool m_append;

			private Stream m_stream;

			public MinimalLock()
			{
			}

			public override Stream AcquireLock()
			{
				if (this.m_stream == null)
				{
					try
					{
						this.m_stream = base.CreateStream(this.m_filename, this.m_append, FileShare.Read);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						base.CurrentAppender.ErrorHandler.Error(string.Concat("Unable to acquire lock on file ", this.m_filename, ". ", exception.Message));
					}
				}
				return this.m_stream;
			}

			public override void CloseFile()
			{
			}

			public override void OpenFile(string filename, bool append, System.Text.Encoding encoding)
			{
				this.m_filename = filename;
				this.m_append = append;
			}

			public override void ReleaseLock()
			{
				base.CloseStream(this.m_stream);
				this.m_stream = null;
			}
		}

		public class MutexLock : FileAppender.LockingModelBase
		{
			private Mutex m_mutex;

			private bool m_mutexClosed;

			private Stream m_stream;

			public MutexLock()
			{
			}

			public override Stream AcquireLock()
			{
				this.m_mutex.WaitOne();
				if (this.m_stream.CanSeek)
				{
					this.m_stream.Seek((long)0, SeekOrigin.End);
				}
				return this.m_stream;
			}

			public override void CloseFile()
			{
				base.CloseStream(this.m_stream);
				this.m_stream = null;
				this.m_mutex.ReleaseMutex();
				this.m_mutex.Close();
				this.m_mutexClosed = true;
			}

			public override void OpenFile(string filename, bool append, System.Text.Encoding encoding)
			{
				try
				{
					this.m_stream = base.CreateStream(filename, append, FileShare.ReadWrite);
					string str = filename.Replace("\\", "_").Replace(":", "_").Replace("/", "_");
					this.m_mutex = new Mutex(false, str);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					base.CurrentAppender.ErrorHandler.Error(string.Concat("Unable to acquire lock on file ", filename, ". ", exception.Message));
				}
			}

			public override void ReleaseLock()
			{
				if (!this.m_mutexClosed)
				{
					this.m_mutex.ReleaseMutex();
				}
			}
		}
	}
}