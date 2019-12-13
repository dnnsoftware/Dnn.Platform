using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.FileSystem
{
    [Serializable]
    public class FolderAlreadyExistsException : Exception
    {
        public FolderAlreadyExistsException()
        {
        }

        public FolderAlreadyExistsException(string message)
            : base(message)
        {
        }

        public FolderAlreadyExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public FolderAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
