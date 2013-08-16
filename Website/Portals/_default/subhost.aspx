<%@ Page Language="C#" %>

<script runat="server">
    
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        PortalSettings settings = PortalController.GetCurrentPortalSettings();
        CultureInfo pageLocale = Localization.GetPageLocale(settings);
        if ((settings != null) && (pageLocale != null))
        {
            Localization.SetThreadCultures(pageLocale, settings);
        }
    }    
    
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

	    var domainName = "";
	    int urlIndex;

		// parse the Request URL into a Domain Name token 
		string[] url = Request.Url.ToString().Split('/');
		for (urlIndex = 2; urlIndex <= url.GetUpperBound(0); urlIndex++)
		{
			bool willExit = false;
			switch (url[urlIndex].ToLower())
			{
				case "admin":
				case "desktopmodules":
				case "mobilemodules":
				case "premiummodules":
					willExit = true;
					break;
				default:
					// check if filename
					if (url[urlIndex].IndexOf(".aspx", StringComparison.Ordinal) == -1)
					{
						domainName = domainName + (!string.IsNullOrEmpty(domainName) ? "/" : "") + url[urlIndex];
					}
					else
					{
						willExit = true;
					}

					break;
			}
			if (willExit)
				break;
		}

		// format the Request.ApplicationPath
		string serverPath = Request.ApplicationPath;
		if (serverPath != null && serverPath.Substring(serverPath.Length - 1, 1) != "/")
		{
			serverPath = serverPath + "/";
		}

        PortalSettings portal = PortalController.GetCurrentPortalSettings();

		var queryString = Request.Url.Query.TrimStart('?');

        if (Request.Url.Query.Length == 0 && portal.HomeTabId > Null.NullInteger)
        {
			Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(portal.HomeTabId, portal, string.Empty, queryString), true);
        }
        else
        {
			domainName = string.Format("{0}Default.aspx?alias={1}&{2}", serverPath, domainName, queryString);

            Response.Redirect(domainName, true);
        }

	}

</script>

