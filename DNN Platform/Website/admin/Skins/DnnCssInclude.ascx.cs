#region Usings
using System;
using ClientDependency.Core.Controls;
using ClientDependency.Core;
#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    public partial class DnnCssInclude : SkinObjectBase
    {
        public CssMediaType CssMedia { get; set; }
        public string FilePath { get; set; }
        public string PathNameAlias { get; set; }
        public int Priority { get; set; }
        public bool AddTag { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public bool ForceVersion { get; set; }
        public string ForceProvider { get; set; }
        public bool ForceBundle { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ctlInclude.AddTag = AddTag;
            ctlInclude.CssMedia = CssMedia;
            ctlInclude.FilePath = FilePath;
            ctlInclude.ForceBundle = ForceBundle;
            ctlInclude.ForceProvider = ForceProvider;
            ctlInclude.ForceVersion = ForceVersion;
            ctlInclude.Name = Name;
            ctlInclude.PathNameAlias = PathNameAlias;
            ctlInclude.Priority = Priority;
            ctlInclude.Version = Version;
        }
    }
}