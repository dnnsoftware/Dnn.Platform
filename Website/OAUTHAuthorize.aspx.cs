using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OAuth.AuthorizationServer.Core.Server;

public partial class OAUTHAuthorize : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
      

    }

    protected object GetOptionalState()
    {

        if (Request.QueryString["redirect_uri"] != null)
        {
            var state = Request.Url.ToString();
            int iqs = state.IndexOf('?');
            String querystring = null;
            if (iqs == -1)
            {
                //String redirecturl = currurl + "?var1=1&var2=2+2%2f3&var1=3";
                //Response.Redirect(redirecturl, true);
            }
            // If query string variables exist, put them in
            // a string.
            else if (iqs >= 0)
            {
                querystring = (iqs < state.Length - 1) ? state.Substring(iqs + 1) : String.Empty;
            }

            // Parse the query string variables into a NameValueCollection.
            NameValueCollection qscoll = HttpUtility.ParseQueryString(querystring);
            
        }
        return string.Empty;
    }
}