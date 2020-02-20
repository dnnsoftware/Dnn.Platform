// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Runtime.Serialization;

using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Services.FileSystem
{
    [Serializable]
    public class NoSpaceAvailableException : Exception
    {
        public NoSpaceAvailableException()
        {
        }

        public NoSpaceAvailableException(string message)
            : base(message)
        {
        }

        public NoSpaceAvailableException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public NoSpaceAvailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
