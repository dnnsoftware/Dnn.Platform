using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core.Controls;
using ClientDependency.Core.Providers;

namespace ClientDependency.Core.Web.Test.Pages
{
    public partial class LazyLoadProvider : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Changes the provider to be used at runtime
            ClientDependencyLoader.Instance.ProviderName = ClientSideRegistrationProvider.DefaultName;

            //dynamically register the dependency
            ClientDependencyLoader.Instance.RegisterDependency("Content.css", "Styles", ClientDependencyType.Css);

        }
    }
}
