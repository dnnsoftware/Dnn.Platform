// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class AttachmentsNotAllowed : Exception
    {
        public AttachmentsNotAllowed()
        {
        }

        public AttachmentsNotAllowed(string message)
            : base(message)
        {
        }

        public AttachmentsNotAllowed(string message, Exception inner)
            : base(message, inner)
        {
        }

        public AttachmentsNotAllowed(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
