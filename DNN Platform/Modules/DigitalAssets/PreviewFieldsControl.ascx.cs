// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;

namespace DotNetNuke.Modules.DigitalAssets
{
    public partial class PreviewFieldsControl : UserControl
    {
        public void GenerateFieldsTable()
        {
            FieldsTable.Rows.Clear();
            foreach (var field in Fields)
            {
                var cellLabel = new TableCell { Text = field.DisplayName + ":", CssClass = "dnnModuleDigitalAssetsPreviewInfoFieldLabel" };
                var cellValue = new TableCell { Text = field.StringValue, CssClass = "dnnModuleDigitalAssetsPreviewInfoFieldValue" };
                var rowField = new TableRow { Cells = { cellLabel, cellValue }, CssClass = "dnnModuleDigitalAssetsPreviewInfoFieldsRow" };
                FieldsTable.Rows.Add(rowField);
            }
        }

        public List<Field> Fields { get; set; }
    
    }
}
