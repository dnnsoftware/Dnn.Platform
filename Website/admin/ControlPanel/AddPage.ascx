<%@ Control language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.ControlPanel.AddPage" CodeFile="AddPage.ascx.cs" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="cbcpPageSettingsContent dnnFormItem">
    <div class="dnnClear">
        <asp:Label id="NameLbl" runat="server" Text="Name" AssociatedControlID="Name" ResourceKey="Name" />
        <asp:TextBox ID="Name" runat="server" MaxLength="200" />
    </div>
    <div class="dnnClear">
        <asp:Label runat="server" ResourceKey="Template" AssociatedControlID="TemplateLst" />
        <%--<asp:DropDownList ID="TemplateLst" runat="server" />--%>
        <dnn:DnnComboBox ID="TemplateLst" runat="server" />
    </div>
    <div class="dnnClear">
        <asp:Label runat="server" ResourceKey="Location" AssociatedControlID="LocationLst" />
        <%--<asp:DropDownList ID="LocationLst" runat="server" />--%>
        <dnn:DnnComboBox ID="LocationLst" runat="server" />
       <%-- <asp:DropDownList ID="PageLst" runat="server" MaxHeight="300px" CssClass="dnnCPPageList" />--%>
       <dnn:DnnComboBox ID="PageLst" runat="server" CssClass="dnnCPPageList" />
    </div>
    <div class="dnnFormCheckbox dnnClear">
        <asp:CheckBox ID="IncludeInMenu" runat="server" Checked="true" />
		<asp:Label runat="server" ResourceKey="IncludeInMenu" AssociatedControlID="IncludeInMenu" />
    </div>
</div>
<asp:LinkButton ID="cmdAddPage" runat="server" ResourceKey="AddButton" CssClass="dnnPrimaryAction" ValidationGroup="ControlPanel" />