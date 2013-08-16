<%@ Control Language="C#" Inherits="DotNetNuke.Modules.Admin.Extensions.Download" AutoEventWireup="false" CodeFile="Download.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<div class="dnnForm dnnDeployExtension dnnClear" id="dnnDeployExtension">
    <h2 class="dnnFormSectionHead"><asp:Label ID="lblTitle" runat="server"><% =GetString("Title")%></asp:Label></h2>
    <div class="dnnFormMessage dnnFormInfo"><asp:Label ID="lblHelp" runat="server"><% =GetString("Help")%></asp:Label></div>
     <fieldset>
        <div class="dnnFormItem">
            <dnn:Label ID="extensionTypeLabel" runat="server" ControlName="extensionType" />
            <asp:Label ID="extensionType" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="extensionNamelabel" runat="server" ControlName="extensionName" />
            <asp:Label ID="extensionName" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="extensionDescLabel" runat="server" ControlName="extensionDesc" />
            <asp:Label ID="extensionDesc" runat="server" />
        </div>
    </fieldset>

    <ul class="dnnActions dnnClear">
    	<li><asp:Hyperlink id="installExtension" runat="server" CssClass="dnnPrimaryAction" resourcekey="install" Visible="false" /></li>
        <li><asp:LinkButton id="deployExtension" runat="server" CssClass="dnnPrimaryAction" resourcekey="deploy" Causesvalidation="False" /></li>
        <li><asp:LinkButton id="downloadExtension" runat="server" CssClass="dnnSecondaryAction" resourcekey="download" Causesvalidation="False" /></li>
        <li><asp:LinkButton id="cancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" Causesvalidation="False" /></li>
    </ul>
</div>