// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.Social.Messaging.Exceptions
{
    [Serializable]
    public class ThrottlingIntervalNotMetException : Exception
    {
        public ThrottlingIntervalNotMetException()
        {
        }

        public ThrottlingIntervalNotMetException(string message)
            : base(message)
        {
        }

        public ThrottlingIntervalNotMetException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public ThrottlingIntervalNotMetException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
