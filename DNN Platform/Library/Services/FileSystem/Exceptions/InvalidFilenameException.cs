// Copyright (c) .NET Foundation. All rights reserved
// Licensed under the MIT License.  See LICENSE file in the project root for full detail

using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.FileSystem
{
    [Serializable]
    public class InvalidFilenameException : Exception
    {
        public InvalidFilenameException()
        {
        }

        public InvalidFilenameException(string message)
            : base(message)
        {
        }

        public InvalidFilenameException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidFilenameException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
