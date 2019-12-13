using System;
using System.Runtime.Serialization;

using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Entities.Users
{
    [Serializable]
    public class UserRelationshipExistsException : Exception
    {
        public UserRelationshipExistsException()
        {
        }

        public UserRelationshipExistsException(string message)
            : base(message)
        {
        }

        public UserRelationshipExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public UserRelationshipExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
