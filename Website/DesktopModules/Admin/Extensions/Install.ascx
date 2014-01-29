<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.Install" CodeFile="Install.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnInstallExtension dnnClear" id="dnnInstallExtension">
    <asp:Wizard ID="wizInstall" runat="server"  DisplaySideBar="false" ActiveStepIndex="0" CellPadding="0" CellSpacing="0" width="100%" DisplayCancelButton="True" CancelButtonType="Link" StartNextButtonType="Link" StepNextButtonType="Link"  FinishCompleteButtonType="Link">
        <CancelButtonStyle CssClass="dnnSecondaryAction" />
        <StartNextButtonStyle CssClass="dnnPrimaryAction" />
        <StepNextButtonStyle CssClass="dnnPrimaryAction" />
        <FinishCompleteButtonStyle CssClass="dnnPrimaryAction" />
        <StepStyle VerticalAlign="Top" />
        <NavigationButtonStyle BorderStyle="None" BackColor="Transparent" />
        <HeaderTemplate>
            <h2 class="dnnFormSectionHead"><asp:Label ID="lblTitle" runat="server"><% =GetText("Title") %></asp:Label></h2>
            <div class="dnnFormMessage dnnFormInfo"><asp:Label ID="lblHelp" runat="server"><% =GetText("Help") %></asp:Label></div>
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
    	        <li><asp:LinkButton id="finishButtonStep" runat="server" CssClass="dnnPrimaryAction" CommandName="MoveComplete" resourcekey="Return" /></li>
            </ul>
        </FinishNavigationTemplate>
        <WizardSteps>
            <asp:WizardStep ID="Step0" runat="Server" Title="Introduction" StepType="Start" AllowReturn="false">
                <div class="dnnForm">
                    <div class="dnnFormItem"><asp:Label ID="lblBrowseFileHelp" runat="server" resourcekey="BrowseFileHelp" /></div>
                    <div class="dnnFormItem dnnClear">
                        <input id="cmdBrowse" type="file" size="50" name="cmdBrowse" runat="server" />
                        <asp:Label ID="lblLoadMessage" runat="server" CssClass="dnnFormMessage dnnFormValidationSummary" Visible="false" />
                    </div>
                </div>
                <div class="dnnFormMessage dnnFormInfo">
                    <asp:Label id="maxSizeWarningLabel" runat="server" />
                </div>
            </asp:WizardStep>
            <asp:WizardStep ID="Step1" runat="server" Title="Warnings" StepType="Step" AllowReturn="false">
                <div class="dnnFormMessage dnnFormWarning" id="lblWarningMessageWrapper" runat="server">
                    <asp:Label ID="lblWarningMessage" runat="server" EnableViewState="False" />
                </div>
                <asp:Panel ID="pnlRepair" runat="server" Visible="false">
                    <p><asp:Label ID="lblRepairInstallHelp" runat="server" resourcekey="RepairInstallHelp" /></p>
                    <p><strong><asp:CheckBox ID="chkRepairInstall" runat="server" resourcekey="RepairInstall" TextAlign="Left" AutoPostBack="true" /></strong></p>
                </asp:Panel>
                <asp:Panel ID="pnlLegacy" runat="server" Visible="false">
                    <asp:RadioButtonList ID="rblLegacySkin" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" CssClass="dnnFormRadioButtons">
                        <asp:ListItem Value="Skin" resourcekey="Skin" />
                        <asp:ListItem Value="Container" resourcekey="Container" />
                    </asp:RadioButtonList>
                </asp:Panel>
                <asp:Panel ID="pnlWhitelist" runat = "server" Visible="false">
                    <asp:Label ID="lblIgnoreWhiteListHelp" runat="server" resourcekey="IgnoreWhiteListHelp" />
                    <asp:CheckBox ID="chkIgnoreWhiteList" runat="server" resourcekey="IgnoreWhiteList" TextAlign="Left" AutoPostBack="true" />
                </asp:Panel>
				<asp:Panel ID="pnlAzureCompact" runat = "server" Visible="false">
                    <p><asp:Label ID="lblAzureCompact" runat="server" /></p>
                    <p><strong><asp:CheckBox ID="chkAzureCompact" runat="server" resourcekey="AzureCompact" TextAlign="Left" AutoPostBack="true" /></strong></p>
                </asp:Panel>
                <asp:PlaceHolder ID="phLoadLogs" runat="server" />
            </asp:WizardStep>
            <asp:WizardStep ID="Step2" runat="Server" Title="PackageInfo" StepType="Step" AllowReturn="false">
                <dnn:DnnFormEditor id="packageForm" runat="Server" FormMode="Short">
                    <Items>
                        <dnn:DnnFormLiteralItem ID="moduleName" runat="server" DataField = "Name" />
                        <dnn:DnnFormLiteralItem ID="packageType" runat="server" DataField = "PackageType" />
                        <dnn:DnnFormLiteralItem ID="packageFriendlyName" runat="server" DataField = "FriendlyName" />
                        <dnn:DnnFormLiteralItem ID="iconFile" runat="server" DataField = "IconFile" />
                        <dnn:DnnFormLiteralItem ID="description" runat="server" DataField = "Description" />
                        <dnn:DnnFormLiteralItem ID="version" runat="server" DataField = "Version" />
                        <dnn:DnnFormLiteralItem ID="owner" runat="server" DataField = "Owner" />
                        <dnn:DnnFormLiteralItem ID="organization" runat="server" DataField = "Organization" />
                        <dnn:DnnFormLiteralItem ID="url" runat="server" DataField = "URL" />
                        <dnn:DnnFormLiteralItem ID="email" runat="server" DataField = "Email" />
                    </Items>
                </dnn:DnnFormEditor>
            </asp:WizardStep>
            <asp:WizardStep ID="Step3" runat="Server" Title="ReleaseNotes" StepType="Step" AllowReturn="false">
                <dnn:DnnFormEditor id="releaseNotesForm" runat="Server" FormMode="Short">
                    <Items><dnn:DnnFormLiteralItem ID="releaseNotes" runat="server" DataField="ReleaseNotes" /></Items>
                </dnn:DnnFormEditor>
            </asp:WizardStep>
            <asp:WizardStep ID="Step4" runat="server" Title="License" StepType="Step" AllowReturn="false">
                <dnn:DnnFormEditor id="licenseForm" runat="Server" FormMode="Short">
                    <Items><dnn:DnnFormLiteralItem ID="license" runat="server" DataField="License" /></Items>
                </dnn:DnnFormEditor>
                <div class="dnnFormItem">
                    <dnn:Label ID="plAcceptLicense" runat="server" resourcekey="AcceptLicense" ControlName="chkAcceptLicense" />
                    <asp:CheckBox ID="chkAcceptLicense" runat="server"  CssClass="dnnFormLabel" />
                    <asp:Label ID="lblAcceptMessage" runat="server" Visible="false" EnableViewState="False" CssClass="dnnFormMessage dnnFormError" />
                </div>
                <div class="dnnFormItem"><asp:PlaceHolder ID="phAcceptLogs" runat="server" /></div>
            </asp:WizardStep>
            <asp:WizardStep ID="Step5" runat="Server" Title="InstallResults" StepType="Finish">
                <div class="dnnFormMessage dnnFormValidationSummary" id="lblInstallMessageRow" runat="server"><asp:Label ID="lblInstallMessage" runat="server" EnableViewState="False" /></div>
                <div class="dnnFormItem dnnInstallLogs dnnClear"><asp:PlaceHolder ID="phInstallLogs" runat="server" /></div>
            </asp:WizardStep>
        </WizardSteps>
    </asp:Wizard>
</div>
<script type="text/javascript">
	/*globals jQuery, window, Sys */
	(function ($, Sys) {
		function setUpInstallWizard() {
			var actionLinks = $("a[id$=nextButtonStart], a[id$=cancelButtonStart], a[id$=nextButtonStep], a[id$=cancelButtonStep], a[id$=finishButtonStep]");
			actionLinks.click(function () {
				if ($(this).hasClass("dnnDisabledAction")) {
					return false;
				}

				actionLinks.addClass("dnnDisabledAction");
			    //show the loading icon
				var loading = $("<div class=\"dnnLoading\"></div>");
			    var container = $('#dnnInstallExtension');
			    loading.css({
			        width: container.width(),
			        height: container.height()
			    });
			    container.prepend(loading);
			});
		}
		
		$(document).ready(function () {
			setUpInstallWizard();
			Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
				setUpInstallWizard();
			});
		});
	}(jQuery, window.Sys));
</script>   