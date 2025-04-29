// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Collections;

using System;
using System.Collections.Generic;

using DotNetNuke.Collections.Internal;
using NUnit.Framework;

[TestFixture]
public class ExclusiveLockStrategyTests : LockStrategyTests
{
    [Test]
    [TestCaseSource(nameof(GetExclusiveLockStrategyObjectDisposedExceptionMethods))]
    public override void MethodsThrowAfterDisposed(Action<ILockStrategy> methodCall)
    {
        base.MethodsThrowAfterDisposed(methodCall);
    }

    internal override ILockStrategy GetLockStrategy()
    {
        return new ExclusiveLockStrategy();
    }

    protected static IEnumerable<Action<ILockStrategy>> GetExclusiveLockStrategyObjectDisposedExceptionMethods()
    {
        var l = (List<Action<ILockStrategy>>)LockStrategyTests.GetObjectDisposedExceptionMethods();

        l.Add((ILockStrategy strategy) =>
        {
            ExclusiveLockStrategy els = (ExclusiveLockStrategy)strategy;
            els.Exit();
        });

        return l;
    }
}
