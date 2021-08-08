﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Compression
{
    using System;
    using System.IO;

    public abstract class HttpOutputFilter : Stream
    {
        private readonly Stream _sink;

        protected HttpOutputFilter(Stream baseStream)
        {
            this._sink = baseStream;
        }

        /// <inheritdoc/>
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override bool CanWrite
        {
            get
            {
                return this._sink.CanWrite;
            }
        }

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        protected Stream BaseStream
        {
            get
            {
                return this._sink;
            }
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin direction)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long length)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Close()
        {
            this._sink.Close();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            this._sink.Flush();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
