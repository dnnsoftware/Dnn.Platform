using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.FileSystem
{
    [Serializable]
    public class InvalidMetadataValuesException : Exception
    {
        public InvalidMetadataValuesException()
        {
        }

        public InvalidMetadataValuesException(string message)
            : base(message)
        {
        }

        public InvalidMetadataValuesException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidMetadataValuesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
