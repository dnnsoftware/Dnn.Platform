// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Security.Permissions;

namespace DotNetNuke.UI.WebControls.Internal
{
    class PermissionTriStateTemplate : ITemplate
    {
        private readonly PermissionInfo _permission;

        public PermissionTriStateTemplate(PermissionInfo permission)
        {
            _permission = permission;
        }
        public void InstantiateIn(Control container)
        {
            var triState = new PermissionTriState();
            triState.DataBinding += BindToTriState;
            container.Controls.Add(triState);
        }
        
        public void BindToTriState(object sender, EventArgs e)
        {
            var triState = (PermissionTriState) sender;
            var dataRowView = ((DataRowView) ((DataGridItem)triState.NamingContainer).DataItem);

            triState.Value = dataRowView[_permission.PermissionName].ToString();
            triState.Locked = !bool.Parse(dataRowView[_permission.PermissionName + "_Enabled"].ToString());
            triState.SupportsDenyMode = SupportDenyMode;
            triState.IsFullControl = IsFullControl;
            triState.IsView = IsView;
            triState.PermissionKey = _permission.PermissionKey;
        }

        public bool IsFullControl { get; set; }

        public bool IsView { get; set; }

        public bool SupportDenyMode { get; set; }
    }
}
