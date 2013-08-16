using System;
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.WebControls;

namespace DotNetNuke.Web.UI.WebControls
{
    /// <summary>
    /// Creates a control that renders its childs as a bulleted list.
    /// </summary>
    /// <remarks>
    /// Control renders an unordered list HTML contol. 
    /// Each child control in <see cref="DnnUnsortedList"/> is rendered as a separate list item.
    /// To obtain a control over list item style, add a <see cref="DnnUnsortedListItem" /> to a controls list, 
    /// and tune this object appropriately.
    /// </remarks>
    public class DnnUnsortedList : WebControl, INamingContainer
    {
        private UniformControlCollection<DnnUnsortedList, DnnUnsortedListItem> _listItems = null;

        public DnnUnsortedList() : base(HtmlTextWriterTag.Ul)
        {
        }

        protected override sealed ControlCollection CreateControlCollection()
        {
            return new TypedControlCollection<DnnUnsortedListItem>(this);
        }

        [PersistenceMode(PersistenceMode.InnerDefaultProperty), MergableProperty(false)]
        public virtual UniformControlCollection<DnnUnsortedList, DnnUnsortedListItem> ListItems
        {
            get
            {
                return _listItems ?? (_listItems = new UniformControlCollection<DnnUnsortedList, DnnUnsortedListItem>(this));
            }
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            if (!string.IsNullOrEmpty(CssClass))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
            }
        }

        /// <summary>
        /// A "macro" that adds a set of controls or control as a single list item (li).  Use ListItems.Add(UnsortedListItem) method
        /// </summary>
        /// <remarks>
        /// All controls from the list will be rendered as a childs of a single list item.
        /// </remarks>
        public void AddListItem(params Control[] listItemControls)
        {
            var listItem = new DnnUnsortedListItem();
            listItem.AddControls(listItemControls);
            ListItems.Add(listItem);
        }

    }

}
