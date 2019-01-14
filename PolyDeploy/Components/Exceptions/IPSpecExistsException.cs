using System;

namespace Cantarus.Modules.PolyDeploy.Components.Exceptions
{
    internal class IPSpecExistsException : Exception
    {
        public IPSpecExistsException(string message) : base(message) { }
    }
}
