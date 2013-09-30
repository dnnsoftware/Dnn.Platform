<%@ Control Language="C#" AutoEventWireup="true" CodeFile="JavaScriptLibraryEditor.ascx.cs" Inherits="DotNetNuke.Modules.Admin.Extensions.JavaScriptLibraryEditor" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<h2 class="dnnFormSectionHead"><a href="" class="dnnLabelExpanded"><%=LocalizeString("Title")%></a></h2>
<fieldset>
    <div class="dnnFormItem">
        <dnn:Label ID="LibraryNameLabel" runat="server" ControlName="LibraryName" />
        <asp:Label runat="server" ID="LibraryName" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="VersionLabel" runat="server" ControlName="Version" />
        <asp:Label ID="Version" runat="server"></asp:Label>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="FileNameLabel" runat="server" ControlName="FileName" />
        <asp:Label ID="FileName" runat="server"></asp:Label>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="ObjectNameLabel" runat="server" ControlName="ObjectName" />
        <asp:Label ID="ObjectName" runat="server"></asp:Label>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="LocationLabel" runat="server" ControlName="Location" />
        <asp:Label ID="Location" runat="server"></asp:Label>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="CDNLabel" runat="server" ControlName="CDN" />
        <asp:Label ID="CDN" runat="server"></asp:Label>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="CustomCDNLabel" runat="server" ControlName="CustomCDN" />
        <asp:TextBox ID="CustomCDN" runat="server"></asp:TextBox>
    </div>
</fieldset>
