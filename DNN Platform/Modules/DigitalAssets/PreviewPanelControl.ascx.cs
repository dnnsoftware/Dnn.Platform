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

using DotNetNuke.Entities.Modules;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;

namespace DotNetNuke.Modules.DigitalAssets
{
    public partial class PreviewPanelControl : System.Web.UI.UserControl
    {
        #region Protected Properties
        protected string Title
        {
            get
            {
                return PreviewInfo.Title;
            }
        }
                
        protected string PreviewImageUrl
        { 
            get
            {
                return PreviewInfo.PreviewImageUrl;
            }
        }

        protected PreviewInfoViewModel PreviewInfo { get; private set; }

        protected IDigitalAssetsController Controller { get; private set; }

        protected ModuleInfo ModuleConfiguration { get; private set; }
        #endregion
        
        #region Public Methods
        public void SetPreviewInfo(PreviewInfoViewModel previewInfoViewModel)
        {
            PreviewInfo = previewInfoViewModel;
            if (FieldsControl != null && PreviewInfo != null)
            {
                var fieldsControl = ((PreviewFieldsControl)FieldsControl);
                fieldsControl.Fields = PreviewInfo.Fields;
                fieldsControl.GenerateFieldsTable();
            }
        }
        
        public void SetController(IDigitalAssetsController damController)
        {
            Controller = damController;
        }

        public void SetModuleConfiguration(ModuleInfo moduleConfiguration)
        {
            ModuleConfiguration = moduleConfiguration;
        }
        #endregion
    }
}