// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System.IO;
using System.IO.Compression;

#endregion

namespace DotNetNuke.HttpModules.Compression
{
    /// <summary>
    /// This is a little filter to support HTTP compression using GZip
    /// </summary>
    public class GZipFilter : CompressingFilter
    {
        private readonly GZipStream m_stream;

        public GZipFilter(Stream baseStream) : base(baseStream)
        {
            m_stream = new GZipStream(baseStream, CompressionMode.Compress);
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
