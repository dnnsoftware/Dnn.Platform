using System;
using System.Runtime.Serialization;

using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Services.FileSystem
{
    [Serializable]
    public class PermissionsNotMetException : Exception
    {
        public PermissionsNotMetException()
        {
        }

        public PermissionsNotMetException(string message)
            : base(message)
        {
        }

        public PermissionsNotMetException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public PermissionsNotMetException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
