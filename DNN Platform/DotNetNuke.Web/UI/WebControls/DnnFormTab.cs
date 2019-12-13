#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    [ParseChildren(true)]
    public class DnnFormTab : WebControl, INamingContainer
    {
        public DnnFormTab()
        {
            Sections = new List<DnnFormSection>();
            Items = new List<DnnFormItemBase>();
        }

        public bool IncludeExpandAll { get; set; }

        internal string ExpandAllScript { get; set; }

        [Category("Behavior"), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormItemBase> Items { get; private set; }

        [Category("Behavior"), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormSection> Sections { get; private set; }

        public string ResourceKey { get; set; }
    }
}
