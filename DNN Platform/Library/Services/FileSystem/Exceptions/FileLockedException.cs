// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.FileSystem
{
    [Serializable]
    public class FileLockedException : Exception
    {
        public FileLockedException()
        {
        }

        public FileLockedException(string message)
            : base(message)
        {
        }

        public FileLockedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public FileLockedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
