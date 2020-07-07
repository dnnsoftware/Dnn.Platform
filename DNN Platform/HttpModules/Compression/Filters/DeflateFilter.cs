// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Compression
{
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// Summary description for DeflateFilter.
    /// </summary>
    public class DeflateFilter : CompressingFilter
    {
        private readonly DeflateStream m_stream;

        public DeflateFilter(Stream baseStream)
            : base(baseStream)
        {
            this.m_stream = new DeflateStream(baseStream, CompressionMode.Compress);
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
            if (!this.HasWrittenHeaders)
            {
                this.WriteHeaders();
            }

            this.m_stream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            this.m_stream.Close();
        }

        public override void Flush()
        {
            this.m_stream.Flush();
        }
    }
}
