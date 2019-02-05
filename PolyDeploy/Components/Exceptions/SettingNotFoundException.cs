using System;
using System.Runtime.Serialization;

namespace Cantarus.Modules.PolyDeploy.Components.Exceptions
{
    [Serializable]
    public class SettingNotFoundException : Exception
    {
        public static SettingNotFoundException Create(string group, string key)
        {
            return new SettingNotFoundException($"Setting in group '{group}' with key '{key}' was not found.");
        }

        public SettingNotFoundException() { }

        public SettingNotFoundException(string message)
            : base(message) { }

        public SettingNotFoundException(string message, Exception innerException)
            : base(message, innerException) { }

        public SettingNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
