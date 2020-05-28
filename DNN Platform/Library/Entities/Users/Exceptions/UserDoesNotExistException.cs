// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Entities.Users
{
    [Serializable]
    public class UserDoesNotExistException : Exception
    {
        public UserDoesNotExistException()
        {
        }

        public UserDoesNotExistException(string message)
            : base(message)
        {
        }

        public UserDoesNotExistException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public UserDoesNotExistException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
