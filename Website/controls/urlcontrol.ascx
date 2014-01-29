<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.UserControls.UrlControl" %>
<div class="urlControl">
    <asp:Panel ID="TypeRow" runat="server" CssClass="urlControlLinkType dnnClear">
        <asp:Label ID="lblURLType" runat="server" EnableViewState="False" resourcekey="Type" CssClass="dnnFormLabel" />
        <asp:RadioButtonList ID="optType" AutoPostBack="True" runat="server" RepeatDirection="Vertical" CssClass="ucLinkTypeRadioButtons" />
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
    <asp:Panel ID="TabRow" runat="server" CssClass="urlControlTab dnnClear">
        <asp:Label ID="lblTab" runat="server" EnableViewState="False" resourcekey="Tab" />
        <asp:DropDownList ID="cboTabs" runat="server" DataTextField="IndentedTabName" DataValueField="TabId" />
    </asp:Panel>
    <asp:Panel id="FileRow" runat="server" CssClass="urlControlFileRow dnnClear">
        <div class="dnnFormItem">
            <asp:Label ID="lblFolder" runat="server" EnableViewState="False" resourcekey="Folder" CssClass="dnnFormLabel" />
            <div>
                <asp:DropDownList ID="cboFolders" runat="server" AutoPostBack="True" />
                <asp:Image ID="imgStorageLocationType" runat="server" Visible="False" />
            </div>
        </div>
        <div class="dnnFormItem">    
            <asp:Label ID="lblFile" runat="server" EnableViewState="False" resourcekey="File" CssClass="dnnFormLabel" />
            <div>
                <asp:DropDownList ID="cboFiles" runat="server" DataTextField="Text" DataValueField="Value" />
                <input id="txtFile" type="file" size="30" name="txtFile" runat="server" />
            </div>
	    </div>
        <div class="dnnFormItem">
        <asp:LinkButton ID="cmdUpload" resourcekey="Upload" CssClass="dnnSecondaryAction" runat="server" CausesValidation="False" />
        <asp:LinkButton ID="cmdSave" resourcekey="Save" CssClass="dnnSecondaryAction" runat="server" CausesValidation="False" />
        <asp:LinkButton ID="cmdCancel" resourcekey="Cancel" CssClass="dnnSecondaryAction" runat="server" CausesValidation="False" />
        </div>
    </asp:Panel>
    <asp:Panel id="ImagesRow" runat="server" CssClass="dnnFormItem urlControlImagesRow dnnClear">
        <asp:Label ID="lblImages" runat="server" EnableViewState="False" resourcekey="Image" CssClass="dnnFormLabel" />
        <asp:DropDownList ID="cboImages" runat="server" />
    </asp:Panel>
    <asp:Panel id="UserRow" runat="server" CssClass="urlControlUseRow dnnClear">
        <asp:Label ID="lblUser" runat="server" EnableViewState="False" resourcekey="User" />
        <asp:TextBox ID="txtUser" runat="server" />
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
