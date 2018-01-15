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

#region Usings

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.UI.WebControls
// ReSharper restore CheckNamespace
{

	/// <summary>
	/// The VisibilityControl control provides a base control for defining visibility
	/// options
	/// </summary>
	/// <remarks>
	/// </remarks>
	[ToolboxData("<{0}:VisibilityControl runat=server></{0}:VisibilityControl>")]
	public class VisibilityControl : WebControl, IPostBackDataHandler, INamingContainer
	{
	    protected ProfileVisibility Visibility
	    {
            get { return Value as ProfileVisibility; }
            set { Value = value; }
	    }

		#region Public Properties
		
		/// <summary>
		/// Caption
		/// </summary>
		/// <value>A string representing the Name of the property</value>
		public string Caption { get; set; }

		/// <summary>
		/// Name is the name of the field as a string
		/// </summary>
		/// <value>A string representing the Name of the property</value>
		public string Name { get; set; }

        /// <summary>
        /// The UserInfo object that represents the User whose profile is being displayed
        /// </summary>
        public UserInfo User { get; set; }

		/// <summary>
		/// StringValue is the value of the control expressed as a String
		/// </summary>
		/// <value>A string representing the Value</value>
        public object Value { get; set; }

		#endregion

		#region IPostBackDataHandler Members

		/// <summary>
		/// LoadPostData loads the Post Back Data and determines whether the value has change
		/// </summary>
		/// <param name="postDataKey">A key to the PostBack Data to load</param>
		/// <param name="postCollection">A name value collection of postback data</param>
		public virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			var dataChanged = false;
            var presentVisibility = Visibility.VisibilityMode;
            var postedValue = Convert.ToInt32(postCollection[postDataKey]);
		    var postedVisibility = (UserVisibilityMode) Enum.ToObject(typeof (UserVisibilityMode), postedValue);
            if (!presentVisibility.Equals(postedVisibility) || postedVisibility == UserVisibilityMode.FriendsAndGroups)
			{
                if (postedVisibility == UserVisibilityMode.FriendsAndGroups)
                {
                    var sb = new StringBuilder();
                    sb.Append("G:");
                    foreach (var role in User.Social.Roles)
                    {
                        if (postCollection[postDataKey + ":group_" + role.RoleID.ToString(CultureInfo.InvariantCulture)] != null)
                        {
                            sb.Append(role.RoleID.ToString(CultureInfo.InvariantCulture) + ",");
                        }
                    }

                    sb.Append(";R:");
                    foreach (var relationship in User.Social.Relationships)
                    {
                        if (postCollection[postDataKey + ":relationship_" + relationship.RelationshipId.ToString(CultureInfo.InvariantCulture)] != null)
                        {
                            sb.Append(relationship.RelationshipId.ToString(CultureInfo.InvariantCulture) + ",");
                        }
                    }
                    
                    Value = new ProfileVisibility(User.PortalID, sb.ToString())
                                    {
                                        VisibilityMode = postedVisibility
                                    };                    
                }
                else
                {
                    Value = new ProfileVisibility
                                    {
                                        VisibilityMode = postedVisibility
                                    };
                }

				dataChanged = true;
			}
			return dataChanged;
		}

		/// <summary>
		/// RaisePostDataChangedEvent runs when the PostBackData has changed.  It triggers
		/// a ValueChanged Event
		/// </summary>
		public void RaisePostDataChangedEvent()
		{
			//Raise the VisibilityChanged Event
		    var args = new PropertyEditorEventArgs(Name) {Value = Value};
		    OnVisibilityChanged(args);
		}

		#endregion
		
		#region Events

		public event PropertyChangedEventHandler VisibilityChanged;
		
		#endregion

        #region Private Methods

        private void RenderVisibility(HtmlTextWriter writer, string optionValue, UserVisibilityMode selectedVisibility, string optionText)
        {
            //Render Li
            writer.RenderBeginTag(HtmlTextWriterTag.Li);

            //Render radio button
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
            writer.AddAttribute("aria-label", ID);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, optionValue);
            if ((Visibility.VisibilityMode == selectedVisibility))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.Write(optionText);
            writer.RenderEndTag();

            //Close Li
            writer.RenderEndTag();
        }

        private void RenderCheckboxItem(HtmlTextWriter writer, string prefix, string value, string text, bool selected)
        {
            //Render Li
            writer.RenderBeginTag(HtmlTextWriterTag.Li);

            //Render radio button
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
            writer.AddAttribute("aria-label", ID);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID + prefix + value);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, value);
            if (selected)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.Write(text);
            writer.RenderEndTag();

            //Close Li
            writer.RenderEndTag();

        }

        private void RenderGroups(HtmlTextWriter writer)
        {
            foreach (var group in User.Social.Roles.Where((role) => role.SecurityMode != SecurityMode.SecurityRole))
            {
                RenderCheckboxItem(writer, ":group_", group.RoleID.ToString(CultureInfo.InvariantCulture), 
                                        group.RoleName,
                                        Visibility.RoleVisibilities.Count(r => r.RoleID == group.RoleID) == 1);
            }
        }

        private void RenderRelationships(HtmlTextWriter writer)
        {
            foreach (var relationship in User.Social.Relationships)
            {
                RenderCheckboxItem(writer, ":relationship_", relationship.RelationshipId.ToString(CultureInfo.InvariantCulture), 
                                        relationship.Name,
                                        Visibility.RelationshipVisibilities.Count(r => r.RelationshipId == relationship.RelationshipId) == 1);
            }
        }

        #endregion

        #region Protected Methods

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

            JavaScript.RequestRegistration(CommonJs.jQuery);
		}

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            Page.RegisterRequiresPostBack(this);
			Page.ClientScript.RegisterClientScriptBlock(GetType(), "visibleChange", "$(document).ready(function(){$('.dnnFormVisibility').on('click', 'input[type=radio]', function(){$(this).parent().parent().find('ul').hide();$(this).parent().next('ul').show();});});", true);
        }

		/// <summary>
		/// OnVisibilityChanged runs when the Visibility has changed.  It raises the VisibilityChanged
		/// Event
		/// </summary>
		protected virtual void OnVisibilityChanged(PropertyEditorEventArgs e)
		{
			if (VisibilityChanged != null)
			{
				VisibilityChanged(this, e);
			}
		}

		/// <summary>
		/// Render renders the control
		/// </summary>
		/// <param name="writer">A HtmlTextWriter.</param>
		protected override void Render(HtmlTextWriter writer)
		{
            //Render Div container
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnFormVisibility dnnDropdownSettings");
			writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

            //Render dnnButtonDropdown
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnButtonDropdown");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            //Render dnnButton Icon
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnButtonIcon");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            //writer.AddAttribute(HtmlTextWriterAttribute.Src, IconController.IconURL("Lock"));
            //writer.RenderBeginTag(HtmlTextWriterTag.Img);
            
            //Close Image Tag
            //writer.RenderEndTag();

            //Close dnnButtonIcon
            writer.RenderEndTag();

            //render dnnButton Arrow
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnButtonArrow");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();

            //Close dnnButton Dropdown
            writer.RenderEndTag();

            //Render UL for radio Button List
            //writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnButtonDropdown-ul");

            writer.RenderBeginTag(HtmlTextWriterTag.Ul);

            RenderVisibility(writer, "0", UserVisibilityMode.AllUsers, Localization.GetString("Public").Trim());

            RenderVisibility(writer, "1", UserVisibilityMode.MembersOnly, Localization.GetString("MembersOnly").Trim());

            RenderVisibility(writer, "2", UserVisibilityMode.AdminOnly, Localization.GetString("AdminOnly").Trim());

            RenderVisibility(writer, "3", UserVisibilityMode.FriendsAndGroups, Localization.GetString("FriendsGroups").Trim());

            //Render UL for check Box List
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, Visibility.VisibilityMode == UserVisibilityMode.FriendsAndGroups ? "block" : "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);

		    RenderRelationships(writer);
            RenderGroups(writer);

            //Close UL
            writer.RenderEndTag();

            //Close UL
            writer.RenderEndTag();

            //Close Div
			writer.RenderEndTag();
		}
		
		#endregion
	}
}
