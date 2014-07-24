using System;
using System.Globalization;
using System.IO;

namespace log4net.Util
{
	public abstract class TextWriterAdapter : TextWriter
	{
		private TextWriter m_writer;

		public override System.Text.Encoding Encoding
		{
			get
			{
				return this.m_writer.Encoding;
			}
		}

		public override IFormatProvider FormatProvider
		{
			get
			{
				return this.m_writer.FormatProvider;
			}
		}

		public override string NewLine
		{
			get
			{
				return this.m_writer.NewLine;
			}
			set
			{
				this.m_writer.NewLine = value;
			}
		}

		protected TextWriter Writer
		{
			get
			{
				return this.m_writer;
			}
			set
			{
				this.m_writer = value;
			}
		}

		protected TextWriterAdapter(TextWriter writer) : base(CultureInfo.InvariantCulture)
		{
			this.m_writer = writer;
		}

		public override void Close()
		{
			this.m_writer.Close();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((IDisposable)this.m_writer).Dispose();
			}
		}

		public override void Flush()
		{
			this.m_writer.Flush();
		}

		public override void Write(char value)
		{
			this.m_writer.Write(value);
		}

		public override void Write(char[] buffer, int index, int count)
		{
			this.m_writer.Write(buffer, index, count);
		}

		public override void Write(string value)
		{
			this.m_writer.Write(value);
		}
	}
}