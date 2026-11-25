<%@ Control Language="C#" AutoEventWireup="false" Inherits="Dnn.RichTextEditorProvider.Web.Settings" %>
<div class="dnnForm dnn-rte-settings">
    <asp:Panel ID="SettingsPanel" runat="server">
        <p><asp:Literal ID="IntroText" runat="server" /></p>
        <fieldset>
            <div class="dnnFormItem">
                <asp:Label ID="ScopeLabel" runat="server" AssociatedControlID="ScopeList" CssClass="dnnLabel" />
                <asp:DropDownList ID="ScopeList" runat="server" AutoPostBack="true" />
            </div>
            <p class="dnnFormHelp"><asp:Literal ID="ScopeHelp" runat="server" /></p>
            <div class="dnnFormItem">
                <asp:Label ID="DefaultToolbarLabel" runat="server" AssociatedControlID="DefaultToolbarList" CssClass="dnnLabel" />
                <asp:DropDownList ID="DefaultToolbarList" runat="server" />
            </div>
            <div class="dnnFormItem">
                <asp:Label ID="UploadFolderLabel" runat="server" AssociatedControlID="UploadFolderText" CssClass="dnnLabel" />
                <asp:TextBox ID="UploadFolderText" runat="server" MaxLength="300" />
            </div>
            <div class="dnnFormItem">
                <asp:Label ID="DefaultUploadLimitLabel" runat="server" AssociatedControlID="DefaultUploadLimitText" CssClass="dnnLabel" />
                <asp:TextBox ID="DefaultUploadLimitText" runat="server" MaxLength="9" />
            </div>
        </fieldset>
        <h3><asp:Literal ID="RolesHeading" runat="server" /></h3>
        <p class="dnnFormHelp"><asp:Literal ID="RolesHelp" runat="server" /></p>
        <table class="dnnGrid dnn-rte-settings__roles">
            <thead>
                <tr class="dnnGridHeader">
                    <th scope="col"><asp:Literal ID="RoleColumn" runat="server" /></th>
                    <th scope="col"><asp:Literal ID="ToolbarColumn" runat="server" /></th>
                    <th scope="col"><asp:Literal ID="UploadLimitColumn" runat="server" /></th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="RoleRows" runat="server">
                    <ItemTemplate>
                        <tr class="dnnGridItem">
                            <td>
                                <asp:HiddenField ID="RoleName" runat="server" Value="<%# Container.DataItem %>" />
                                <%# System.Web.HttpUtility.HtmlEncode((string)Container.DataItem) %>
                            </td>
                            <td><asp:DropDownList ID="RoleToolbar" runat="server" /></td>
                            <td><asp:TextBox ID="RoleUploadLimit" runat="server" MaxLength="9" Columns="8" /></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        <ul class="dnnActions dnnClear">
            <li><asp:LinkButton ID="SaveButton" runat="server" CssClass="dnnPrimaryAction" /></li>
        </ul>
    </asp:Panel>
    <asp:Label ID="MessageLabel" runat="server" EnableViewState="false" />
</div>
