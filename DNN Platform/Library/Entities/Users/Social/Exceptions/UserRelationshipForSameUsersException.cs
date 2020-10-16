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
        /// <summary>
        /// Initializes a new instance of the <see cref="UserRelationshipForSameUsersException"/> class.
        /// </summary>
        public UserRelationshipForSameUsersException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRelationshipForSameUsersException"/> class.
        /// </summary>
        /// <param name="message"></param>
        public UserRelationshipForSameUsersException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRelationshipForSameUsersException"/> class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public UserRelationshipForSameUsersException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRelationshipForSameUsersException"/> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public UserRelationshipForSameUsersException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
