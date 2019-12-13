// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Threading;

using DotNetNuke.Collections.Internal;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Collections
{
    [TestFixture]
    public class ReaderWriterSlimLockTests
    {
        [Test]
        public void DoubleDisposeAllowed()
        {
            using (var l = new ReaderWriterLockSlim())
            {
                var scl = new ReaderWriterSlimLock(l);

                scl.Dispose();
                scl.Dispose();
                //no exception on 2nd dispose
            }
        }
    }
}
