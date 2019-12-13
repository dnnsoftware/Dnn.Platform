using System;
using System.Runtime.Serialization;

using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Entities.Users
{
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
