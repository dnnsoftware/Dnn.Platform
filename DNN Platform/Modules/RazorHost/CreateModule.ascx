<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="CreateModule.ascx.cs" Inherits="DotNetNuke.Modules.RazorHost.CreateModule" %>
<%@ Register Assembly="DotnetNuke" Namespace="DotNetNuke.UI.WebControls" TagPrefix="dnn" %>
<%@ Register Assembly="DotnetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" TagPrefix="dnn" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnRazorHostCreateModule dnnClear" id="dnnRazorHost">
    <fieldset>
        <div class="dnnFormItem">
            <dnn:Label id="scriptsLabel" runat="Server" controlname="scriptList" />
            <asp:DropDownList ID="scriptList" runat="server" AutoPostBack="true" />
        </div>
		<div class="dnnFormItem razorHostModSource"><asp:Label ID="lblSourceFile" runat="server" /></div>
        <div class="dnnFormItem razorHostModSource"><asp:Label ID="lblModuleControl" runat="server" /></div>
        <div class="dnnFormItem">
            <dnn:label id="plFolder" controlname="txtFolder" runat="server" />
            <asp:TextBox ID="txtFolder" runat="server" />
            <asp:RequiredFieldValidator ID="valFolder" runat="server" resourceKey="valFolder" ControlToValidate="txtFolder" 
                CssClass="dnnFormError" EnableClientScript="true" Display="Dynamic" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plName" controlname="txtName" runat="server" />
            <asp:TextBox ID="txtName" runat="server" />
            <asp:RequiredFieldValidator ID="valName" runat="server" resourceKey="valName" ControlToValidate="txtName" 
            CssClass="dnnFormError" EnableClientScript="true" Display="Dynamic" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plDescription" controlname="txtDescription" runat="server" />
            <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="5"></asp:TextBox>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plAddPage" controlname="chkAddPage" runat="server" />
            <asp:CheckBox ID="chkAddPage" runat="server" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li><asp:linkbutton id="cmdCreate" resourcekey="cmdCreate" runat="server" cssclass="dnnPrimaryAction" /></li>
        <li><asp:linkbutton id="cmdCancel" resourcekey="cmdCancel" runat="server" cssclass="dnnSecondaryAction" CausesValidation="false" /></li>
    </ul>
    <div class="dnnClear"><asp:PlaceHolder ID="phInstallLogs" runat="server" /></div>
</div>