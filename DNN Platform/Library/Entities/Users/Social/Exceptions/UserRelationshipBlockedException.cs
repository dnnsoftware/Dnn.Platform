﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Runtime.Serialization;

using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Entities.Users
{
    [Serializable]
    public class UserRelationshipBlockedException : Exception
    {
        public UserRelationshipBlockedException()
        {
        }

        public UserRelationshipBlockedException(string message)
            : base(message)
        {
        }

        public UserRelationshipBlockedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public UserRelationshipBlockedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
