// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls.Internal
{
    using System;
    using System.Data;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Security.Permissions;

    internal class PermissionTriStateTemplate : ITemplate
    {
        private readonly PermissionInfo _permission;

        public PermissionTriStateTemplate(PermissionInfo permission)
        {
            this._permission = permission;
        }

        public bool IsFullControl { get; set; }

        public bool IsView { get; set; }

        public bool SupportDenyMode { get; set; }

        public void InstantiateIn(Control container)
        {
            var triState = new PermissionTriState();
            triState.DataBinding += this.BindToTriState;
            container.Controls.Add(triState);
        }

        public void BindToTriState(object sender, EventArgs e)
        {
            var triState = (PermissionTriState)sender;
            var dataRowView = (DataRowView)((DataGridItem)triState.NamingContainer).DataItem;

            triState.Value = dataRowView[this._permission.PermissionName].ToString();
            triState.Locked = !bool.Parse(dataRowView[this._permission.PermissionName + "_Enabled"].ToString());
            triState.SupportsDenyMode = this.SupportDenyMode;
            triState.IsFullControl = this.IsFullControl;
            triState.IsView = this.IsView;
            triState.PermissionKey = this._permission.PermissionKey;
        }
    }
}
