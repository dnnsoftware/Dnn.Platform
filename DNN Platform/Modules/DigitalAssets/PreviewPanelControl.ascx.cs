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
