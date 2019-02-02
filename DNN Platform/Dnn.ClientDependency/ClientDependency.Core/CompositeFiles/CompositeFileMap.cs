using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace ClientDependency.Core.CompositeFiles
{
    /// <summary>
	/// Deserialized structure of the XML stored in the map file
	/// </summary>
	public class CompositeFileMap
	{

		internal CompositeFileMap(string key, string compressionType, string file, IEnumerable<string> filePaths, int version)
		{
            DependentFiles = filePaths;
			FileKey = key;
			CompositeFileName = file;
			CompressionType = compressionType;
            Version = version;
		}

		public string FileKey { get; private set; }
		public string CompositeFileName { get; private set; }
		public string CompressionType { get; private set; }
        public int Version { get; private set; }
        public IEnumerable<string> DependentFiles { get; private set; }

        private byte[] m_FileBytes;

		/// <summary>
		/// If for some reason the file doesn't exist any more or we cannot read the file, this will return false.
		/// </summary>
		public bool HasFileBytes
		{
			get
			{
				GetCompositeFileBytes();
				return m_FileBytes != null;
			}
		}

		/// <summary>
		/// Returns the file's bytes
		/// </summary>
		public byte[] GetCompositeFileBytes()
		{
			if (m_FileBytes == null)
			{
                if (string.IsNullOrEmpty(CompositeFileName))
                {
                    return null;
                }

				try
				{
					FileInfo fi = new FileInfo(CompositeFileName);
					FileStream fs = fi.OpenRead();
					byte[] fileBytes = new byte[fs.Length];
					fs.Read(fileBytes, 0, fileBytes.Length);
					fs.Close();
					m_FileBytes = fileBytes;
				}
				catch
				{
					m_FileBytes = null;
				}				
			}
			return m_FileBytes;
		}

		

	}
}
