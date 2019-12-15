// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
