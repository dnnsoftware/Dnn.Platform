#region Usings

using System;

using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Common.Controls
{
    public partial class Message : PortalModuleBase
    {
        private void InitializeComponent()
        {
            ID = "Message";
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitializeComponent();
        }
    }
}
