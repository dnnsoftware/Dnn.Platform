#region Usings

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnLiteral : Literal, ILocalizable
    {
        private bool _Localize = true;

        #region "Protected Methods"

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            LocalResourceFile = Utilities.GetLocalResourceFile(this);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            LocalizeStrings();
            base.Render(writer);
        }

        #endregion

        #region "ILocalizable Implementation"

        public bool Localize
        {
            get
            {
                return _Localize;
            }
            set
            {
                _Localize = value;
            }
        }

        public string LocalResourceFile { get; set; }

        public virtual void LocalizeStrings()
        {
            if ((Localize))
            {
                if ((!string.IsNullOrEmpty(Text)))
                {
                    Text = Localization.GetString(Text, LocalResourceFile);
                }
            }
        }

        #endregion
    }
}
