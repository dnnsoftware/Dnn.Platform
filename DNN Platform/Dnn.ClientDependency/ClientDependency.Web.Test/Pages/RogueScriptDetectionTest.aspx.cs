using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core.Controls;

namespace ClientDependency.Web.Test.Pages
{
    public partial class RogueScriptDetectionTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //dynamically change the provider for this page
            ClientDependencyLoader.GetInstance(new HttpContextWrapper(this.Context))
                .ProviderName = "LoaderControlProvider";
        }
    }
}
