using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnn.ExportImport.Components
{
    public class Constants
    {
        public const string ExportFolder = "~/Install/Export/export_";
        public const string ExportDateFormat = "yyyyMMddHHmmss";
        public const string ExportDbExt = ".xdb"; // exportDB file extension
        public const string ExportZipExt = ".resources"; // zipped file extension to prevent downloading

        public const string SharedResources = "/DesktopModules/DnnExportImport/App_LocalResources/ExportImport.resx";
    }
}
