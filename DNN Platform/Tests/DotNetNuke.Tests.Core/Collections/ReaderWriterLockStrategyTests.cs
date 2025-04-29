// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Collections;

using DotNetNuke.Collections.Internal;

using NUnit.Framework;

[TestFixture]
public class ReaderWriterLockStrategyTests : LockStrategyTests
{
    [Test]
    public override void DoubleReadLockThrows()
    {
        Assert.DoesNotThrow(base.DoubleReadLock);
    }

    [Test]
    public override void DoubleWriteLockThrows()
    {
        Assert.DoesNotThrow(base.DoubleWriteLock);
    }

    internal override ILockStrategy GetLockStrategy()
    {
        return new ReaderWriterLockStrategy();
    }
}
