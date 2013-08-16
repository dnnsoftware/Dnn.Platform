<%@ Control Language="C#" CodeBehind="~/DesktopModules/Skins/skin.cs" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Skin" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Register TagPrefix="dnn" TagName="JQUERY" Src="~/Admin/Skins/jQuery.ascx" %>
<dnn:JQUERY ID="dnnjQuery" runat="server" />
<dnn:DnnJsInclude ID="DnnJsInclude1" runat="server" FilePath="~/Resources/Shared/Scripts/slides.min.jquery.js" />
<script>
    $(function () {
        $('#slides').slides({
            preload: true,
            preloadImage: '<%= Page.ResolveClientUrl("~/images/loading.gif") %>',
            play: 0,
            next: 'next',
            prev: 'prev',
            pagination: true,
            generatePagination: false,
            hoverPause: true     
        });
    });
</script>
<div id="ContentPane" runat="server" />
