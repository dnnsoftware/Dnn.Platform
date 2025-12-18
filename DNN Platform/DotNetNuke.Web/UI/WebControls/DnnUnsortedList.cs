// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <summary>Creates a control that renders its childs as a bulleted list.</summary>
    /// <remarks>
    /// Control renders an unordered list HTML contol.
    /// Each child control in <see cref="DnnUnsortedList"/> is rendered as a separate list item.
    /// To obtain a control over list item style, add a <see cref="DnnUnsortedListItem" /> to a controls list,
    /// and tune this object appropriately.
    /// </remarks>
    public class DnnUnsortedList : WebControl, INamingContainer
    {
        private UniformControlCollection<DnnUnsortedList, DnnUnsortedListItem> listItems = null;

        /// <summary>Initializes a new instance of the <see cref="DnnUnsortedList"/> class.</summary>
        public DnnUnsortedList()
            : base(HtmlTextWriterTag.Ul)
        {
        }

        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        [MergableProperty(false)]
        public virtual UniformControlCollection<DnnUnsortedList, DnnUnsortedListItem> ListItems
        {
            get
            {
                return this.listItems ?? (this.listItems = new UniformControlCollection<DnnUnsortedList, DnnUnsortedListItem>(this));
            }
        }

        /// <summary>A "macro" that adds a set of controls or control as a single list item (li).  Use ListItems.Add(UnsortedListItem) method.</summary>
        /// <remarks>All controls from the list will be rendered as children of a single list item.</remarks>
        /// <param name="listItemControls">The controls to add to a new list item.</param>
        public void AddListItem(params Control[] listItemControls)
        {
            var listItem = new DnnUnsortedListItem();
            listItem.AddControls(listItemControls);
            this.ListItems.Add(listItem);
        }

        /// <inheritdoc/>
        protected override sealed ControlCollection CreateControlCollection()
        {
            return new TypedControlCollection<DnnUnsortedListItem>(this);
        }

        /// <inheritdoc/>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
            }
        }
    }
}
