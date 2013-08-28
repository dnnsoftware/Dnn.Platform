// 
// *NOTE: Please manage your content in the  associated [CONTROL].html file in this folder
// 

<%@ Control Language="C#" ClassName="[OWNER].[MODULE].[CONTROL]" Inherits="DotNetNuke.Entities.Modules.PortalModuleBase" %>

<%@ Import Namespace="System.IO" %>

<script runat="server">


	#region Copyright

	// 
	// Copyright (c) [YEAR]
	// by [OWNER]
	// 

	#endregion

	#region Event Handlers

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

                //Only required if the HTML content uses jQuery or the Services Framework
                jQuery.RequestRegistration();
                ServicesFramework.Instance.RequestAjaxScriptSupport();
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
			
		if (!Page.IsPostBack)
		{
                        //Load the HTML file
			var path = Server.MapPath(ModulePath) + "[CONTROL].html";
			if (File.Exists(path))
			{
  			    var content = Null.NullString;
            		    TextReader tr = new StreamReader(path);
			    content = tr.ReadToEnd();
            		    tr.Close();
                            ctlContent.Text = content;
                        }
		}
	}

	#endregion

</script>


<asp:Localize id="ctlContent" runat="server" />
