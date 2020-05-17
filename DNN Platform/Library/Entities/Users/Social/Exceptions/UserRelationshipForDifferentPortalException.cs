// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Runtime.Serialization;

using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Entities.Users
{
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
