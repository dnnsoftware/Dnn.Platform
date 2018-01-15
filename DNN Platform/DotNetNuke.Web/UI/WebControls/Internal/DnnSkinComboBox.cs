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
using System.Collections;
using System.Linq;
using System.Web.UI;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.UI.Skins;

namespace DotNetNuke.Web.UI.WebControls.Internal
{
    ///<remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    [ToolboxData("<{0}:DnnSkinComboBox runat='server'></{0}:DnnSkinComboBox>")]
    public class DnnSkinComboBox : DnnComboBox
    {
        #region Public Properties

        public int PortalId { get; set; }

        public string RootPath { get; set; }

        public SkinScope Scope { get; set; }

        public bool IncludeNoneSpecificItem { get; set; }

        public string NoneSpecificText { get; set; }

        #endregion

        #region Private Properties

        private PortalInfo Portal
        {
            get { return PortalId == Null.NullInteger ? null : PortalController.Instance.GetPortal(PortalId); }
        }

        #endregion

        #region Constructors

        public DnnSkinComboBox()
        {
            PortalId = Null.NullInteger;
        }

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DataTextField = "Key";
            DataValueField = "Value";

            if (!Page.IsPostBack && !string.IsNullOrEmpty(RootPath))
            {
                DataSource = SkinController.GetSkins(Portal, RootPath, Scope)
                                           .ToDictionary(skin => skin.Key, skin => skin.Value);

                if (string.IsNullOrEmpty(SelectedValue))
                {
                    DataBind();
                }
                else
                {
                    DataBind(SelectedValue);
                }

                if (IncludeNoneSpecificItem)
                {
                    InsertItem(0, NoneSpecificText, string.Empty);
                }
            }

            AttachEvents();
        }

        protected override void PerformDataBinding(IEnumerable dataSource)
        {
            //do not select item during data binding, item will select later
            var selectedValue = SelectedValue;
            SelectedValue = null;

            base.PerformDataBinding(dataSource);

            SelectedValue = selectedValue;
        }

        #endregion

        #region Private Methods

        private void AttachEvents()
        {
            if (!UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                return;
            }

            Attributes.Add("PortalPath", Portal != null ? Portal.HomeDirectory : string.Empty);
            Attributes.Add("HostPath", Globals.HostPath);

            //OnClientSelectedIndexChanged = "selectedIndexChangedMethod";
            var indexChangedMethod = @"function selectedIndexChangedMethod(sender, eventArgs){
    var value = eventArgs.get_item().get_value();
    value = value.replace('[L]', sender.get_attributes().getAttribute('PortalPath'));
    value = value.replace('[G]', sender.get_attributes().getAttribute('HostPath'));
    sender.get_inputDomElement().title = value;
}";
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "OnClientSelectedIndexChanged", indexChangedMethod, true);

            //foreach (var item in Items)
            //{
            //    if (string.IsNullOrEmpty(item.Value))
            //    {
            //        continue;
            //    }

            //    var tooltip = item.Value.Replace("[G]", Globals.HostPath);
            //    if (Portal != null)
            //    {
            //        tooltip = tooltip.Replace("[L]", Portal.HomeDirectory);
            //    }

            //    item.ToolTip = tooltip;
            //    if (item.Value.Equals(SelectedValue))
            //    {
            //        ToolTip = tooltip;
            //    }
            //}
        }

        #endregion
    }
}
