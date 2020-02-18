// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Collections.Internal;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Collections
{
    [TestFixture]
    public class ReaderWriterLockStrategyTests : LockStrategyTests
    {
        internal override ILockStrategy GetLockStrategy()
        {
            return new ReaderWriterLockStrategy();
        }

        [Test] // no ExpectedException attribute
        public override void DoubleReadLockThrows()
        {
            base.DoubleReadLockThrows();
        }

        [Test] // no ExpectedException attribute
        public override void DoubleWriteLockThrows()
        {
            base.DoubleWriteLockThrows();
        }
    }
}
