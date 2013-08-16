namespace DotNetNuke.Modules.Journal
{
    public class FilesStatus
    {
		// ReSharper disable InconsistentNaming
        public bool success { get; set; }
        public string name { get; set; }
        public string extension { get; set; }
		public string type { get; set; }
		public int size { get; set; }
		public string progress { get; set; }
		public string url { get; set; }
		public string thumbnail_url { get; set; }
		public string message { get; set; }
        public int file_id { get; set; }
        // ReSharper restore InconsistentNaming

		public FilesStatus ()
		{
		}
	}
}