// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.OutputCache
{
    using System;
    using System.IO;
    using System.Text;

    // helper class to capture the response into a file
    public abstract class OutputCacheResponseFilter : Stream
    {
        private Stream _captureStream;

        public OutputCacheResponseFilter(Stream filterChain, string cacheKey, TimeSpan cacheDuration, int maxVaryByCount)
        {
            this.ChainedStream = filterChain;
            this.CacheKey = cacheKey;
            this.CacheDuration = cacheDuration;
            this.MaxVaryByCount = maxVaryByCount;
            this._captureStream = this.CaptureStream;
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public TimeSpan CacheDuration { get; set; }

        public virtual string CacheKey { get; set; }

        public Stream CaptureStream
        {
            get
            {
                return this._captureStream;
            }

            set
            {
                this._captureStream = value;
            }
        }

        public Stream ChainedStream { get; set; }

        public bool HasErrored { get; set; }

        public int MaxVaryByCount { get; set; }

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

        public override void Flush()
        {
            this.ChainedStream.Flush();
            if (this.HasErrored)
            {
                return;
            }

            if (this._captureStream != null)
            {
                this._captureStream.Flush();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.ChainedStream.Write(buffer, offset, count);
            if (this.HasErrored)
            {
                return;
            }

            if (this._captureStream != null)
            {
                this._captureStream.Write(buffer, offset, count);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public virtual byte[] StopFiltering(int itemId, bool deleteData)
        {
            if (this.HasErrored)
            {
                return null;
            }

            if (this.CaptureStream != null)
            {
                this.CaptureStream.Position = 0;
                using (var reader = new StreamReader(this.CaptureStream, Encoding.Default))
                {
                    string output = reader.ReadToEnd();
                    this.AddItemToCache(itemId, output);
                }

                this.CaptureStream.Close();
                this.CaptureStream = null;
            }

            if (deleteData)
            {
                this.RemoveItemFromCache(itemId);
            }

            return null;
        }

        protected virtual void AddItemToCache(int itemId, string output)
        {
        }

        protected virtual void RemoveItemFromCache(int itemId)
        {
        }
    }
}
