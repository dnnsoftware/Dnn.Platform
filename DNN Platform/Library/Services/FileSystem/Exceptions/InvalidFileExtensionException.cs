// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Runtime.Serialization;

    using DotNetNuke.Services.Exceptions;

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
