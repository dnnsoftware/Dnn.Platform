// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets
{
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
    using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;

    public partial class PreviewPanelControl : System.Web.UI.UserControl
    {
        protected string Title
        {
            get
            {
                return this.PreviewInfo.Title;
            }
        }

        protected string PreviewImageUrl
        {
            get
            {
                return this.PreviewInfo.PreviewImageUrl;
            }
        }

        protected PreviewInfoViewModel PreviewInfo { get; private set; }

        protected IDigitalAssetsController Controller { get; private set; }

        protected ModuleInfo ModuleConfiguration { get; private set; }

        public void SetPreviewInfo(PreviewInfoViewModel previewInfoViewModel)
        {
            this.PreviewInfo = previewInfoViewModel;
            if (this.FieldsControl != null && this.PreviewInfo != null)
            {
                var fieldsControl = (PreviewFieldsControl)this.FieldsControl;
                fieldsControl.Fields = this.PreviewInfo.Fields;
                fieldsControl.GenerateFieldsTable();
            }
        }

        public void SetController(IDigitalAssetsController damController)
        {
            this.Controller = damController;
        }

        public void SetModuleConfiguration(ModuleInfo moduleConfiguration)
        {
            this.ModuleConfiguration = moduleConfiguration;
        }
    }
}
