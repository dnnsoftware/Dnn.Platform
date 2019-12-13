using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.Social.Messaging.Exceptions
{
    [Serializable]
    public class AttachmentsNotAllowed : Exception
    {
        public AttachmentsNotAllowed()
        {
        }

        public AttachmentsNotAllowed(string message)
            : base(message)
        {
        }

        public AttachmentsNotAllowed(string message, Exception inner)
            : base(message, inner)
        {
        }

        public AttachmentsNotAllowed(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
