using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.Social.Messaging.Exceptions
{
    [Serializable]
    public class RecipientLimitExceededException : Exception
    {
        public RecipientLimitExceededException()
        {
        }

        public RecipientLimitExceededException(string message)
            : base(message)
        {
        }

        public RecipientLimitExceededException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public RecipientLimitExceededException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
