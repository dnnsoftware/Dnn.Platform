// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Collections
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Collections.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class ExclusiveLockStrategyTests : LockStrategyTests
    {
        internal override ILockStrategy GetLockStrategy()
        {
            return new ExclusiveLockStrategy();
        }

        protected override IEnumerable<Action<ILockStrategy>> GetObjectDisposedExceptionMethods()
        {
            var l = (List<Action<ILockStrategy>>)base.GetObjectDisposedExceptionMethods();

            l.Add((ILockStrategy strategy) =>
                      {
                          ExclusiveLockStrategy els = (ExclusiveLockStrategy)strategy;
                          els.Exit();
                      });

            return l;
        }
    }
}
