// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;

    public partial class PreviewFieldsControl : UserControl
    {
        public List<Field> Fields { get; set; }

        public void GenerateFieldsTable()
        {
            this.FieldsTable.Rows.Clear();
            foreach (var field in this.Fields)
            {
                var cellLabel = new TableCell { Text = field.DisplayName + ":", CssClass = "dnnModuleDigitalAssetsPreviewInfoFieldLabel" };
                var cellValue = new TableCell { Text = field.StringValue, CssClass = "dnnModuleDigitalAssetsPreviewInfoFieldValue" };
                var rowField = new TableRow { Cells = { cellLabel, cellValue }, CssClass = "dnnModuleDigitalAssetsPreviewInfoFieldsRow" };
                this.FieldsTable.Rows.Add(rowField);
            }
        }
    }
}
