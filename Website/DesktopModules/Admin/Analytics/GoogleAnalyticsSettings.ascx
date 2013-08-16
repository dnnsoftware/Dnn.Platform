<%@ Control Language="C#" Inherits="DotNetNuke.Modules.Admin.Analytics.GoogleAnalyticsSettings" AutoEventWireup="false" CodeFile="GoogleAnalyticsSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnAnalytics dnnClear" id="dnnAnalytics">
    <fieldset>
        <div class="dnnFormItem">
            <dnn:label id="lblTrackingId" runat="server" controlname="txtTrackingId" />
            <asp:textbox id="txtTrackingId" runat="server" Width="280px" />
            <asp:RequiredFieldValidator ID="valTrackingId" runat="server" CssClass="dnnFormMessage dnnFormError" ControlToValidate="txtTrackingId" Display="Dynamic" resourcekey="valTrackingId" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblUrlParameter" runat="server" controlname="txtUrlParameter" suffix=":" />
            <asp:textbox id="txtUrlParameter" runat="server" textmode="multiline" rows="6" Width="280px" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
    </ul>  
</div>