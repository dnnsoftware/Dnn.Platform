<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.UserControls.Help" %>
<asp:Label ID="lblHelp" runat="server" Width="100%" EnableViewState="False" />
<div class="dnnForm dnnHelpControl">
    <fieldset>
        <div class="dnnFormItem dnnClear">
            <asp:Literal ID="helpFrame" runat="server"></asp:Literal>
        </div>
        <ul class="dnnActions dnnClear">
            <li>
                <asp:HyperLink ID="cmdHelp" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdHelp" Target="_new" EnableViewState="False" /></li>
            <li>
                <asp:LinkButton ID="cmdCancel" runat="server" class="dnnSecondaryAction" resourcekey="cmdCancel" CausesValidation="False" EnableViewState="False" /></li>
        </ul>
        <div class="dnnFormItem">
            <asp:Label ID="lblInfo" runat="server" Width="100%" EnableViewState="False" /></div>
    </fieldset>
</div>
