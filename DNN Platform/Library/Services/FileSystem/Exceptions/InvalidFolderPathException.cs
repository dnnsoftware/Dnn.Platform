using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.FileSystem
{
    [Serializable]
    public class InvalidFolderPathException : Exception
    {
        public InvalidFolderPathException()
        {
        }

        public InvalidFolderPathException(string message)
            : base(message)
        {
        }

        public InvalidFolderPathException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidFolderPathException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
