using System;
using System.Runtime.Serialization;

using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Services.FileSystem
{
    [Serializable]
    public class FolderProviderException : Exception
    {
        public FolderProviderException()
        {
        }

        public FolderProviderException(string message)
            : base(message)
        {
        }

        public FolderProviderException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public FolderProviderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
