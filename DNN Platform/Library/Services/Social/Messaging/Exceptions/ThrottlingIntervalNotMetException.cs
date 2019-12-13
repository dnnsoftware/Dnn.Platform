using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.Social.Messaging.Exceptions
{
    [Serializable]
    public class ThrottlingIntervalNotMetException : Exception
    {
        public ThrottlingIntervalNotMetException()
        {
        }

        public ThrottlingIntervalNotMetException(string message)
            : base(message)
        {
        }

        public ThrottlingIntervalNotMetException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public ThrottlingIntervalNotMetException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
