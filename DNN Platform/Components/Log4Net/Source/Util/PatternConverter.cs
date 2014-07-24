using log4net.Repository;

using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;

namespace log4net.Util
{
	public abstract class PatternConverter
	{
		private const int c_renderBufferSize = 256;

		private const int c_renderBufferMaxCapacity = 1024;

		private readonly static string[] SPACES;

		private PatternConverter m_next;

		private int m_min = -1;

		private int m_max = 2147483647;

		private bool m_leftAlign;

		private string m_option;

		private ReusableStringWriter m_formatWriter = new ReusableStringWriter(CultureInfo.InvariantCulture);

		private PropertiesDictionary properties;

		public virtual log4net.Util.FormattingInfo FormattingInfo
		{
			get
			{
				return new log4net.Util.FormattingInfo(this.m_min, this.m_max, this.m_leftAlign);
			}
			set
			{
				this.m_min = value.Min;
				this.m_max = value.Max;
				this.m_leftAlign = value.LeftAlign;
			}
		}

		public virtual PatternConverter Next
		{
			get
			{
				return this.m_next;
			}
		}

		public virtual string Option
		{
			get
			{
				return this.m_option;
			}
			set
			{
				this.m_option = value;
			}
		}

		public PropertiesDictionary Properties
		{
			get
			{
				return this.properties;
			}
			set
			{
				this.properties = value;
			}
		}

		static PatternConverter()
		{
			string[] strArrays = new string[] { " ", "  ", "    ", "        ", "                ", "                                " };
			PatternConverter.SPACES = strArrays;
		}

		protected PatternConverter()
		{
		}

		protected abstract void Convert(TextWriter writer, object state);

		public virtual void Format(TextWriter writer, object state)
		{
			if (this.m_min < 0 && this.m_max == 2147483647)
			{
				this.Convert(writer, state);
				return;
			}
			this.m_formatWriter.Reset(1024, 256);
			this.Convert(this.m_formatWriter, state);
			StringBuilder stringBuilder = this.m_formatWriter.GetStringBuilder();
			int length = stringBuilder.Length;
			if (length > this.m_max)
			{
				writer.Write(stringBuilder.ToString(length - this.m_max, this.m_max));
				return;
			}
			if (length >= this.m_min)
			{
				writer.Write(stringBuilder.ToString());
				return;
			}
			if (this.m_leftAlign)
			{
				writer.Write(stringBuilder.ToString());
				PatternConverter.SpacePad(writer, this.m_min - length);
				return;
			}
			PatternConverter.SpacePad(writer, this.m_min - length);
			writer.Write(stringBuilder.ToString());
		}

		public virtual PatternConverter SetNext(PatternConverter patternConverter)
		{
			this.m_next = patternConverter;
			return this.m_next;
		}

		protected static void SpacePad(TextWriter writer, int length)
		{
			while (length >= 32)
			{
				writer.Write(PatternConverter.SPACES[5]);
				length = length - 32;
			}
			for (int i = 4; i >= 0; i--)
			{
				if ((length & 1 << (i & 31)) != 0)
				{
					writer.Write(PatternConverter.SPACES[i]);
				}
			}
		}

		protected static void WriteDictionary(TextWriter writer, ILoggerRepository repository, IDictionary value)
		{
			PatternConverter.WriteDictionary(writer, repository, value.GetEnumerator());
		}

		protected static void WriteDictionary(TextWriter writer, ILoggerRepository repository, IDictionaryEnumerator value)
		{
			writer.Write("{");
			bool flag = true;
			while (value.MoveNext())
			{
				if (!flag)
				{
					writer.Write(", ");
				}
				else
				{
					flag = false;
				}
				PatternConverter.WriteObject(writer, repository, value.Key);
				writer.Write("=");
				PatternConverter.WriteObject(writer, repository, value.Value);
			}
			writer.Write("}");
		}

		protected static void WriteObject(TextWriter writer, ILoggerRepository repository, object value)
		{
			if (repository != null)
			{
				repository.RendererMap.FindAndRender(value, writer);
				return;
			}
			if (value == null)
			{
				writer.Write(SystemInfo.NullText);
				return;
			}
			writer.Write(value.ToString());
		}
	}
}