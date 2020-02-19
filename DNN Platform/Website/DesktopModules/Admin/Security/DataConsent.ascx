<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="DataConsent.ascx.cs" Inherits="DotNetNuke.Modules.Admin.Users.DataConsent" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<div class="dnnForm dnnPasswordReset dnnClear">
 <div>
  <asp:CheckBox runat="server" ID="chkAgree" />
  <%= String.Format(DotNetNuke.Services.Localization.Localization.GetString("DataConsent", LocalResourceFile),
    PortalSettings.Current.TermsTabId == Null.NullInteger ? DotNetNuke.Common.Globals.NavigateURL(PortalSettings.Current.ActiveTab.TabID, "Terms") : DotNetNuke.Common.Globals.NavigateURL(PortalSettings.Current.TermsTabId),
    PortalSettings.Current.PrivacyTabId == Null.NullInteger ? DotNetNuke.Common.Globals.NavigateURL(PortalSettings.Current.ActiveTab.TabID, "Privacy") : DotNetNuke.Common.Globals.NavigateURL(PortalSettings.Current.PrivacyTabId)) %>
 </div>
 <asp:Panel runat="server" ID="pnlNoAgreement">
 </asp:Panel>
 <ul class="dnnActions dnnClear">
  <li>
   <asp:Button ID="cmdSubmit" CssClass="dnnPrimaryAction" runat="server" resourcekey="cmdSubmit" /></li>
  <li>
   <asp:Button ID="cmdCancel" CssClass="dnnSecondaryAction" runat="server" resourcekey="cmdCancel" /></li>
  <li>
   <asp:Button ID="cmdDeleteMe" CssClass="dnnSecondaryAction" runat="server" resourcekey="cmdDeleteMe" /></li>
 </ul>
</div>
