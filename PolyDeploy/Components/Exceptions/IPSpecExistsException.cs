using System;
using System.Runtime.Serialization;

namespace Cantarus.Modules.PolyDeploy.Components.Exceptions
{
    [Serializable]
    public class IPSpecExistsException : Exception
    {
        public IPSpecExistsException() { }

        public IPSpecExistsException(string message)
            : base(message) { }

        public IPSpecExistsException(string message, Exception innerException)
            : base(message, innerException) { }

        public IPSpecExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
