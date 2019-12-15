// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Collections.Internal;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Collections
{
    [TestFixture]
    public class ReaderWriterLockSharedDictionaryTests : SharedDictionaryTests
    {
        public override LockingStrategy LockingStrategy
        {
            get
            {
                return LockingStrategy.ReaderWriter;
            }
        }
    }
}
