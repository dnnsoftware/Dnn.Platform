<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Users.Membership" CodeFile="Membership.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<dnn:DnnFormEditor id="membershipForm" runat="Server" FormMode="Short">
    <Items>
        <dnn:DnnFormLiteralItem ID="createdDate" runat="server" DataField="CreatedDate" />
        <dnn:DnnFormLiteralItem ID="lastLoginDate" runat="server" DataField="LastLoginDate" />
        <dnn:DnnFormLiteralItem ID="lastActivityDate" runat="server" DataField="LastActivityDate" />
        <dnn:DnnFormLiteralItem ID="lastPasswordChangeDate" runat="server" DataField="LastPasswordChangeDate" />
        <dnn:DnnFormLiteralItem ID="lastLockoutDate" runat="server" ResourceKey="LastLockoutDate" />
        <dnn:DnnFormLiteralItem ID="isOnLine" runat="server" ResourceKey="IsOnLine" />
        <dnn:DnnFormLiteralItem ID="lockedOut" runat="server" ResourceKey="LockedOut" />
        <dnn:DnnFormLiteralItem ID="approved" runat="server" ResourceKey="Approved" />
        <dnn:DnnFormLiteralItem ID="updatePassword" runat="server" ResourceKey="UpdatePassword" />
        <dnn:DnnFormLiteralItem ID="isDeleted" runat="server" ResourceKey="IsDeleted" />
   </Items>
</dnn:DnnFormEditor>
<ul id="actionsRow" runat="server" class="dnnActions dnnClear">
    <li><asp:LinkButton id="cmdAuthorize" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdAuthorize" CausesValidation="False" /></li>
    <li><asp:LinkButton id="cmdUnAuthorize" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdUnAuthorize" CausesValidation="False" /></li>
    <li><asp:LinkButton id="cmdUnLock" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdUnLock" CausesValidation="False" /></li>
    <li><asp:LinkButton id="cmdPassword" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdPassword" CausesValidation="False" /></li>
    <li><asp:LinkButton id="cmdToggleSuperuser" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" Visible="False" /></li>
</ul>
