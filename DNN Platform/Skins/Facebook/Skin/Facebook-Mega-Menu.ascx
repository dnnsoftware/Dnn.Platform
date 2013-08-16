<%@ Control language="vb" AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Skins.Skin" %>
<%@ Register TagPrefix="dnn" TagName="LANGUAGE" Src="~/Admin/Skins/Language.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.DDRMenu.TemplateEngine" Assembly="DotNetNuke.Web.DDRMenu" %>
<%@ Register TagPrefix="dnn" TagName="CONTROLPANEL" Src="~/Admin/Skins/controlpanel.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<div id="DNN6" class="Home">
		<div id="Background"></div>
    <div id="Header">
        <div id="ContentBG">
            <div id="ControlPanelWrapper">
                <dnn:CONTROLPANEL runat="server" id="cp"  IsDockable="True" />
		    </div>
		
        </div>
	</div>
    <div id="Content">
        <div id="Panes">
		    <div id="LogoRow">
			     <div class="LogoRowRight">
                    <div id="Login">
					<dnn:LANGUAGE runat="server" id="dnnLANGUAGE"  showMenu="False" showLinks="True" />
					</div>
			      </div>
		    </div>
            <div id="ContentPane" runat="server"></div>
        </div>
    </div>
</div>

<dnn:DnnJsInclude runat="server" FilePath="jquery.cycle.min.js" PathNameAlias="SkinPath" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/Scripts/jquery/jquery.hoverIntent.min.js" />
