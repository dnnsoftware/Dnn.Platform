using log4net.Core;

using System;
using System.IO;

namespace log4net.Util.PatternStringConverters
{
	internal sealed class RandomStringPatternConverter : PatternConverter, IOptionHandler
	{
		private readonly static Random s_random;

		private int m_length = 4;

		private readonly static Type declaringType;

		static RandomStringPatternConverter()
		{
			RandomStringPatternConverter.s_random = new Random();
			RandomStringPatternConverter.declaringType = typeof(RandomStringPatternConverter);
		}

		public RandomStringPatternConverter()
		{
		}

		public void ActivateOptions()
		{
			int num;
			string option = this.Option;
			if (option != null && option.Length > 0)
			{
				if (SystemInfo.TryParse(option, out num))
				{
					this.m_length = num;
					return;
				}
				LogLog.Error(RandomStringPatternConverter.declaringType, string.Concat("RandomStringPatternConverter: Could not convert Option [", option, "] to Length Int32"));
			}
		}

		protected override void Convert(TextWriter writer, object state)
		{
			try
			{
				lock (RandomStringPatternConverter.s_random)
				{
					for (int i = 0; i < this.m_length; i++)
					{
						int num = RandomStringPatternConverter.s_random.Next(36);
						if (num < 26)
						{
							writer.Write((char)(65 + num));
						}
						else if (num >= 36)
						{
							writer.Write('X');
						}
						else
						{
							writer.Write((char)(48 + (num - 26)));
						}
					}
				}
			}
			catch (Exception exception)
			{
				LogLog.Error(RandomStringPatternConverter.declaringType, "Error occurred while converting.", exception);
			}
		}
	}
}