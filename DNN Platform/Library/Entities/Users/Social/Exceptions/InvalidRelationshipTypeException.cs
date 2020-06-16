// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    using System;
    using System.Runtime.Serialization;

    using DotNetNuke.Services.Exceptions;

    [Serializable]
    public class InvalidRelationshipTypeException : Exception
    {
        public InvalidRelationshipTypeException()
        {
        }

        public InvalidRelationshipTypeException(string message)
            : base(message)
        {
        }

        public InvalidRelationshipTypeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidRelationshipTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
