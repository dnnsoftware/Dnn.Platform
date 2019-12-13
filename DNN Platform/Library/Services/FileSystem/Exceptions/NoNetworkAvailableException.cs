// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.FileSystem
{
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
