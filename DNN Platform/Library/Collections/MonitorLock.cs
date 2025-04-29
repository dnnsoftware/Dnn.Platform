// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal;

using System;

/// <summary>An <see cref="ISharedCollectionLock"/> implementation which uses an <see cref="ExclusiveLockStrategy"/>.</summary>
internal class MonitorLock : IDisposable, ISharedCollectionLock
{
    private ExclusiveLockStrategy lockStrategy;

    // To detect redundant calls
    private bool isDisposed;

    /// <summary>Initializes a new instance of the <see cref="MonitorLock"/> class.</summary>
    /// <param name="lockStrategy">An <see cref="ExclusiveLockStrategy"/> instance to use.</param>
    public MonitorLock(ExclusiveLockStrategy lockStrategy)
    {
        this.lockStrategy = lockStrategy;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code.  Put cleanup code in Dispose(bool disposing) below.
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases resources before final disposal.</summary>
    /// <param name="disposing">A value indicating if the instance is currently beeing disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                this.lockStrategy.Exit();
                this.lockStrategy = null;
            }
        }

        this.isDisposed = true;
    }
}
