<%@ Control Language="C#" Inherits="DotNetNuke.Modules.Admin.Extensions.Store" AutoEventWireup="false" CodeFile="Store.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnStore dnnClear">
    <div class="dnnFormItem">
        <dnn:label id="plUsername" AssociatedControlID="txtUsername" runat="server" resourcekey="Username" CssClass="dnnFormLabel" />
        <asp:textbox id="txtUsername" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plPassword" AssociatedControlID="txtPassword" runat="server" resourcekey="Password" CssClass="dnnFormLabel" />
        <asp:textbox id="txtPassword" textmode="Password" runat="server" />
    </div>
     <ul class="dnnActions dnnClear">
    	<li><asp:LinkButton id="cmdSave" resourcekey="cmdSave" cssclass="dnnPrimaryAction" text="Save" runat="server" /></li>
        <li><asp:LinkButton id="cmdCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" Causesvalidation="False" /></li>
    </ul>
</div>