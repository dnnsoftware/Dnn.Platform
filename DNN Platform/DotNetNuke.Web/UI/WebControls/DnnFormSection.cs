#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    [ParseChildren(true)]
    public class DnnFormSection : WebControl, INamingContainer
    {
        public DnnFormSection()
        {
            Items = new List<DnnFormItemBase>();
        }

        public bool Expanded { get; set; }

        [Category("Behavior"), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormItemBase> Items { get; private set; }

        public string ResourceKey { get; set; }
    }
}
