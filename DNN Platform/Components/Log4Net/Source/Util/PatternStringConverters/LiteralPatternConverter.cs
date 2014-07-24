using System;
using System.IO;

namespace log4net.Util.PatternStringConverters
{
	internal class LiteralPatternConverter : PatternConverter
	{
		public LiteralPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, object state)
		{
			throw new InvalidOperationException("Should never get here because of the overridden Format method");
		}

		public override void Format(TextWriter writer, object state)
		{
			writer.Write(this.Option);
		}

		public override PatternConverter SetNext(PatternConverter pc)
		{
			LiteralPatternConverter literalPatternConverter = pc as LiteralPatternConverter;
			if (literalPatternConverter == null)
			{
				return base.SetNext(pc);
			}
			this.Option = string.Concat(this.Option, literalPatternConverter.Option);
			return this;
		}
	}
}