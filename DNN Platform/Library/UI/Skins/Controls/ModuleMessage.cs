#region Usings

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{

    /// <summary></summary>
    /// <remarks></remarks>
    public class ModuleMessage : SkinObjectBase
    {
        #region ModuleMessageType enum

        public enum ModuleMessageType
        {
            GreenSuccess,
            YellowWarning,
            RedError,
            BlueInfo
        }

        #endregion
		#region "Private Members"
		
        protected Panel dnnSkinMessage;
        protected Label lblHeading;
        protected Label lblMessage;
    	protected Control scrollScript;
		
		#endregion
		
		#region "Public Members"

        public string Text { get; set; }

        public string Heading { get; set; }

        public ModuleMessageType IconType { get; set; }

        public string IconImage { get; set; }

		/// <summary>
		/// Check this message is shown as page message or module message.
		/// </summary>
    	public bool IsModuleMessage
    	{
    		get
    		{
    			return this.Parent.ID == "MessagePlaceHolder";
    		}
    	}

        #endregion

		#region "Protected Methods"

		/// <summary>
		/// The Page_Load server event handler on this page is used
		/// to populate the role information for the page
		/// </summary>
		protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                var strMessage = "";
				
				//check to see if a url
                //was passed in for an icon
                if (!String.IsNullOrEmpty(IconImage))
                {
                    strMessage += Text;
                    dnnSkinMessage.CssClass = "dnnFormMessage dnnFormWarning";
                }
                else
                {
                    switch (IconType)
                    {
                        case ModuleMessageType.GreenSuccess:
                            strMessage += Text;
                            dnnSkinMessage.CssClass = "dnnFormMessage dnnFormSuccess";
                            break;
                        case ModuleMessageType.YellowWarning:
                            strMessage += Text;
                            dnnSkinMessage.CssClass = "dnnFormMessage dnnFormWarning";
                            break;
                        case ModuleMessageType.BlueInfo:
                            strMessage += Text;
                            dnnSkinMessage.CssClass = "dnnFormMessage dnnFormInfo";
                            break;
                        case ModuleMessageType.RedError:
                            strMessage += Text;
                            dnnSkinMessage.CssClass = "dnnFormMessage dnnFormValidationSummary";
                            break;
                    }
                }
                lblMessage.Text = strMessage;

                if (!String.IsNullOrEmpty(Heading))
                {
                    lblHeading.Visible = true;
                    lblHeading.Text = Heading;
                }
            }
            catch (Exception exc) //Control failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc, false);
            }
        }

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			//set the scroll js only shown for module message and in postback mode.
			scrollScript.Visible = IsPostBack && IsModuleMessage;
		}
		
		#endregion
    }
}
