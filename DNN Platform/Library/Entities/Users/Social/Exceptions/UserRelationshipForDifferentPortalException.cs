// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    using System;
    using System.Runtime.Serialization;

    using DotNetNuke.Services.Exceptions;

    [Serializable]
    public class UserRelationshipForDifferentPortalException : Exception
    {
        public UserRelationshipForDifferentPortalException()
        {
        }

        public UserRelationshipForDifferentPortalException(string message)
            : base(message)
        {
        }

        public UserRelationshipForDifferentPortalException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public UserRelationshipForDifferentPortalException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
