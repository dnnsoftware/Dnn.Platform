using log4net.Core;
using System;
using System.IO;

namespace log4net.Util
{
	public class CountingQuietTextWriter : QuietTextWriter
	{
		private long m_countBytes;

		public long Count
		{
			get
			{
				return this.m_countBytes;
			}
			set
			{
				this.m_countBytes = value;
			}
		}

		public CountingQuietTextWriter(TextWriter writer, IErrorHandler errorHandler) : base(writer, errorHandler)
		{
			this.m_countBytes = (long)0;
		}

		public override void Write(char value)
		{
			try
			{
				base.Write(value);
				CountingQuietTextWriter byteCount = this;
				long mCountBytes = byteCount.m_countBytes;
				System.Text.Encoding encoding = this.Encoding;
				char[] chrArray = new char[] { value };
				byteCount.m_countBytes = mCountBytes + (long)encoding.GetByteCount(chrArray);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.ErrorHandler.Error(string.Concat("Failed to write [", value, "]."), exception, ErrorCode.WriteFailure);
			}
		}

		public override void Write(char[] buffer, int index, int count)
		{
			if (count > 0)
			{
				try
				{
					base.Write(buffer, index, count);
					CountingQuietTextWriter mCountBytes = this;
					mCountBytes.m_countBytes = mCountBytes.m_countBytes + (long)this.Encoding.GetByteCount(buffer, index, count);
				}
				catch (Exception exception)
				{
					base.ErrorHandler.Error("Failed to write buffer.", exception, ErrorCode.WriteFailure);
				}
			}
		}

		public override void Write(string str)
		{
			if (str != null && str.Length > 0)
			{
				try
				{
					base.Write(str);
					CountingQuietTextWriter mCountBytes = this;
					mCountBytes.m_countBytes = mCountBytes.m_countBytes + (long)this.Encoding.GetByteCount(str);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					base.ErrorHandler.Error(string.Concat("Failed to write [", str, "]."), exception, ErrorCode.WriteFailure);
				}
			}
		}
	}
}