// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Compression
{
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// This is a little filter to support HTTP compression using GZip.
    /// </summary>
    public class GZipFilter : CompressingFilter
    {
        private readonly GZipStream m_stream;

        public GZipFilter(Stream baseStream)
            : base(baseStream)
        {
            this.m_stream = new GZipStream(baseStream, CompressionMode.Compress);
        }

        public override string ContentEncoding
        {
            get
            {
                return "gzip";
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
