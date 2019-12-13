using System;
using System.Runtime.Serialization;

using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Services.FileSystem
{
    [Serializable]
    public class NoSpaceAvailableException : Exception
    {
        public NoSpaceAvailableException()
        {
        }

        public NoSpaceAvailableException(string message)
            : base(message)
        {
        }

        public NoSpaceAvailableException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public NoSpaceAvailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
