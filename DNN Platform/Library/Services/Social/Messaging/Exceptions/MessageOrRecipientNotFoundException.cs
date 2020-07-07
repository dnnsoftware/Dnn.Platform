// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class MessageOrRecipientNotFoundException : Exception
    {
        public MessageOrRecipientNotFoundException()
        {
        }

        public MessageOrRecipientNotFoundException(string message)
            : base(message)
        {
        }

        public MessageOrRecipientNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public MessageOrRecipientNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
