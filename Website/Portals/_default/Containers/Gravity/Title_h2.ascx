<%@ Control AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Containers.Container" %>
<%@ Register TagPrefix="dnn" TagName="TITLE" Src="~/Admin/Containers/Title.ascx" %>
<style>
    .DNNContainer_Title_h2 h2 .TitleH2 {
	    display: block;
	    margin-bottom: 25px;
    }
</style>

<div class="DNNContainer_Title_h2 SpacingBottom">
    <h2><dnn:TITLE runat="server" id="dnnTITLE" CssClass="TitleH2" /></h2>
    <div id="ContentPane" runat="server"></div>
	<div class="clear"></div>
</div>
