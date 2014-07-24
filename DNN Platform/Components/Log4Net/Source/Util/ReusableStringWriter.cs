using System;
using System.IO;
using System.Text;

namespace log4net.Util
{
	public class ReusableStringWriter : StringWriter
	{
		public ReusableStringWriter(IFormatProvider formatProvider) : base(formatProvider)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}

		public void Reset(int maxCapacity, int defaultSize)
		{
			StringBuilder stringBuilder = this.GetStringBuilder();
			stringBuilder.Length = 0;
			if (stringBuilder.Capacity > maxCapacity)
			{
				stringBuilder.Capacity = defaultSize;
			}
		}
	}
}