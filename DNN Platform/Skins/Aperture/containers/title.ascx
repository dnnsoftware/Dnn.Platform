<%@ Control AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Containers.Container" %>
<%@ Register TagPrefix="dnn" TagName="TITLE" Src="~/Admin/Containers/Title.ascx" %>
<div class="aperture-container aperture-title-wrapper">
    <h5><dnn:TITLE runat="server" id="apertureTitle" /></h5>
    <div id="ContentPane" runat="server"></div>
</div>
