<%@ Control AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Containers.Container" %>
<%@ Register TagPrefix="dnn" TagName="TITLE" Src="~/Admin/Containers/Title.ascx" %>
<style>
    .DNNContainer_Title_h3 h3 .TitleH3 {
	    display: block;
	    padding-bottom: 10px;
	    margin-bottom: 25px;
	    border-bottom: solid 1px #c0c0c0;
    }
</style>

<div class="DNNContainer_Title_h3 SpacingBottom">
    <h3><dnn:TITLE runat="server" id="dnnTITLE" CssClass="TitleH3" /></h3>
    <div id="ContentPane" runat="server"></div>
	<div class="clear"></div>
</div>
