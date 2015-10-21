<%@ Control AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Containers.Container" %>
<%@ Register TagPrefix="dnn" TagName="TITLE" Src="~/Admin/Containers/Title.ascx" %>
<div class="DNNContainer_Title_h3 SpacingBottom">
    <div class="heading_wrapper">
        <h3><dnn:TITLE runat="server" id="dnnTITLE" CssClass="TitleH3" /></h3>
        <div id="ContentPane" runat="server"></div>
	    <div class="clear"></div>
    </div>
</div>
