<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.ModuleDefinitions.CreateModuleDefinition" CodeFile="EditModuleDefinition.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div class="dnnForm dnnEditDefinition dnnClear" id="dnnEditDefinition">
    <fieldset>
        <div class="dnnFormItem">
            <dnn:label id="plCreate" controlname="optCreate" runat="server" />            
           <%-- <asp:DropDownList ID="cboCreate" runat="server" AutoPostBack="True">
                <asp:ListItem Value="New" resourcekey="New" />
                <asp:ListItem Value="Control" resourcekey="Control" />
                <asp:ListItem Value="Manifest" resourcekey="Manifest" />
            </asp:DropDownList>--%>
            <dnn:DnnComboBox  ID="cboCreate" runat="server" AutoPostBack="True">
                <Items>
                    <dnn:DnnComboBoxItem Value="New" Text="New" ResourceKey="New" />
                    <dnn:DnnComboBoxItem Value="Control" Text="Control" ResourceKey="Control" />
                    <dnn:DnnComboBoxItem Value="Manifest" Text="Manifest" ResourceKey="Manifest" />
                </Items>
            </dnn:DnnComboBox>
        </div>
        <div id="rowOwner1" runat="server" visible="false" class="dnnFormItem">
            <dnn:label id="plOwner1" runat="server" controlname="cboOwner"/>
            <%--<asp:DropDownList ID="cboOwner" runat="server" AutoPostBack="True" />--%>
            <dnn:DnnComboBox ID="cboOwner" runat="server" AutoPostBack="True" CssClass="dnnFixedSizeComboBox" />
            <asp:linkbutton CssClass="dnnSecondaryAction" id="cmdAddOwner" resourcekey="cmdAdd" runat="server" Visible= "false" />
        </div>
        <div id="rowOwner2" runat="server" visible="false" class="dnnFormItem">
            <dnn:label id="plOwner2" runat="server" controlname="txtOwner"/>
            <asp:TextBox ID="txtOwner" Runat="server"  MaxLength="255" CssClass="dnnFixedSizeComboBox" />
            <asp:linkbutton cssclass="dnnSecondaryAction" id="cmdSaveOwner" resourcekey="cmdSave" runat="server" />
            <asp:linkbutton cssclass="dnnSecondaryAction" id="cmdCancelOwner" resourcekey="cmdCancel" runat="server" />
        </div>
        <div id="rowModule1" runat="server" visible="false" class="dnnFormItem">
            <dnn:label id="plModule1" runat="server" controlname="cboModule" />
            <%--<asp:DropDownList ID="cboModule" runat="server" AutoPostBack="true" />--%>
            <dnn:DnnComboBox ID="cboModule" runat="server" AutoPostBack="true"  CssClass="dnnFixedSizeComboBox"/>
            <asp:linkbutton cssclass="dnnSecondaryAction" id="cmdAddModule" resourcekey="cmdAdd" runat="server" Visible="false" />
        </div>
        <div id="rowModule2" runat="server" visible="false" class="dnnFormItem">
            <dnn:label id="plModule2" runat="server" controlname="txtModule" />
            <asp:TextBox ID="txtModule" Runat="server" MaxLength="255" ValidationGroup="first" CssClass="dnnFixedSizeComboBox" />
            <asp:linkbutton cssclass="dnnSecondaryAction" id="cmdSaveModule" resourcekey="cmdSave" runat="server" />
            <asp:linkbutton cssclass="dnnSecondaryAction" id="cmdCancelModule" resourcekey="cmdCancel" runat="server" />
        </div>
        <div id="rowFile1" runat="server" visible="false" class="dnnFormItem">
            <dnn:label id="plFile1" controlname="cboFile" runat="server" />
            <%--<asp:dropdownlist id="cboFile" runat="server" />--%>
            <dnn:DnnComboBox id="cboFile" runat="server" />
        </div>
        <div id="rowLang" runat="server" visible="false" class="dnnFormItem">
            <dnn:label id="plLang" controlname="rblLanguage" runat="server" />
            <asp:RadioButtonList ID="rblLanguage" CssClass="dnnFormRadioButtons" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                <asp:ListItem Value="C#" resourcekey="CSharp" />
                <asp:ListItem Value="VB" resourcekey="VisualBasic" />
            </asp:RadioButtonList>
        </div>
        <div id="rowFile2" runat="server" visible="false" class="dnnFormItem">
            <dnn:label id="plFile2" controlname="txtFile" runat="server" />
            <asp:TextBox ID="txtFile" Runat="server" MaxLength="255" />
        </div>
        <div id="rowName" runat="server" visible="false" class="dnnFormItem">
            <dnn:label id="plName" controlname="txtName" runat="server" />
            <asp:TextBox ID="txtName" runat="server" />
        </div>
        <div id="rowDescription" runat="server" visible="false" class="dnnFormItem">
            <dnn:label id="plDescription" controlname="txtDescription" runat="server" />
            <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="8" />
        </div>
        <div id="rowSource" runat="server" visible="false" class="dnnFormItem">
            <dnn:label id="plSource" controlname="txtSource" runat="server" />
            <asp:TextBox ID="txtSource" runat="server" TextMode="MultiLine" Rows="8" />
        </div>
        <div id="rowAddPage" runat="server" visible="false" class="dnnFormItem">
            <dnn:label id="plAddPage" controlname="chkAddPage" runat="server" />
            <asp:CheckBox ID="chkAddPage" runat="server" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li><asp:linkbutton id="cmdCreate" resourcekey="cmdCreate" runat="server" cssclass="dnnPrimaryAction"/></li>
        <li><asp:linkbutton id="cmdCancel" resourcekey="cmdCancel" runat="server" cssclass="dnnSecondaryAction" CausesValidation="false" /></li>
    </ul>
</div>
<div class="dnnClear"><asp:PlaceHolder ID="phInstallLogs" runat="server" /></div>