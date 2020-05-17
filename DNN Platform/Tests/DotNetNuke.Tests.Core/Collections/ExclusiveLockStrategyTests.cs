// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;

using DotNetNuke.Collections.Internal;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Collections
{
    [TestFixture]
    public class ExclusiveLockStrategyTests : LockStrategyTests
    {
        internal override ILockStrategy GetLockStrategy()
        {
            return new ExclusiveLockStrategy();
        }

        protected override IEnumerable<Action<ILockStrategy>> GetObjectDisposedExceptionMethods()
        {
            var l = (List<Action<ILockStrategy>>) base.GetObjectDisposedExceptionMethods();

            l.Add((ILockStrategy strategy) =>
                      {
                          ExclusiveLockStrategy els = (ExclusiveLockStrategy) strategy;
                          els.Exit();
                      });

            return l;
        }
    }
}
