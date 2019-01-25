using System;

namespace Cantarus.Modules.PolyDeploy.Components.Exceptions
{
    internal class SettingNotFoundException : Exception
    {
        public static SettingNotFoundException Create(string group, string key)
        {
            return new SettingNotFoundException($"Setting in group '{group}' with key '{key}' was not found.");
        }

        public SettingNotFoundException(string message) : base(message) { }
    }
}
