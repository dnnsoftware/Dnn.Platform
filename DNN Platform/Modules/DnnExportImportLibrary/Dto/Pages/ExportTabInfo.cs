﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Dto.Pages
{
    public class ExportTabInfo
    {
        public int TabID { get; set; }
        public int ParentID { get; set; }
        public string TabName { get; set; }
        public string TabPath { get; set; }
    }
}
