using System;
using System.Runtime.Serialization;

using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Entities.Users
{
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
