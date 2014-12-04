<%@ Control Language="c#" AutoEventWireup="false" Inherits="WatchersNET.CKEditor.Controls.UrlControl" %>

<table>
    <tr id="TypeRow" runat="server">
            <td style="white-space:nowrap">
                <br/>
                <asp:Label ID="URLTypeLabel" runat="server" EnableViewState="False" resourcekey="Type" CssClass="NormalBold" />
                <br/>
                <asp:RadioButtonList ID="Type" CssClass="NormalBold" AutoPostBack="True" RepeatDirection="Vertical" runat="server" />
                <br/>
            </td>
        </tr>
        <tr id="URLRow" runat="server">
            <td style="white-space:nowrap">
                <asp:Label ID="URLLabel" runat="server" EnableViewState="False" resourcekey="URL" CssClass="NormalBold" />
                <asp:DropDownList ID="Urls" runat="server" DataTextField="Url" DataValueField="Url" CssClass="NormalTextBox" Width="300" />
                <asp:TextBox ID="UrlTextBox" runat="server" CssClass="NormalTextBox" Width="300" />
                <br/>
                <asp:LinkButton ID="Select" resourcekey="Select" CssClass="CommandButton" runat="server" CausesValidation="False" />
                <asp:LinkButton ID="Delete" resourcekey="Delete" CssClass="CommandButton" runat="server" CausesValidation="False" />
                <asp:LinkButton ID="Add" resourcekey="Add" CssClass="CommandButton" runat="server" CausesValidation="False" />
            </td>
        </tr>
        <tr id="FileRow" runat="server">
            <td style="white-space:nowrap">
                <asp:Label ID="FolderLabel" runat="server" EnableViewState="False" resourcekey="Folder" CssClass="NormalBold" />
                <asp:DropDownList ID="Folders" runat="server" AutoPostBack="True" CssClass="NormalTextBox" Width="300" />
                <asp:Image ID="StorageLocationTypeImage" runat="server" Visible="False" />
                <br />
                <asp:Label ID="FileLabel" runat="server" EnableViewState="False" resourcekey="File" CssClass="NormalBold" />
                <asp:DropDownList ID="Files" runat="server" DataTextField="Text" DataValueField="Value" CssClass="NormalTextBox" Width="300" />
                <input id="File" type="file" size="30" name="File" runat="server" width="300" />
                <br />
                <asp:LinkButton ID="Upload" resourcekey="Upload" CssClass="CommandButton" runat="server" CausesValidation="False" />
                <asp:LinkButton ID="Save" resourcekey="Save" CssClass="CommandButton" runat="server" CausesValidation="False" />
                <asp:LinkButton ID="Cancel" resourcekey="Cancel" CssClass="CommandButton" runat="server" CausesValidation="False" />
            </td>
        </tr>
        <tr id="ErrorRow" runat="server">
            <td>
                <asp:Label ID="MessageLabel" runat="server" EnableViewState="False" CssClass="NormalRed" />
                <br />
            </td>
        </tr>
</table>