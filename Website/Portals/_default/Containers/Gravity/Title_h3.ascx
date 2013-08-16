<%@ Control AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Containers.Container" %>
<%@ Register TagPrefix="dnn" TagName="TITLE" Src="~/Admin/Containers/Title.ascx" %>
<%@ Register TagPrefix="dnn" TagName="VISIBILITY" Src="~/Admin/Containers/Visibility.ascx" %>
<div class="DNNContainer_Title_h3 SpacingBottom">
    <h3><dnn:TITLE runat="server" id="dnnTITLE" CssClass="TitleH3" /></h3>
    <div id="ContentPane" runat="server"></div>
	<div class="clear"></div>
</div>
