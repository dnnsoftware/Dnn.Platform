#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace DotNetNuke.Web.UI.WebControls.Extensions
{
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