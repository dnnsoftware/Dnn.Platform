<%@ Control language="vb" AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Skins.Skin" %>
<%@ Register TagPrefix="dnn" TagName="LANGUAGE" Src="~/Admin/Skins/Language.ascx" %>
<%@ Register TagPrefix="dnn" TagName="LOGO" Src="~/Admin/Skins/Logo.ascx" %>
<%@ Register TagPrefix="dnn" TagName="PRIVACY" Src="~/Admin/Skins/Privacy.ascx" %>
<%@ Register TagPrefix="dnn" TagName="TERMS" Src="~/Admin/Skins/Terms.ascx" %>
<%@ Register TagPrefix="dnn" TagName="COPYRIGHT" Src="~/Admin/Skins/Copyright.ascx" %>
<%@ Register TagPrefix="dnn" TagName="LINKTOMOBILE" Src="~/Admin/Skins/LinkToMobileSite.ascx" %>
<%@ Register TagPrefix="dnn" TagName="META" Src="~/Admin/Skins/Meta.ascx" %>
<%@ Register TagPrefix="dnn" TagName="MENU" src="~/DesktopModules/DDRMenu/Menu.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<dnn:META ID="META1" runat="server" Name="viewport" Content="width=device-width,initial-scale=1" />

<dnn:DnnJsInclude ID="bootstrapJS" runat="server" FilePath="bootstrap/js/bootstrap.min.js" PathNameAlias="SkinPath" AddTag="false" />
<dnn:DnnJsInclude ID="customJS" runat="server" FilePath="js/scripts.js" PathNameAlias="SkinPath" AddTag="false" />

<div id="siteWrapper">
    <div id="userControls" class="container">
        <div class="row-fluid">
            <div class="span2 language pull-left">
                <dnn:LANGUAGE runat="server" id="LANGUAGE1"  showMenu="False" showLinks="True" />
            </div>
            <div id="search" class="span3 pull-right">
                <!-- No Search box -->
            </div>
            <div id="login" class="span5 pull-right">
                <!-- No Login Control -->
                <!-- No User Control -->
            </div>
        </div>
    </div>
    <div id="siteHeadouter">
        <div id="siteHeadinner" class="container">
        	<div class="navbar">
            	<div class="navbar-inner">
                    <span class="brand visible-desktop">
                        <dnn:LOGO runat="server" id="dnnLOGO" />
                    </span>
                    <span class="brand hidden-desktop">
                        <dnn:LOGO runat="server" id="dnnLOGOmobi" />
                    </span>
                    <a class="btn btn-navbar" data-toggle="collapse" data-target=".nav-collapse">Menu</a>
                    <div id="navdttg" class="nav-collapse collapse pull-right">
                        <dnn:MENU ID="bootstrapNav" MenuStyle="bootstrapNav" runat="server"></dnn:MENU>
                    </div>
                </div>
			</div>
        </div>
    </div>
    <div id="contentWrapper">
        <div class="container">
            <div id="content">
                <div class="row-fluid">
		            <div id="ContentPane" class="contentPane" runat="server"></div>
                </div>
                <div class="row-fluid">
				    <div id="leftPane" class="span8 leftPane spacingTop" runat="server"></div>
				    <div id="sidebarPane" class="span4 sidebarPane spacingTop" runat="server"></div>
			    </div>
                <div class="row-fluid">
			        <div id="contentPaneLower" class="span12 contentPane spacingTop" runat="server"></div>
                </div>
            </div>
            <div id="footer">
                <div class="row-fluid">
        	        <div id="footerLeftOuterPane" class="span2 footerPane" runat="server"></div>
                    <div id="footerLeftPane" class="span2 footerPane" runat="server"></div>
                    <div id="footerCenterPane" class="span2 footerPane" runat="server"></div>
                    <div id="footerRightPane" class="span2 footerPane" runat="server"></div>
                    <div id="footerRightOuterPane" class="span2 offset2 footerPaneRight" runat="server"></div>
                </div>
                <div class="row-fluid">
                    <hr class="span12"/>
                </div>
                <div id="copyright" class="row-fluid">
				    <div class="pull-right">
					    <dnn:LINKTOMOBILE ID="dnnLinkToMobile" runat="server" />
					    <dnn:TERMS ID="dnnTerms" runat="server" /> |
					    <dnn:PRIVACY ID="dnnPrivacy" runat="server" />
				    </div>
				    <dnn:COPYRIGHT ID="dnnCopyright" runat="server" CssClass="pull-left" />
                </div>
            </div>
        </div>
	</div>
</div>
<dnn:DnnJsInclude ID="dttg" runat="server" FilePath="js/doubletaptogo.min.js" PathNameAlias="SkinPath" AddTag="false" />
<script type="text/javascript">
     $(function () {
          $('#navdttg li:has(ul)').doubleTapToGo();
     });
</script>