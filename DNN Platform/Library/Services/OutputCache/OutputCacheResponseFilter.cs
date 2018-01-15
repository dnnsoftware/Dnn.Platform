#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.IO;
using System.Text;

#endregion

namespace DotNetNuke.Services.OutputCache
{
    // helper class to capture the response into a file
    public abstract class OutputCacheResponseFilter : Stream
    {
        private Stream _captureStream;

        #region "Constructors"

        public OutputCacheResponseFilter(Stream filterChain, string cacheKey, TimeSpan cacheDuration, int maxVaryByCount)
        {
            ChainedStream = filterChain;
            CacheKey = cacheKey;
            CacheDuration = cacheDuration;
            MaxVaryByCount = maxVaryByCount;
            _captureStream = CaptureStream;
        }

        #endregion

        #region "Public Properties"

        public TimeSpan CacheDuration { get; set; }

        public virtual string CacheKey { get; set; }

        public Stream CaptureStream
        {
            get
            {
                return _captureStream;
            }
            set
            {
                _captureStream = value;
            }
        }

        public Stream ChainedStream { get; set; }

        public bool HasErrored { get; set; }

        public int MaxVaryByCount { get; set; }

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

        #endregion

        #region "Public Methods"

        public override void Flush()
        {
            ChainedStream.Flush();
            if (HasErrored)
            {
                return;
            }
            if ((((_captureStream) != null)))
            {
                _captureStream.Flush();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ChainedStream.Write(buffer, offset, count);
            if (HasErrored)
            {
                return;
            }
            if ((((_captureStream) != null)))
            {
                _captureStream.Write(buffer, offset, count);
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

        #endregion

        protected virtual void AddItemToCache(int itemId, string output)
        {
        }

        protected virtual void RemoveItemFromCache(int itemId)
        {
        }

        public virtual byte[] StopFiltering(int itemId, bool deleteData)
        {
            if (HasErrored)
            {
                return null;
            }

            if ((((CaptureStream) != null)))
            {
                CaptureStream.Position = 0;
                using (var reader = new StreamReader(CaptureStream, Encoding.Default))
                {
                    string output = reader.ReadToEnd();
                    AddItemToCache(itemId, output);
                }
                CaptureStream.Close();
                CaptureStream = null;
            }
            if (deleteData)
            {
                RemoveItemFromCache(itemId);
            }
            return null;
        }
    }
}