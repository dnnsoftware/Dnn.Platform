// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.IO;

using System;
using System.IO;

/// <summary>
/// Disposable wrapper around a single, exclusively-owned, randomly-named temporary file.
/// Replaces the insecure <see cref="Path.GetTempFileName"/> pattern flagged by SonarQube
/// rule <c>csharpsquid:S5445</c> (CWE-377).
/// </summary>
public sealed class SecureTempFile : IDisposable
{
    private const int BufferSize = 4096;

    private readonly FileStream stream;
    private readonly string path;
    private bool disposed;

    /// <summary>Initializes a new instance of the <see cref="SecureTempFile"/> class. A new
    /// temporary file with a cryptographically-random name is created inside
    /// <see cref="System.IO.Path.GetTempPath"/> and opened exclusively for read/write. The
    /// file is registered for automatic deletion when the underlying handle closes.</summary>
    /// <exception cref="IOException">Thrown if the temp directory is unwritable or a
    /// (vanishingly rare) name collision occurs.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user lacks write
    /// permission on the temp directory.</exception>
    public SecureTempFile()
    {
        var dir = System.IO.Path.GetTempPath();
        var name = System.IO.Path.GetRandomFileName();
        this.path = System.IO.Path.Combine(dir, name);

        this.stream = new FileStream(
            this.path,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None,
            BufferSize,
            FileOptions.DeleteOnClose);
    }

    /// <summary>Gets the absolute filesystem path of the temporary file. Stable for the
    /// lifetime of this instance.</summary>
    /// <exception cref="ObjectDisposedException">Thrown if accessed after disposal.</exception>
    public string Path
    {
        get
        {
            this.ThrowIfDisposed();
            return this.path;
        }
    }

    /// <summary>Gets the exclusive read/write <see cref="FileStream"/> opened against
    /// <see cref="Path"/>. Closed automatically on disposal.</summary>
    /// <exception cref="ObjectDisposedException">Thrown if accessed after disposal.</exception>
    public FileStream Stream
    {
        get
        {
            this.ThrowIfDisposed();
            return this.stream;
        }
    }

    /// <summary>Gets a value indicating whether this instance has been disposed.</summary>
    public bool IsDisposed => this.disposed;

    /// <summary>Closes the underlying stream, which triggers OS-level deletion of the file
    /// because the stream was opened with <see cref="FileOptions.DeleteOnClose"/>.
    /// Idempotent.</summary>
    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        this.stream.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException(nameof(SecureTempFile));
        }
    }
}
