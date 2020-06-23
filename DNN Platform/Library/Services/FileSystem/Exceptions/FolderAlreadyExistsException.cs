// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class FolderAlreadyExistsException : Exception
    {
        public FolderAlreadyExistsException()
        {
        }

        public FolderAlreadyExistsException(string message)
            : base(message)
        {
        }

        public FolderAlreadyExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public FolderAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
