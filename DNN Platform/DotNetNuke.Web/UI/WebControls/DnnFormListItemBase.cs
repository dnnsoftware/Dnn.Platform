#region Usings

using System.Collections;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public abstract class DnnFormListItemBase : DnnFormItemBase
    {
        private IEnumerable _listSource;

        public string DefaultValue { get; set; }

        public IEnumerable ListSource
        {
            get
            {
                return _listSource;
            }
            set
            {
                var changed = !Equals(_listSource, value);
                if (changed)
                {
                    _listSource = value;
                    BindList();
                }
            }
        }

        public string ListTextField { get; set; }

        public string ListValueField { get; set; }

        protected virtual void BindList()
        {
        }
    }
}
