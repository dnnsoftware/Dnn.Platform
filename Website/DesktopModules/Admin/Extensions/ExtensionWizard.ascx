<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.ExtensionWizard" CodeFile="ExtensionWizard.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Security.Permissions.Controls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>

<div class="dnnForm dnnCreateExtension dnnClear" id="dnnCreateExtension">
    <asp:Wizard ID="wizNewExtension" runat="server"  DisplaySideBar="false" ActiveStepIndex="0"
        CellPadding="5" CellSpacing="5" 
        DisplayCancelButton="True"
        CancelButtonType="Link"
        StartNextButtonType="Link"
        StepNextButtonType="Link" 
        StepPreviousButtonType="Link"
        FinishCompleteButtonType="Link"
        >
        <CancelButtonStyle CssClass="dnnSecondaryAction" />
        <StartNextButtonStyle CssClass="dnnPrimaryAction" />
        <StepNextButtonStyle CssClass="dnnPrimaryAction" />
        <FinishCompleteButtonStyle CssClass="dnnPrimaryAction" />
        <StepStyle VerticalAlign="Top" />
        <NavigationButtonStyle BorderStyle="None" BackColor="Transparent" />
        <HeaderTemplate>
            <asp:Label ID="lblTitle" CssClass="Head" runat="server"><% =GetText("Title") %></asp:Label><br /><br />
            <asp:Label ID="lblHelp" CssClass="WizardText" runat="server"><% =GetText("Help") %></asp:Label>
        </HeaderTemplate>
        <StartNavigationTemplate>
            <ul class="dnnActions dnnClear">
    	        <li><asp:LinkButton id="nextButtonStart" runat="server" CssClass="dnnPrimaryAction" CommandName="MoveNext" resourcekey="Next" /></li>
                <li><asp:LinkButton id="cancelButtonStart" runat="server" CssClass="dnnSecondaryAction" CommandName="Cancel" resourcekey="Cancel" Causesvalidation="False" /></li>
            </ul>
        </StartNavigationTemplate>
        <StepNavigationTemplate>
            <ul class="dnnActions dnnClear">
    	        <li><asp:LinkButton id="nextButtonStep" runat="server" CssClass="dnnPrimaryAction" CommandName="MoveNext" resourcekey="Next" /></li>
                <li><asp:LinkButton id="cancelButtonStep" runat="server" CssClass="dnnSecondaryAction" CommandName="Cancel" resourcekey="Cancel" Causesvalidation="False" /></li>
            </ul>
        </StepNavigationTemplate>
        <FinishNavigationTemplate>
            <ul class="dnnActions dnnClear">
    	        <li><asp:LinkButton id="finishButtonStep" runat="server" CssClass="dnnPrimaryAction" CommandName="Cancel" resourcekey="Return" /></li>
            </ul>
        </FinishNavigationTemplate>
        <WizardSteps>
            <asp:WizardStep ID="Step0" runat="Server" Title="Introduction" StepType="Start" AllowReturn="false">
                <div class="dnnForm">
                    <div id="extensionTypeRow" runat="server" class="dnnFormItem">
                        <dnn:Label ID="plExtensionType" runat="server" ControlName="cboExtensionType" />
                        <%--<asp:DropDownList ID="cboExtensionType" runat="server" DataTextField="Description" DataValueField="PackageType" AutoPostBack="true"/>--%>
                        <dnn:DnnComboBox ID="cboExtensionType" runat="server" DataTextField="Description" DataValueField="PackageType" AutoPostBack="true"/>
                    </div>
                    <div class="dnnFormItem">
                        <asp:Label ID="lblHelp" runat="server" cssClass="WizardText" resourcekey="IntroductionHelp" />
                        <asp:Label ID="lblLanguageHelp" runat="server" cssClass="WizardText" resourcekey="LanguageHelp" />
                        <asp:Label ID="lblExtensionLanguageHelp" runat="server" cssClass="WizardText" resourcekey="ExtensionLanguageHelp" />
                    </div>
                </div>
                 <dnn:DnnFormEditor id="extensionForm" runat="Server" FormMode="Short">
                    <Items>
                        <dnn:DnnFormTextBoxItem ID="extensionName" runat="server" DataField="Name" Required="true" />
                        <dnn:DnnFormTextBoxItem ID="extensionFriendlyName" runat="server" DataField="FriendlyName" Required="true" />
                        <dnn:DnnFormTextBoxItem ID="description" runat="server" DataField="Description" />
                        <dnn:DnnFormEditControlItem ID="version" runat="server" DataField="Version" 
                                        ControlType="DotNetNuke.UI.WebControls.VersionEditControl, DotNetNuke"/>
                    </Items>
                </dnn:DnnFormEditor>
                <div class="dnnForm">
                    <div class="dnnFormItem">
                        <asp:Label ID="lblError" runat="server" cssClass="dnnFormMessage dnnFormValidationSummary" Visible="false" />
                    </div>
                </div>               
            </asp:WizardStep>
            <asp:WizardStep ID="Step1" runat="Server" Title="Specific" StepType="Step" AllowReturn="false">
                <asp:PlaceHolder ID="phEditor" runat="server" />
            </asp:WizardStep>
            <asp:WizardStep ID="Step2" runat="server" Title="OwnerInfo" StepType="Step" AllowReturn="false">
                <dnn:DnnFormEditor id="ownerForm" runat="Server" FormMode="Short">
                    <Items>
                        <dnn:DnnFormTextBoxItem ID="owner" runat="server" DataField="Owner" />
                        <dnn:DnnFormTextBoxItem ID="organization" runat="server" DataField="Organization" />
                        <dnn:DnnFormTextBoxItem ID="url" runat="server" DataField = "URL" />
                        <dnn:DnnFormTextBoxItem ID="email" runat="server" DataField="Email" />
                    </Items>
                </dnn:DnnFormEditor>                
            </asp:WizardStep>
        </WizardSteps>
    </asp:Wizard>
</div>
<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
    function setUpDnnExtensions() {
        $('#dnnCreateExtension').dnnPanels();
    }
    $(document).ready(function () {
        setUpDnnExtensions();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setUpDnnExtensions();
        });
    });
} (jQuery, window.Sys));
</script>