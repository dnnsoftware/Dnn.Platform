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
