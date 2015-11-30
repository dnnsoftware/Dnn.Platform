#region Usings
using System;
using ClientDependency.Core.Controls;
using ClientDependency.Core;
#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    public partial class DnnCssExclude : SkinObjectBase
    {
        public string Name { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ctlExclude.Name = Name;
        }
    }
}