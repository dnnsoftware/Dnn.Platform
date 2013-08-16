<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.UserControls.HelpButtonControl" targetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<asp:linkbutton id=cmdHelp tabindex="-1" runat="server" CausesValidation="False" enableviewstate="False">
  <dnn:DnnImage id="imgHelp"  runat="server" IconKey="Help" enableviewstate="False" />
</asp:linkbutton>
<asp:panel id=pnlHelp runat="server" CssClass="dnnFormMessge dnnFormInfo" enableviewstate="False">
  <asp:label id=lblHelp runat="server" enableviewstate="False" />
</asp:panel>