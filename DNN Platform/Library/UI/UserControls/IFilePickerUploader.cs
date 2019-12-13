using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.UI.UserControls
{
    public interface IFilePickerUploader
    {
        int FileID { get; set; }

        string FilePath { get; set; }

        string FileFilter { get; set; }
    }
}
