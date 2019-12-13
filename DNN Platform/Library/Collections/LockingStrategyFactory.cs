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
