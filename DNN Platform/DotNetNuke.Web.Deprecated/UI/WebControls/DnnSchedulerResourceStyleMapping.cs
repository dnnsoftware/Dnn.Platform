#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnSchedulerResourceStyleMapping : ResourceStyleMapping
    {
        public DnnSchedulerResourceStyleMapping()
        {
        }

        public DnnSchedulerResourceStyleMapping(string type, string key, string applyCssClass) : base(type, key, applyCssClass)
        {
        }

        public DnnSchedulerResourceStyleMapping(string type, string key, string text, string applyCssClass) : base(type, key, text, applyCssClass)
        {
        }
    }
}
