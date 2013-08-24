<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MyFiles.ascx.cs" Inherits="DotNetNuke.Modules.Journal.MyFiles" %>
<table>
    <tr>
        <td></td>
        <td>
        <asp:FileUpload ID="fileUp" runat="server" />
        <asp:Button ID="btnUp" runat="server" Text="Upload" />
        </td>
    </tr>
</table>
<asp:Literal ID="litOut" runat="server" />