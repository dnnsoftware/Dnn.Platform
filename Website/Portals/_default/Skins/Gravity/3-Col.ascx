<%@ Control language="vb" AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Skins.Skin" %>
<%@ Register TagPrefix="dnn" TagName="LANGUAGE" Src="~/Admin/Skins/Language.ascx" %>
<%@ Register TagPrefix="dnn" TagName="LOGO" Src="~/Admin/Skins/Logo.ascx" %>
<%@ Register TagPrefix="dnn" TagName="SEARCH" Src="~/Admin/Skins/Search.ascx" %>
<%@ Register TagPrefix="dnn" TagName="BREADCRUMB" Src="~/Admin/Skins/BreadCrumb.ascx" %>
<%@ Register TagPrefix="dnn" TagName="USER" Src="~/Admin/Skins/User.ascx" %>
<%@ Register TagPrefix="dnn" TagName="LOGIN" Src="~/Admin/Skins/Login.ascx" %>
<%@ Register TagPrefix="dnn" TagName="PRIVACY" Src="~/Admin/Skins/Privacy.ascx" %>
<%@ Register TagPrefix="dnn" TagName="TERMS" Src="~/Admin/Skins/Terms.ascx" %>
<%@ Register TagPrefix="dnn" TagName="COPYRIGHT" Src="~/Admin/Skins/Copyright.ascx" %>
<%@ Register TagPrefix="dnn" TagName="LINKTOMOBILE" Src="~/Admin/Skins/LinkToMobileSite.ascx" %>
<%@ Register TagPrefix="dnn" TagName="MENU" src="~/DesktopModules/DDRMenu/Menu.ascx" %>
<div id="siteWrapper">
    <div id="userControls" class="wrapper">
        <div id="search">
            <dnn:SEARCH ID="dnnSearch" runat="server" ShowSite="false" ShowWeb="false" EnableTheming="true" Submit="Search" CssClass="SearchButton" />
        </div><!---/search-->
         <div id="login">
            <dnn:USER ID="dnnUser" runat="server" LegacyMode="false" /> 
            <dnn:LOGIN ID="dnnLogin" CssClass="LoginLink" runat="server" LegacyMode="false" />
       	</div><!--/login-->
        <div class="clear"></div>
    </div><!--/userControls-->
    <div id="siteHeadouter">
        <div id="siteHeadinner" class="wrapper">
            <div id="logo">
                <dnn:LOGO runat="server" id="dnnLOGO" />
            </div><!--/Logo-->
            <div class="right">
				<div class="language">
					<dnn:LANGUAGE runat="server" id="dnnLANGUAGE"  showMenu="False" showLinks="True" />
					<div class="clear"></div>
				</div>
				<div id="nav">
					<dnn:MENU MenuStyle="Simple" runat="server"></dnn:MENU>
				</div><!--/nav-->
			</div>
            <div class="clear"></div>
        </div><!--/siteHeadinner-->    
    </div><!--/siteHeadouter-->
    <div id="contentWrapper" class="wrapper">
		<div id="Breadcrumb"><dnn:BREADCRUMB ID="dnnBreadcrumb" runat="server" CssClass="breadcrumbLink" RootLevel="0" Separator="&lt;img src=&quot;images/breadcrumb.png&quot;&gt;" /></div>    	
        <div>
			<div id="leftPane" class="threeColLeftPane spacingRight" runat="server"></div>
			<div id="centerPane" class="threeColCenterPane spacingRight" runat="server"></div>
			<div id="rightPane" class="threeColRightPane" runat="server"></div>
			<div class="clear"></div>
		</div>
		<div id="contentPane" class="contentPane" runat="server"></div>
        <div id="footer">
        	<div id="footerLeftOuterPane" class="footerPane spacingRight" runat="server"></div>
            <div id="footerLeftPane" class="footerPane spacingRight" runat="server"></div>
            <div id="footerCenterPane" class="footerPane spacingRight" runat="server"></div>
            <div id="footerRightPane" class="footerPane spacingRight" runat="server"></div>
            <div id="footerRightOuterPane" class="footerPaneRight" runat="server"></div>
            <div class="clear"></div>
            <hr/>
            <div id="copyright">
				<div class="right">
					<dnn:LINKTOMOBILE ID="dnnLinkToMobile" runat="server" />
					<dnn:TERMS ID="dnnTerms" runat="server" /> |
					<dnn:PRIVACY ID="dnnPrivacy" runat="server" />
				</div>
				<dnn:COPYRIGHT ID="dnnCopyright" runat="server" />
            </div><!--/copyright-->
        </div><!--/footer-->
	</div><!--/contentWrapper-->
</div><!--/siteWrapper-->

    	
	
	



