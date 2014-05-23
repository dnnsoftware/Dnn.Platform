<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Web.UI.WebControls.DnnUrlControl" %>
<%@ Register TagPrefix="dnn" TagName="FilePickerUploader" Src="filepickeruploader.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="urlControl">
    <asp:Panel ID="TypeRow" runat="server" CssClass="urlControlLinkType dnnClear">
        <asp:Label ID="lblURLType" runat="server" EnableViewState="False" resourcekey="Type" CssClass="dnnFormLabel" />
        <asp:RadioButtonList ID="optType" AutoPostBack="True" runat="server" RepeatDirection="Vertical" CssClass="ucLinkTypeRadioButtons"  />
    </asp:Panel>
    <asp:Panel ID="URLRow" runat="server" CssClass="urlControlFile dnnClear">
        <asp:Label ID="lblURL" runat="server" EnableViewState="False" resourcekey="URL" />
        <div>
            <asp:DropDownList ID="cboUrls" runat="server" DataTextField="Url" DataValueField="Url" />
            <asp:TextBox ID="txtUrl" runat="server" />
            <asp:LinkButton ID="cmdSelect" resourcekey="Select" CssClass="dnnSecondaryAction" runat="server" CausesValidation="False" />
            <asp:LinkButton ID="cmdDelete" resourcekey="Delete" CssClass="dnnSecondaryAction" runat="server" CausesValidation="False" />
            <asp:LinkButton ID="cmdAdd" resourcekey="Add" CssClass="dnnSecondaryAction" runat="server" CausesValidation="False" />
	    </div>
    </asp:Panel>
    <asp:Panel ID="TabRow" runat="server" CssClass="urlControlTab dnnClear" >
        <div class="dnnFormItem">
            <asp:Label ID="lblTab" runat="server" EnableViewState="False" resourcekey="Tab" />
            <dnn:DnnPageDropDownList ID="cboTabs" runat="server"  />
        </div>
    </asp:Panel>
    <asp:Panel id="FileRow" runat="server" CssClass="urlControlFileRow dnnClear">
        <dnn:FilePickerUploader ID="ctlFile" runat="server" />
    </asp:Panel>
    <asp:Panel id="ImagesRow" runat="server" CssClass="dnnFormItem urlControlImagesRow dnnClear">
        <asp:Label ID="lblImages" runat="server" EnableViewState="False" resourcekey="Image" CssClass="dnnFormLabel" />
        <asp:DropDownList ID="cboImages" runat="server" />
    </asp:Panel>
    <asp:Panel id="UserRow" runat="server" CssClass="urlControlUseRow dnnClear">
        <asp:Label ID="lblUser" runat="server" EnableViewState="False" resourcekey="User" />
        <asp:TextBox ID="txtUser" runat="server" />U
    </asp:Panel>
    <asp:Panel id="ErrorRow" runat="server" CssClass="dnnFormMessage" Visible="false">
        <asp:Label ID="lblMessage" runat="server" EnableViewState="False" CssClass="dnnFormError" />
    </asp:Panel>
    <div class="dnnClear">
        <asp:CheckBox ID="chkTrack" resourcekey="Track" runat="server" Text="Track?" TextAlign="Right" /><br/>
        <asp:CheckBox ID="chkLog" resourcekey="Log" runat="server" Text="Log?" TextAlign="Right" /><br/>
        <asp:CheckBox ID="chkNewWindow" resourcekey="NewWindow" runat="server" Text="New Window?" TextAlign="Right" Visible="False" /><br/>
    </div>
</div>
