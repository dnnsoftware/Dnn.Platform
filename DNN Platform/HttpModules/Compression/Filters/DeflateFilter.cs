#region Usings

using System.IO;
using System.IO.Compression;

#endregion

namespace DotNetNuke.HttpModules.Compression
{
    /// <summary>
    /// Summary description for DeflateFilter.
    /// </summary>
    public class DeflateFilter : CompressingFilter
    {
        private readonly DeflateStream m_stream;

        public DeflateFilter(Stream baseStream) : base(baseStream)
        {
            m_stream = new DeflateStream(baseStream, CompressionMode.Compress);
        }

        public override string ContentEncoding
        {
            get
            {
                return "deflate";
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!HasWrittenHeaders)
            {
                WriteHeaders();
            }
            m_stream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            m_stream.Close();
        }

        public override void Flush()
        {
            m_stream.Flush();
        }
    }
}
