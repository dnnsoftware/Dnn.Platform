namespace DotNetNuke.Entities.Modules
{
    public interface IShareable
    {
        /// <summary>Does this module support Module Sharing (i.e., sharing modules between sites within a SiteGroup)?</summary>
        ModuleSharing SharingSupport { get; set; }
    }
}
