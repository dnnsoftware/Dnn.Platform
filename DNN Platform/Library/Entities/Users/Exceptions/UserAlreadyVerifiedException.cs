using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Entities.Users
{
    [Serializable]
    public class UserAlreadyVerifiedException : Exception
    {
        public UserAlreadyVerifiedException()
        {
        }

        public UserAlreadyVerifiedException(string message)
            : base(message)
        {
        }

        public UserAlreadyVerifiedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public UserAlreadyVerifiedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
