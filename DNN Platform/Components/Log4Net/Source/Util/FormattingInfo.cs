namespace log4net.Util
{
	public class FormattingInfo
	{
		private int m_min = -1;

		private int m_max = 2147483647;

		private bool m_leftAlign;

		public bool LeftAlign
		{
			get
			{
				return this.m_leftAlign;
			}
			set
			{
				this.m_leftAlign = value;
			}
		}

		public int Max
		{
			get
			{
				return this.m_max;
			}
			set
			{
				this.m_max = value;
			}
		}

		public int Min
		{
			get
			{
				return this.m_min;
			}
			set
			{
				this.m_min = value;
			}
		}

		public FormattingInfo()
		{
		}

		public FormattingInfo(int min, int max, bool leftAlign)
		{
			this.m_min = min;
			this.m_max = max;
			this.m_leftAlign = leftAlign;
		}
	}
}