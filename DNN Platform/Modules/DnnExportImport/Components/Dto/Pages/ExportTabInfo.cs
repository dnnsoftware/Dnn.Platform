using System;
// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Components.Dto.Pages
{
    public class ExportTabInfo
    {
        public int TabID { get; set; }
        public int ParentID { get; set; }
        public string TabName { get; set; }
        public string TabPath { get; set; }
    }
}