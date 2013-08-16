<%@ Control language="vb" CodeBehind="~/admin/Containers/container.vb" AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Containers.Container" %>
<%@ Register TagPrefix="dnn" TagName="Title" Src="~/Admin/Containers/Title.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Toggle" Src="~/Admin/Containers/Toggle.ascx" %>
<div class="dnnPEMToggledCont dnnClear">
	<dnn:Toggle ID="ToggleTitle" class="ToggledContentContTitle" Target="ContentPane" runat="server">
		<dnn:Title runat="server" />
	</dnn:Toggle>
    <div id="ContentPane" class="ToggledContentContBody" runat="server"></div>
</div>