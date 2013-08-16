<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.LanguagePackEditor" CodeFile="LanguagePackEditor.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<h2 class="dnnFormSectionHead"><a href="" class="dnnLabelExpanded"><%=LocalizeString("Title")%></a></h2>
<fieldset>
    <div class="dnnFormItem">
        <dnn:Label ID="plPackageLanguage" runat="server" ControlName="cboLanguage" />
        <%--<asp:DropDownList ID="cboLanguage" runat="server" DataTextField="Text" DataValueField="LanguageID"/>--%>
        <dnn:DnnComboBox ID="cboLanguage" runat="server" DataTextField="Text" DataValueField="LanguageID"/>
    </div>
    <div id="packageRow" runat="server" class="dnnFormItem">
        <dnn:Label ID="plPackage" runat="server" ControlName="cboPackage" />
        <%--<asp:DropDownList ID="cboPackage" runat="server" DataTextField="FriendlyName" DataValueField="PackageID"/>--%>
        <dnn:DnnComboBox ID="cboPackage" runat="server" DataTextField="FriendlyName" DataValueField="PackageID"/>
    </div>
    <ul class="dnnActions dnnClear">
    	<li><asp:LinkButton id="cmdEdit" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdEdit" /></li>
    </ul>
</fieldset>