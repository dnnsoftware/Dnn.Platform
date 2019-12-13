#region Usings

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;


#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnLanguageLabel : CompositeControl, ILocalizable
    {
        #region Controls

        private Image _Flag;

        private Label _Label;

        #endregion

        public DnnLanguageLabel()
        {
            Localize = true;
        }

        #region Public Properties

        public CultureDropDownTypes DisplayType { get; set; }

        public string Language
        {
            get
            {
                return (string) ViewState["Language"];
            }
            set
            {
                ViewState["Language"] = value;
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            LocalResourceFile = Utilities.GetLocalResourceFile(this);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   CreateChildControls overrides the Base class's method to correctly build the
        ///   control based on the configuration
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void CreateChildControls()
        {
            //First clear the controls collection
            Controls.Clear();

            _Flag = new Image {ViewStateMode = ViewStateMode.Disabled};
            Controls.Add(_Flag);

            Controls.Add(new LiteralControl("&nbsp;"));

            _Label = new Label();
            _Label.ViewStateMode = ViewStateMode.Disabled;
            Controls.Add(_Label);

            //Call base class's method

            base.CreateChildControls();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   OnPreRender runs just before the control is rendered
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (string.IsNullOrEmpty(Language))
            {
                _Flag.ImageUrl = "~/images/Flags/none.gif";
            }
            else
            {
                _Flag.ImageUrl = string.Format("~/images/Flags/{0}.gif", Language);
            }

            if (DisplayType == 0)
            {
                PortalSettings _PortalSettings = PortalController.Instance.GetCurrentPortalSettings();
                string _ViewTypePersonalizationKey = "ViewType" + _PortalSettings.PortalId;
                string _ViewType = Convert.ToString(Personalization.GetProfile("LanguageDisplayMode", _ViewTypePersonalizationKey));
                switch (_ViewType)
                {
                    case "NATIVE":
                        DisplayType = CultureDropDownTypes.NativeName;
                        break;
                    case "ENGLISH":
                        DisplayType = CultureDropDownTypes.EnglishName;
                        break;
                    default:
                        DisplayType = CultureDropDownTypes.DisplayName;
                        break;
                }
            }

            string localeName = null;
            if (string.IsNullOrEmpty(Language))
            {
                localeName = Localization.GetString("NeutralCulture", Localization.GlobalResourceFile);
            }
            else
            {
                localeName = Localization.GetLocaleName(Language, DisplayType);
            }
            _Label.Text = localeName;
            _Flag.AlternateText = localeName;
        }

        #endregion

        #region ILocalizable Implementation

        public bool Localize { get; set; }

        public string LocalResourceFile { get; set; }

        public virtual void LocalizeStrings()
        {
        }

        #endregion
    }
}
