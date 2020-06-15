// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging.Exceptions
{
    using System;
    using System.Runtime.Serialization;

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
