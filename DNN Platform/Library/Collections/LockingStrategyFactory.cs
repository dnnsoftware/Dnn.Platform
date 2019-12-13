// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Collections.Internal
{
    internal class LockingStrategyFactory
    {
        public static ILockStrategy Create(LockingStrategy strategy)
        {
            switch (strategy)
            {
                case LockingStrategy.ReaderWriter:

                    return new ReaderWriterLockStrategy();
                case LockingStrategy.Exclusive:

                    return new ExclusiveLockStrategy();
                default:

                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
