<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.BatchInstall" CodeFile="BatchInstall.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>

<div style="text-align:left">
    <asp:Label ID="lblHelp" runat="server" cssClass="Normal" resourceKey="Help" />
    <br /><br />
    <asp:Label ID="lblModules" CssClass="Head" runat="server" resourcekey="AvailableModules" />
    <br /><br />
    <asp:CheckBoxList ID="lstModules" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" />
    <asp:Label ID="lblNoModules" runat="server" />
    <hr />
    <asp:Label ID="lblModulesError" runat="server" />
    <br /><br />
    <asp:Label ID="lblSkins" CssClass="Head" runat="server" resourcekey="AvailableSkins"  />
    <br /><br />
    <asp:CheckBoxList ID="lstSkins" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" />
    <asp:Label ID="lblNoSkins" runat="server" />
    <br />
    <asp:Label ID="lblContainers" CssClass="Head" runat="server" resourcekey="AvailableContainers"  />
    <br /><br />
    <asp:CheckBoxList ID="lstContainers" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" />
    <asp:Label ID="lblNoContainers" runat="server" />
    <hr />
    <asp:Label ID="lblSkinsError" runat="server" />
    <br /><br />
    <asp:Label ID="lblLanguages" CssClass="Head" runat="server" resourcekey="AvailableLanguages" />
    <br /><br />
    <asp:CheckBoxList ID="lstLanguages" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" />
    <asp:Label ID="lblNoLanguages" runat="server" />
    <hr />
    <asp:Label ID="lblLanguagesError" runat="server" />
    <br /><br />
    <asp:Label ID="lblAuthSystems" CssClass="Head" runat="server" resourcekey="AvailableAuthSystems" />
    <br /><br />
    <asp:CheckBoxList ID="lstAuthSystems" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" />
    <asp:Label ID="lblNoAuthSystems" runat="server" />
    <hr />
    <asp:Label ID="lblAuthSystemsError" runat="server" />
</div>
<p>
	<dnn:commandbutton id="cmdInstall" runat="server" CssClass="CommandButton" IconKey="Save" ResourceKey="cmdInstall" />
</p>
