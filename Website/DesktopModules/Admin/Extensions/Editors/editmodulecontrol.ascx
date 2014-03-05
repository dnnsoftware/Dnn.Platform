<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.ModuleDefinitions.EditModuleControl" CodeFile="EditModuleControl.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div class="dnnForm dnnEditModuleControl dnnClear" id="dnnEditModuleControl">
    <fieldset>
        <div class="dnnFormItem">
            <dnn:label id="plModule" controlname="lblModule" runat="server" />
            <asp:Label id="lblModule" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plDefinition" controlname="lblDefinition" runat="server" />
            <asp:Label id="lblDefinition" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plKey" controlname="txtKey" runat="server" />
            <asp:textbox id="txtKey" columns="30" maxlength="50" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plTitle" controlname="txtTitle" runat="server" />
            <asp:textbox id="txtTitle" columns="30" maxlength="50" runat="server" />
        </div>
         <div class="dnnFormItem">
            <dnn:label id="plSourceFolder" controlname="cboSourceFolder" runat="server" />
            <dnn:DnnComboBox id="cboSourceFolder" runat="server" autopostback="True" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plSource" controlname="cboSource" runat="server" />
            <%--<asp:dropdownlist id="cboSource" runat="server" autopostback="True" />--%>
            <dnn:DnnComboBox id="cboSource" runat="server" autopostback="True" />
        </div>
        <div class="dnnFormItem">
            <div class="dnnLabel"></div>
            <asp:textbox id="txtSource" columns="30" maxlength="100" runat="server" CssClass="emcTextSource" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plType" controlname="cboType" runat="server" />
            <dnn:DnnComboBox ID="cboType" runat="server">
                <Items>
                    <dnn:DnnComboBoxItem resourcekey="Skin" value="-2" />
				    <dnn:DnnComboBoxItem resourcekey="Anonymous" value="-1" />
				    <dnn:DnnComboBoxItem resourcekey="View" value="0" />
				    <dnn:DnnComboBoxItem resourcekey="Edit" value="1" />
				    <dnn:DnnComboBoxItem resourcekey="Admin" value="2" />
				    <dnn:DnnComboBoxItem resourcekey="Host" value="3" />
                </Items>
            </dnn:DnnComboBox>
          <%--  <asp:dropdownlist id="cboType" runat="server">
				<asp:listitem resourcekey="Skin" value="-2" />
				<asp:listitem resourcekey="Anonymous" value="-1" />
				<asp:listitem resourcekey="View" value="0" />
				<asp:listitem resourcekey="Edit" value="1" />
				<asp:listitem resourcekey="Admin" value="2" />
				<asp:listitem resourcekey="Host" value="3" />
			</asp:dropdownlist>--%>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plViewOrder" controlname="txtViewOrder" runat="server" />
            <asp:textbox id="txtViewOrder" columns="30" maxlength="2" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plIcon" controlname="cboIcon" runat="server" />
           <%-- <asp:dropdownlist id="cboIcon" runat="server" datavaluefield="Value" datatextfield="Text" />--%>
            <dnn:DnnComboBox id="cboIcon" runat="server" datavaluefield="Value" datatextfield="Text" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plHelpURL" controlname="txtHelpURL" runat="server" />
            <asp:textbox id="txtHelpURL" runat="server" maxlength="200" columns="30" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="supportsModalPopUpsLabel" controlname="supportsModalPopUpsCheckBox" runat="server" />
            <asp:checkbox id="supportsModalPopUpsCheckBox" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plSupportsPartialRendering" controlname="chkSupportsPartialRendering" runat="server" />
            <asp:checkbox id="chkSupportsPartialRendering" runat="server" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li><asp:linkbutton id="cmdUpdate" resourcekey="cmdUpdate" runat="server" cssclass="dnnPrimaryAction"/></li>
        <li><asp:linkbutton id="cmdDelete" resourcekey="cmdDelete" runat="server" cssclass="dnnSecondaryAction" CausesValidation="false" /></li>
        <li><asp:linkbutton id="cmdCancel" resourcekey="cmdCancel" runat="server" cssclass="dnnSecondaryAction" CausesValidation="false" /></li>
    </ul>
</div>