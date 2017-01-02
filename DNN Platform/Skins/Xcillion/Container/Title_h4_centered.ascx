<%@ Control AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Containers.Container" %>
<%@ Register TagPrefix="dnn" TagName="TITLE" Src="~/Admin/Containers/Title.ascx" %>
<div class="DNNContainer_Title_h4 SpacingBottom">
    <div class="heading_wrapper">
        <h4><dnn:TITLE runat="server" id="dnnTITLE" CssClass="TitleH4 centered" /></h4>
        <div id="ContentPane" runat="server"></div>
	    <div class="clear"></div>
    </div>
</div>
