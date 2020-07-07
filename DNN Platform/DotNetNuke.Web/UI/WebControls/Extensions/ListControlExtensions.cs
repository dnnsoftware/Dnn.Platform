// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls.Extensions
{
    using System;
    using System.Linq;
    using System.Web.UI.WebControls;

    public static class ListControlExtensions
    {
        public static void AddItem(this ListControl control, string text, string value)
        {
            control.Items.Add(new ListItem(text, value));
        }

        public static void InsertItem(this ListControl control, int index, string text, string value)
        {
            control.Items.Insert(index, new ListItem(text, value));
        }

        public static void DataBind(this ListControl control, string initialValue)
        {
            DataBind(control, initialValue, false);
        }

        public static void DataBind(this ListControl control, string initial, bool findByText)
        {
            control.DataBind();

            Select(control, initial, findByText);
        }

        public static void Select(this ListControl control, string initial)
        {
            Select(control, initial, false, -1);
        }

        public static void Select(this ListControl control, string initial, bool findByText)
        {
            Select(control, initial, findByText, -1);
        }

        public static void Select(this ListControl control, string initial, bool findByText, int fallbackIndex)
        {
            control.ClearSelection();
            if (findByText)
            {
                if (control.Items.FindByTextWithIgnoreCase(initial) != null)
                {
                    control.Items.FindByTextWithIgnoreCase(initial).Selected = true;
                }
                else if (fallbackIndex > -1)
                {
                    control.Items[fallbackIndex].Selected = true;
                }
            }
            else
            {
                if (control.Items.FindByValueWithIgnoreCase(initial) != null)
                {
                    control.Items.FindByValueWithIgnoreCase(initial).Selected = true;
                }
                else if (fallbackIndex > -1)
                {
                    control.Items[fallbackIndex].Selected = true;
                }
            }
        }

        /// <summary>
        /// Use this method instead of ListItemCollection.FindByText to find the specific item with case-insensitive.
        /// </summary>
        /// <param name="listItems">the items.</param>
        /// <param name="text">the item with this text want to find.</param>
        /// <returns>the specific item or null if didn't match the text with any item.</returns>
        public static ListItem FindByTextWithIgnoreCase(this ListItemCollection listItems, string text)
        {
            return listItems.Cast<ListItem>().FirstOrDefault(item => item.Text.Equals(text, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Use this method instead of ListItemCollection.FindBValue to find the specific item with case-insensitive.
        /// </summary>
        /// <param name="listItems">the items.</param>
        /// <param name="value">the item with this value want to find.</param>
        /// <returns>the specific item or null if didn't match the value with any item.</returns>
        public static ListItem FindByValueWithIgnoreCase(this ListItemCollection listItems, string value)
        {
            return listItems.Cast<ListItem>().FirstOrDefault(item => item.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
