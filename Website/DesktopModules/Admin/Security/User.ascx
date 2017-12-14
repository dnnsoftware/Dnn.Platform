<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Users.User" Codebehind="User.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls.Internal" %>

<dnn:DnnFormEditor id="userForm" runat="Server" FormMode="Short">
    <Items>
        <dnn:DnnFormLiteralItem ID="userNameReadOnly" runat="server"  DataField="Username" />
        <dnn:DnnFormTextBoxItem ID="userName" runat="server" DataField="Username" Required="true"/>
        <dnn:DnnFormTextBoxItem ID="renameUserName" runat="server" DataField="Username" Visible="false" />                 
        <dnn:DnnFormTextBoxItem ID="firstName" runat="server" DataField="FirstName" Required="true" />
        <dnn:DnnFormTextBoxItem ID="lastName" runat="server" DataField="LastName" Required="true" />
        <dnn:DnnFormTextBoxItem ID="displayName" runat="server" DataField="DisplayName" Required="true" />
        <dnn:DnnFormLiteralItem ID="displayNameReadOnly" runat="server" DataField="DisplayName" />
        <dnn:DnnFormTextBoxItem ID="email" runat="server" DataField="Email" Required="true" />
   </Items>
</dnn:DnnFormEditor>
<div class="dnnFormGroup dnnFormItem dnnFormShort" id="renameUserPortals" runat="server" Visible="False">
    <dnn:Label ID="numSites" runat="server"></dnn:Label>
    <dnn:DnnComboBox CheckBoxes="false" id="cboSites" runat="server" Width="100" DataMember="PortalName" Visible="False" />
</div>

<asp:panel id="pnlAddUser" runat="server" visible="False" CssClass="dnnForm dnnFormPassword">
    <div id="AuthorizeNotify" runat="server" >
        <div class="dnnFormItem">
            <dnn:label id="plAuthorize" runat="server" controlname="chkAuthorize" />
            <asp:checkbox id="chkAuthorize" runat="server" checked="True" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plNotify" runat="server" controlname="chkNotify" />
            <asp:checkbox id="chkNotify" runat="server" checked="True" />
        </div>
    </div>
    <div id="Password" runat="server" >
        <div class="dnnFormItem">
            <div class="dnnLabel">            
            </div>
            <p class="dnnLeft" style="margin-top: 15px; font-weight:bold">
                <asp:label id="lblPasswordHelp" runat="server" />
            </p>
        </div>
        <div id="randomRow" runat="server" class="dnnFormItem">
            <dnn:label id="plRandom" runat="server" controlname="chkRandom" />
            <asp:checkbox id="chkRandom" runat="server" checked="True" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plPassword" runat="server" controlname="txtPassword" cssclass="dnnFormRequired"  />
            <asp:Panel ID="passwordContainer" runat="server">
    			<asp:textbox id="txtPassword" runat="server" TextMode="Password" size="12" maxlength="39" AutoCompleteType="Disabled" />
            </asp:Panel>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plConfirm" runat="server" controlname="txtConfirm" text="Confirm Password:" cssclass="dnnFormRequired"  ></dnn:label>
			<asp:textbox id="txtConfirm" runat="server" textmode="Password" size="12" maxlength="39" CssClass="password-confirm" />
            <asp:CompareValidator ID="ComparePasswordsValidator" runat="server"
                                  resourcekey = "ComparePasswordsValidator.ErrorMessage" 
                                  CssClass="dnnFormMessage dnnFormError"    
                                  Display="Dynamic"                               
                                  ControlToValidate="txtPassword" 
                                  ControlToCompare="txtConfirm"></asp:CompareValidator>
        </div>
        <div id="questionRow" runat="server"  class="dnnFormItem" visible="false">
            <dnn:label id="plQuestion" runat="server" controlname="lblQuestion"  cssclass="dnnFormRequired" />
			<asp:textbox id="txtQuestion" runat="server" size="25" maxlength="256" />
        </div>
        <div id="answerRow" runat="server" class="dnnFormItem" visible="false">
            <dnn:label id="plAnswer" runat="server" controlname="txtAnswer" cssclass="dnnFormRequired"  />
			<asp:textbox id="txtAnswer" runat="server" size="25" maxlength="128" />
        </div>
    </div>
</asp:panel>
<ul id="actionsRow" runat="server" class="dnnActions dnnClear">
    <li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
    <li><asp:LinkButton id="cmdDelete" runat="server" CssClass="dnnSecondaryAction"  /></li>
    <li><asp:LinkButton id="cmdRemove" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdRemove" /></li>
    <li><asp:LinkButton id="cmdRestore" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdRestore" /></li>
</ul>