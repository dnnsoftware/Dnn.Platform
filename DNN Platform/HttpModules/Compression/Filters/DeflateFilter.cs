// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Compression;

using System.IO;
using System.IO.Compression;

/// <summary>Summary description for DeflateFilter.</summary>
public class DeflateFilter : CompressingFilter
{
    private readonly DeflateStream stream;

    /// <summary>Initializes a new instance of the <see cref="DeflateFilter"/> class.</summary>
    /// <param name="baseStream">The base stream.</param>
    public DeflateFilter(Stream baseStream)
        : base(baseStream)
    {
        this.stream = new DeflateStream(baseStream, CompressionMode.Compress);
    }

    /// <inheritdoc/>
    public override string ContentEncoding
    {
        get
        {
            return "deflate";
        }
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        if (!this.HasWrittenHeaders)
        {
            this.WriteHeaders();
        }

        this.stream.Write(buffer, offset, count);
    }

    /// <inheritdoc/>
    public override void Close()
    {
        this.stream.Close();
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        this.stream.Flush();
    }
}
