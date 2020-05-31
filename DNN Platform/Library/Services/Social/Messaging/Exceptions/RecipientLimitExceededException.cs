// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.Social.Messaging.Exceptions
{
    [Serializable]
    public class RecipientLimitExceededException : Exception
    {
        public RecipientLimitExceededException()
        {
        }

        public RecipientLimitExceededException(string message)
            : base(message)
        {
        }

        public RecipientLimitExceededException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public RecipientLimitExceededException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
