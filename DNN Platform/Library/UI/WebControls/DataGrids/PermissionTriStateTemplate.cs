#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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