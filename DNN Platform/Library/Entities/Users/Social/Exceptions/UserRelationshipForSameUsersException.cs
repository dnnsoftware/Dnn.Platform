﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class UserRelationshipForSameUsersException : Exception
    {
        public UserRelationshipForSameUsersException()
        {
        }

        public UserRelationshipForSameUsersException(string message)
            : base(message)
        {
        }

        public UserRelationshipForSameUsersException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public UserRelationshipForSameUsersException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
