#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections.Generic;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.UrlManagement.Components;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.UrlManagement
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PortalAliases : PortAliasesModuleBase
    {
		#region Private Members

        private const string resourcefile = "~/DesktopModules/Admin/UrlManagement/App_LocalResources/SharedResources.resx";
		
		#endregion

		#region Protected Properties

        protected override int ActionColumnIndex
        {
            get { return 2; }
        }

        protected override LinkButton AddAliasButton
        {
            get { return addAliasButton; }
        }

        protected override Label ErrorLabel
        {
            get { return lblError; }
        }

        protected override DataGrid Grid
        {
            get { return portalAiasesGrid; }
        }

		#endregion

		#region Protected Methods

        protected override void GetPortalAliasProperties(int index, PortalAliasInfo portalAlias)
        {
        }

        protected override void OnInit(EventArgs e)
        {
            LocalResourceFile = resourcefile;

            base.OnInit(e);

            portalAiasesGrid.UpdateCommand += SaveAliasesGrid;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                Localization.LocalizeDataGrid(ref portalAiasesGrid, LocalResourceFile);
                // hide language column if there is only 1 language
                if (LocaleController.Instance.GetLocales(PortalId).Count == 1) portalAiasesGrid.Columns[2].Visible = false;
            }

            base.OnLoad(e);

        }

		#endregion

    }
}