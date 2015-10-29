namespace DotNetNuke.Entities.Modules.Settings
{
    /// <summary>
    /// Base class for an attribute assigned to a DNN setting property (e.g. ModuleSettings, TabModuleSettings, PortalSettings etc.
    /// </summary>
    public class DnnSettingAttributeBase : ParameterAttributeBase, IParameterGrouping
    {
        #region Implementation of IParameterGrouping

        public string Category { get; set; }

        public string Prefix { get; set; }

        #endregion
    }
}
