<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Modules.ModulePermissions" CodeFile="ModulePermissions.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Security.Permissions.Controls" Assembly="DotNetNuke" %>
<div class="dnnForm dnnModuleSettings dnnClear" id="dnnModulePermissions">
    <div class="msPermissions dnnClear" id="msPermissions">
        <div class="mspContent dnnClear">
            <h2 id="dnnPanel-ModuleAdditionalPages" class="dnnFormSectionHead"><%=LocalizeString("Permissions")%></h2>
            <fieldset>
                <div id="permissionsRow" runat="server">
                    <dnn:modulepermissionsgrid id="dgPermissions" runat="server" />
                </div>
            </fieldset>
        </div>
    </div>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton ID="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
        <li><asp:HyperLink ID="cancelHyperLink" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
    </ul>
</div>
