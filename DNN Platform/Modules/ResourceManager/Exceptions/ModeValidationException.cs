using System;

namespace Dnn.Modules.ResourceManager.Exceptions
{
    public class ModeValidationException : Exception
    {
        public ModeValidationException(string message) : base(message)
        {
        }
    }
}