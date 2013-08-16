<%@ Control Language="C#" CodeBehind="~/admin/Containers/container.cs" AutoEventWireup="false" Inherits="DotNetNuke.UI.Containers.Container" %>
<%@ Register TagPrefix="dnn" TagName="ACTIONBUTTON" Src="~/Admin/Containers/ActionButton.ascx" %>
<div id="ContentPane" runat="server" />
<div class="c_footer">
    <dnn:ACTIONBUTTON runat="server" id="dnnACTIONBUTTON1"  CommandName="AddContent.Action" DisplayIcon="True" DisplayLink="True" />
    <dnn:ACTIONBUTTON runat="server" id="dnnACTIONBUTTON2"  CommandName="SyndicateModule.Action" DisplayIcon="True" DisplayLink="false" />
    <dnn:ACTIONBUTTON runat="server" id="dnnACTIONBUTTON4"  CommandName="ModuleSettings.Action" DisplayIcon="True" DisplayLink="false" />
</div>