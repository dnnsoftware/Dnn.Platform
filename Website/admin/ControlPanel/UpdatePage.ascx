<%@ Control language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.ControlPanel.UpdatePage" CodeFile="UpdatePage.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnFormItem">
    <div class="dnnClear">
        <asp:Label runat="server" ResourceKey="Name" AssociatedControlID="Name" />
        <asp:TextBox ID="Name" runat="server" MaxLength="200" />
    </div>
    <div class="dnnClear">
        <asp:Label runat="server" ResourceKey="Location" AssociatedControlID="LocationLst" />
        <%--<asp:DropDownList ID="LocationLst" runat="server" />--%>
        <dnn:DnnComboBox ID="LocationLst" runat="server" />
        <%--<asp:DropDownList ID="PageLst" runat="server" MaxHeight="300px" CssClass="dnnCPPageList" />--%>
        <dnn:DnnComboBox ID="PageLst" runat="server" CssClass="dnnCPPageList" />
    </div>
    <div class="dnnClear">
        <asp:Label id="SkinLbl" runat="server" ResourceKey="Skin" AssociatedControlID="SkinLst" />
        <dnn:DnnComboBox ID="SkinLst" runat="server" MaxHeight="300px" />
    </div>
    <div class="dnnFormCheckbox dnnClear">
        <asp:CheckBox ID="IncludeInMenu" runat="server" Checked="true" />
        <asp:Label id="IncludeInMenuLbl" runat="server" ResourceKey="IncludeInMenu" AssociatedControlID="IncludeInMenu" />
    </div>
    <div class="dnnFormCheckbox dnnClear">
        <asp:CheckBox ID="IsDisabled" runat="server" Checked="false" />
        <asp:Label id="DisabledLbl" runat="server" ResourceKey="Disabled" AssociatedControlID="IsDisabled" />
    </div>
    <asp:Panel ID="IsSecurePanel" runat="server" CssClass="dnnClear">
        <dnn:DnnFieldLabel id="IsSecureLbl" runat="server" Text="Secured" AssociatedControlID="IsSecure" />
        <asp:CheckBox ID="IsSecure" runat="server" Checked="false" CssClass="dnnFormCheckbox" />
    </asp:Panel>
</div>
<asp:LinkButton ID="cmdUpdate" runat="server" ResourceKey="UpdateButton" CssClass="dnnPrimaryAction" ValidationGroup="ControlPanel" />