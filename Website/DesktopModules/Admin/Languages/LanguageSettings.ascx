<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Languages.LanguageSettings" CodeFile="LanguageSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<div class="dnnFormItem">
    <dnn:Label ID="plUsePaging" runat="server" ControlName="chkUsePaging" />
    <asp:CheckBox ID="chkUsePaging" runat="server" />
</div>
<div class="dnnFormItem">
    <dnn:Label ID="plPageSize" runat="server" ControlName="txtPageSize" />
    <asp:TextBox ID="txtPageSize" runat="server" />
    <asp:RequiredFieldValidator ID="valPageSize2" runat="server" ControlToValidate="txtPageSize" resourcekey="PageSize.Error" Display="Dynamic" CssClass="dnnFormMessage dnnFormError" />
    <asp:RangeValidator ID="valPageSize" runat="server" ControlToValidate="txtPageSize" resourcekey="PageSize.Error" Type="Integer" Display="Dynamic" MinimumValue="0" CssClass="dnnFormMessage dnnFormError" />
</div>
<div class="dnnFormItem">
    <dnn:Label ID="plShowLanguages" runat="server"  ControlName="chkShowLanguages" />
    <asp:CheckBox runat="server" ID="chkShowLanguages" />
</div>
<div class="dnnFormItem">
    <dnn:Label ID="plShowSettings" runat="server"  ControlName="chkShowSettings" />
    <asp:CheckBox runat="server" ID="chkShowSettings" />
</div>