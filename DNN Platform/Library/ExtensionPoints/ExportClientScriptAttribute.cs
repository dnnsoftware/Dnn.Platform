namespace DotNetNuke.ExtensionPoints
{
    using System.ComponentModel.Composition;

    [MetadataAttribute]
    public class ExportClientScriptAttribute : ExportAttribute
    {
        public ExportClientScriptAttribute()
            : base(typeof(IScriptItemExtensionPoint))
        {            
        }
    }
}
