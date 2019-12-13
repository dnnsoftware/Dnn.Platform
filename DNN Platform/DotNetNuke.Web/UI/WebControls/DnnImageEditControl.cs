using DotNetNuke.Common;

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnImageEditControl : DnnFileEditControl
    {
        public DnnImageEditControl()
        {
            FileFilter = Globals.glbImageFileTypes;
        }
    }
}
