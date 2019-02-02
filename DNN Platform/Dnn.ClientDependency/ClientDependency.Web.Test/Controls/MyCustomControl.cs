using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using ClientDependency.Core;

namespace ClientDependency.Web.Test.Controls
{

    /// <summary>
    /// This simply demonstrates using attributes to register file dependencies.
    /// </summary>
    [ClientDependency(ClientDependencyType.Css, "~/Css/CustomControl.css")]
    public class MyCustomControl : Control
    {

        private HtmlGenericControl m_MainDiv;
        

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            m_MainDiv = new HtmlGenericControl();
            //m_MainDiv.ID = "myControl";
            m_MainDiv.Attributes.Add("class", "myControl");
            m_MainDiv.Controls.Add(new LiteralControl("<div>My Custom Control</div>"));

            this.Controls.Add(m_MainDiv);
        }

    }
}
