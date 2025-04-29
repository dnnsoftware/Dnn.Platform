// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.OutputCache;

using System;
using System.IO;
using System.Text;

// helper class to capture the response into a file
public abstract class OutputCacheResponseFilter : Stream
{
    private Stream captureStream;

    /// <summary>Initializes a new instance of the <see cref="OutputCacheResponseFilter"/> class.</summary>
    /// <param name="filterChain"></param>
    /// <param name="cacheKey"></param>
    /// <param name="cacheDuration"></param>
    /// <param name="maxVaryByCount"></param>
    public OutputCacheResponseFilter(Stream filterChain, string cacheKey, TimeSpan cacheDuration, int maxVaryByCount)
    {
        this.ChainedStream = filterChain;
        this.CacheKey = cacheKey;
        this.CacheDuration = cacheDuration;
        this.MaxVaryByCount = maxVaryByCount;
        this.captureStream = this.CaptureStream;
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
            return true;
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

    public TimeSpan CacheDuration { get; set; }

    public virtual string CacheKey { get; set; }

    public Stream CaptureStream
    {
        get
        {
            return this.captureStream;
        }

        set
        {
            this.captureStream = value;
        }
    }

    public Stream ChainedStream { get; set; }

    public bool HasErrored { get; set; }

    public int MaxVaryByCount { get; set; }

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

    /// <inheritdoc/>
    public override void Flush()
    {
        this.ChainedStream.Flush();
        if (this.HasErrored)
        {
            return;
        }

        if (this.captureStream != null)
        {
            this.captureStream.Flush();
        }
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        this.ChainedStream.Write(buffer, offset, count);
        if (this.HasErrored)
        {
            return;
        }

        if (this.captureStream != null)
        {
            this.captureStream.Write(buffer, offset, count);
        }
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
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
