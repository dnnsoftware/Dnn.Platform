// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Localization;
    using Telerik.Web.UI;

    public class DnnGridColumn : GridColumn
    {
        private string _HeaderText;

        public string LocalResourceFile
        {
            get
            {
                return Utilities.GetLocalResourceFile(this.Owner.OwnerGrid.Parent);
            }
        }

        public override string HeaderText
        {
            get
            {
                if (string.IsNullOrEmpty(base.HeaderText))
                {
                    base.HeaderText = Localization.GetString(string.Format("{0}.Header", this._HeaderText), DotNetNuke.Web.UI.Utilities.GetLocalResourceFile(this.Owner.OwnerGrid.Parent));
                }

                return base.HeaderText;
            }

            set
            {
                this._HeaderText = value;
                base.HeaderText = string.Empty;
            }
        }

        public string setHeaderText
        {
            set
            {
                base.HeaderText = value;
            }
        }

        public override GridColumn Clone()
        {
            var dnnGridColumn = new DnnGridColumn();
            dnnGridColumn.CopyBaseProperties(this);
            dnnGridColumn.setHeaderText = this.HeaderText;
            return dnnGridColumn;
        }
    }
}
