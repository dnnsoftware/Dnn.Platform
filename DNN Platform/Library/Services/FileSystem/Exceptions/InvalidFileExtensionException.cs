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
    public class InvalidFileExtensionException : Exception
    {
        public InvalidFileExtensionException()
        {
        }

        public InvalidFileExtensionException(string message)
            : base(message)
        {
        }

        public InvalidFileExtensionException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidFileExtensionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
