using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Entities.Users
{
    [Serializable]
    public class InvalidVerificationCodeException : Exception
    {
        public InvalidVerificationCodeException()
        {
        }

        public InvalidVerificationCodeException(string message)
            : base(message)
        {
        }

        public InvalidVerificationCodeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidVerificationCodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
