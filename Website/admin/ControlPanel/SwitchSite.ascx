<%@ Control language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.ControlPanel.SwitchSite" CodeFile="SwitchSite.ascx.cs" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%--<asp:DropDownList ID="SitesLst" runat="server" MaxHeight="300px" Width="200px" />--%>
<dnn:DnnComboBox ID="SitesLst" runat="server" />
<asp:LinkButton ID="cmdSwitch" runat="server" ResourceKey="SwitchButton" CausesValidation="false" CssClass="dnnSecondaryAction selectAction" />