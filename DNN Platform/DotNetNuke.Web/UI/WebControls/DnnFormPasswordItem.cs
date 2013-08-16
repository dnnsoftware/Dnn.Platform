#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Web.Client.ClientResourceManagement;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFormPasswordItem : DnnFormItemBase
    {
        private TextBox _password;

        public string TextBoxCssClass
        {
            get
            {
                return ViewState.GetValue("TextBoxCssClass", string.Empty);
            }
            set
            {
                ViewState.SetValue("TextBoxCssClass", value, string.Empty);
            }
        }

        public string ContainerCssClass
        {
            get
            {
                return ViewState.GetValue("ContainerCssClass", string.Empty);
            }
            set
            {
                ViewState.SetValue("ContainerCssClass", value, string.Empty);
            }
        }

        private void TextChanged(object sender, EventArgs e)
        {
            UpdateDataSource(Value, _password.Text, DataField);
        }

        /// <summary>
        /// Use container to add custom control hierarchy to
        /// </summary>
        /// <param name="container"></param>
        /// <returns>An "input" control that can be used for attaching validators</returns>
        protected override WebControl CreateControlInternal(Control container)
        {
            _password = new TextBox()
            {
                ID = ID + "_TextBox",
                TextMode = TextBoxMode.Password,
                CssClass = TextBoxCssClass,
                MaxLength = 20, //ensure password cannot be cut if too long
                Text = Convert.ToString(Value) // Load from ControlState
            };
            _password.TextChanged += TextChanged;

            var passwordContainer = new Panel() { ID = "passwordContainer", CssClass = ContainerCssClass };

            // add control hierarchy to the container
            container.Controls.Add(passwordContainer);

            passwordContainer.Controls.Add(_password);

            // return input control that can be used for validation
            return _password;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.jquery.tooltip.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.PasswordStrength.js");

            jQuery.RequestDnnPluginsRegistration();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            var options = new DnnPaswordStrengthOptions();
            var optionsAsJsonString = Json.Serialize(options);
            var script = string.Format("dnn.initializePasswordStrength('.{0}', {1});{2}",
                TextBoxCssClass, optionsAsJsonString, Environment.NewLine);

            if (ScriptManager.GetCurrent(Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(Page, GetType(), "PasswordStrength", script, true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(GetType(), "PasswordStrength", script, true);
            }

        }

    }

}
