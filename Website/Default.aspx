<%@ Page Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Framework.DefaultPage" CodeFile="Default.aspx.cs" %>
<%@ Register TagPrefix="dnncrm" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Common.Controls" Assembly="DotNetNuke" %>
<asp:literal id="skinDocType" runat="server" ViewStateMode="Disabled"/>
<html <asp:literal id="attributeList" runat="server" ViewStateMode="Disabled"></asp:literal>>
<head id="Head" runat="server" ViewStateMode="Disabled">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <title />
    <meta id="MetaRefresh" runat="Server" http-equiv="Refresh" name="Refresh" Visible="False" />
    <meta id="MetaDescription" runat="Server" name="DESCRIPTION" Visible="False"/>
    <meta id="MetaKeywords" runat="Server" name="KEYWORDS" Visible="False"/>
    <meta id="MetaCopyright" runat="Server" name="COPYRIGHT" Visible="False"/>
    <meta id="MetaGenerator" runat="Server" name="GENERATOR" Visible="False"/>
    <meta id="MetaAuthor" runat="Server" name="AUTHOR" Visible="False"/>
    <meta id="MetaRobots" runat="server" name="ROBOTS" Visible="False" />
    <asp:PlaceHolder runat="server" ID="metaPanel">
        <meta content="text/html; charset=UTF-8" http-equiv="Content-Type" />
        <meta content="text/javascript" http-equiv="Content-Script-Type" />
        <meta content="text/css" http-equiv="Content-Style-Type" />
        <meta name="RESOURCE-TYPE" content="DOCUMENT" />
        <meta name="REVISIT-AFTER" content="1 DAYS" />
        <meta name="RATING" content="GENERAL" />
    </asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="ClientDependencyHeadCss"></asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="ClientDependencyHeadJs"></asp:PlaceHolder>
    <asp:placeholder id="CSS" runat="server" />
    <asp:placeholder id="SCRIPTS" runat="server" />
</head>
<body id="Body" runat="server">
    
    <dnn:Form ID="Form" runat="server" ENCTYPE="multipart/form-data">
        <asp:PlaceHolder ID="BodySCRIPTS" runat="server" />
        <asp:Label ID="SkinError" runat="server" CssClass="NormalRed" Visible="False"></asp:Label>
        <asp:PlaceHolder ID="SkinPlaceHolder" runat="server" />
        <input id="ScrollTop" runat="server" name="ScrollTop" type="hidden" />
        <input id="__dnnVariable" runat="server" name="__dnnVariable" type="hidden" autocomplete="off" />
        <asp:placeholder runat="server" ID="ClientResourcesFormBottom" />
    </dnn:Form>
    <asp:placeholder runat="server" id="ClientResourceIncludes" />
    <dnncrm:ClientResourceLoader runat="server" id="ClientResourceLoader">
        <Paths>
            <dnncrm:ClientResourcePath Name="SkinPath" Path="<%# CurrentSkinPath %>" />
            <dnncrm:ClientResourcePath Name="SharedScripts" Path="~/Resources/Shared/Scripts/" />
        </Paths>
    </dnncrm:ClientResourceLoader>
</body>
</html>