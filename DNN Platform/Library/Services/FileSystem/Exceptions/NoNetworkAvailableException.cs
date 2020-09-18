// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class NoNetworkAvailableException : Exception
    {
        public NoNetworkAvailableException()
        {
        }

        public NoNetworkAvailableException(string message)
            : base(message)
        {
        }

        public NoNetworkAvailableException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public NoNetworkAvailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
