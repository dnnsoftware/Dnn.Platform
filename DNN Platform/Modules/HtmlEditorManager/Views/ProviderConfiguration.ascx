<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProviderConfiguration.ascx.cs" Inherits="DotNetNuke.Modules.HtmlEditorManager.Views.ProviderConfiguration" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>

<div class="html-editor-manager">
    <h2>HTML Editor Manager</h2>
    <div class="current-provider">
        <h4>Current Provider:</h4>
        <span><%#this.Model.SelectedEditor %></span>
    </div>
    <div class="change-provider">
        <asp:DropDownList ID="ProvidersDropDownList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ProvidersDropDownList_SelectedIndexChanged" DataSource="<%#this.Model.Editors %>" SelectedValue="<%#this.Model.SelectedEditor %>" />
        <asp:Button ID="SaveButton" runat="server" class="dnnPrimaryAction" OnClick="SaveButton_Click" Text="Change" Enabled="<%#this.Model.CanSave %>" />
    </div>
    <div>
        <asp:PlaceHolder runat="server" ID="EditorPanel"></asp:PlaceHolder>
    </div>
</div>
