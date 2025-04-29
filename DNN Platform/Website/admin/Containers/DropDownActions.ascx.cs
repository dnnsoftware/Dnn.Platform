// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.NavigationProvider;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.UI.WebControls;

    public partial class DropDownActions : ActionBase
    {
        private NavigationProvider objControl;
        private string strProviderName = "DNNDropDownNavigationProvider";

        public NavigationProvider Control
        {
            get
            {
                return this.objControl;
            }
        }

        public string ProviderName
        {
            get
            {
                return this.strProviderName;
            }

            set
            {
            }
        }

        public void BindDropDown()
        {
            DNNNodeCollection objNodes;
            objNodes = Navigation.GetActionNodes(this.ActionRoot, this);
            foreach (DNNNode objNode in objNodes)
            {
                this.ProcessNodes(objNode);
            }

            this.Control.Bind(objNodes);

            this.Visible = this.DisplayControl(objNodes);
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdGo.Click += this.CmdGo_Click;

            try
            {
                this.BindDropDown();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.objControl = NavigationProvider.Instance(this.ProviderName);
            this.Control.ControlID = "ctl" + this.ID;
            this.Control.Initialize();
            this.spActions.Controls.Add(this.Control.NavigationControl);
        }

        private void Page_Load(object sender, EventArgs e)
        {
            this.cmdGo.Attributes.Add("onclick", "if (cmdGo_OnClick(dnn.dom.getById('" + this.Control.NavigationControl.ClientID + "')) == false) return false;");
        }

        private void CmdGo_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                var cboActions = (DropDownList)this.Control.NavigationControl;
                if (cboActions.SelectedIndex != -1)
                {
                    this.ProcessAction(cboActions.SelectedItem.Value);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void ProcessNodes(DNNNode objParent)
        {
            if (!string.IsNullOrEmpty(objParent.JSFunction))
            {
                ClientAPI.RegisterClientVariable(this.Page, "__dnn_CSAction_" + this.Control.NavigationControl.ClientID + "_" + objParent.ID, objParent.JSFunction, true);
            }

            objParent.ClickAction = eClickAction.None; // since GO button is handling actions dont allow selected index change fire postback

            foreach (DNNNode objNode in objParent.DNNNodes)
            {
                this.ProcessNodes(objNode);
            }
        }
    }
}
