using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.Social.Messaging.Exceptions
{
    [Serializable]
    public class MessageOrRecipientNotFoundException : Exception
    {
        public MessageOrRecipientNotFoundException()
        {
        }

        public MessageOrRecipientNotFoundException(string message)
            : base(message)
        {
        }

        public MessageOrRecipientNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public MessageOrRecipientNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
