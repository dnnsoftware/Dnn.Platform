using System;

namespace DotNetNuke.Entities.Modules.Settings
{
    /// <summary>
    /// When applied to a property this attribute persists the value of the property in the DNN ModuleSettings referenced by the ModuleId within the context.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    [Serializable]
    public class ModuleSettingAttribute : ParameterAttributeBase
    {
    }
}
