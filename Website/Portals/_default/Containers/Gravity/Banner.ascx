<%@ Control AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Containers.Container" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Register TagPrefix="dnn" TagName="JQUERY" Src="~/Admin/Skins/jQuery.ascx" %>
<dnn:JQUERY ID="dnnjQuery" runat="server" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/Scripts/slides.min.jquery.js" />
<script>
    $(function () {
        $('#slides').slides({
            preload: true,
            preloadImage: '<%= Page.ResolveClientUrl("~/images/loading.gif") %>',
            play: 5000,
            pause: 2500,
            hoverPause: true
        });
    });
</script>
<div class="DNNContainer_without_title">
	<div id="ContentPane" runat="server"></div>
</div>