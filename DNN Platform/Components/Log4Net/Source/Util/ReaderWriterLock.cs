namespace log4net.Util
{
	public sealed class ReaderWriterLock
	{
		private System.Threading.ReaderWriterLock m_lock;

		public ReaderWriterLock()
		{
			this.m_lock = new System.Threading.ReaderWriterLock();
		}

		public void AcquireReaderLock()
		{
			this.m_lock.AcquireReaderLock(-1);
		}

		public void AcquireWriterLock()
		{
			this.m_lock.AcquireWriterLock(-1);
		}

		public void ReleaseReaderLock()
		{
			this.m_lock.ReleaseReaderLock();
		}

		public void ReleaseWriterLock()
		{
			this.m_lock.ReleaseWriterLock();
		}
	}
}