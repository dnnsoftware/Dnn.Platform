<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.UserControls.LabelControl" %>
<div class="dnnLabel">    
    <label>
        <asp:Label ID="lblLabel" runat="server" EnableViewState="False"/>   
    </label>
    <asp:LinkButton ID="cmdHelp" TabIndex="-1" runat="server" CausesValidation="False"
        EnableViewState="False" CssClass="dnnFormHelp">        
    </asp:LinkButton>
    <asp:Panel ID="pnlHelp" runat="server" CssClass="dnnTooltip">
        <div class="dnnFormHelpContent dnnClear">
            <asp:Label ID="lblHelp" runat="server" EnableViewState="False" class="dnnHelpText" />
            <a href="#" class="pinHelp"></a>
       </div>   
    </asp:Panel>
</div>

