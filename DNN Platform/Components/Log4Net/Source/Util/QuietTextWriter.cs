using log4net.Core;
using System;
using System.IO;

namespace log4net.Util
{
	public class QuietTextWriter : TextWriterAdapter
	{
		private IErrorHandler m_errorHandler;

		private bool m_closed;

		public bool Closed
		{
			get
			{
				return this.m_closed;
			}
		}

		public IErrorHandler ErrorHandler
		{
			get
			{
				return this.m_errorHandler;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				this.m_errorHandler = value;
			}
		}

		public QuietTextWriter(TextWriter writer, IErrorHandler errorHandler) : base(writer)
		{
			if (errorHandler == null)
			{
				throw new ArgumentNullException("errorHandler");
			}
			this.ErrorHandler = errorHandler;
		}

		public override void Close()
		{
			this.m_closed = true;
			base.Close();
		}

		public override void Write(char value)
		{
			try
			{
				base.Write(value);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.m_errorHandler.Error(string.Concat("Failed to write [", value, "]."), exception, ErrorCode.WriteFailure);
			}
		}

		public override void Write(char[] buffer, int index, int count)
		{
			try
			{
				base.Write(buffer, index, count);
			}
			catch (Exception exception)
			{
				this.m_errorHandler.Error("Failed to write buffer.", exception, ErrorCode.WriteFailure);
			}
		}

		public override void Write(string value)
		{
			try
			{
				base.Write(value);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.m_errorHandler.Error(string.Concat("Failed to write [", value, "]."), exception, ErrorCode.WriteFailure);
			}
		}
	}
}