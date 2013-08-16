<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.PackageWriter" CodeFile="PackageWriter.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/Controls/LabelControl.ascx" %>
<div class="dnnForm dnnExtensionsPackageWriter">
    <h2 class="dnnFormSectionHead"><asp:Label ID="lblTitle" runat="server" /></h2>
    <asp:Wizard ID="wizPackage" runat="server" DisplaySideBar="false" ActiveStepIndex="0" CellPadding="0" CellSpacing="0" DisplayCancelButton="True" CancelButtonType="Link" StartNextButtonType="Link" StepNextButtonType="Link" StepPreviousButtonType="Link" FinishCompleteButtonType="Link">
        <CancelButtonStyle CssClass="dnnSecondaryAction" />
        <StartNextButtonStyle CssClass="dnnPrimaryAction" />
        <StepNextButtonStyle CssClass="dnnPrimaryAction" />
        <FinishCompleteButtonStyle CssClass="dnnPrimaryAction" />
        <StepStyle VerticalAlign="Top" />
        <NavigationButtonStyle BorderStyle="None" BackColor="Transparent" />
        <HeaderTemplate>
            <h2 class="dnnFormSectionHead"><asp:Label ID="lblTitle" CssClass="Head" runat="server"><% =GetText("Title") %></asp:Label></h2>
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
                <li><asp:LinkButton id="StepPreviousButton" runat="server" CssClass="dnnSecondaryAction" CommandName="MovePrevious" resourcekey="Previous" /></li>
                <li><asp:LinkButton id="cancelButtonStep" runat="server" CssClass="dnnSecondaryAction" CommandName="Cancel" resourcekey="Cancel" Causesvalidation="False" /></li>
            </ul>
        </StepNavigationTemplate>
        <FinishNavigationTemplate>
            <ul class="dnnActions dnnClear">
    	        <li><asp:LinkButton id="finishButtonStep" runat="server" CssClass="dnnPrimaryAction" CommandName="MoveComplete" resourcekey="Cancel" /></li>
            </ul>
        </FinishNavigationTemplate>
        <WizardSteps>
            <asp:WizardStep ID="Step0" runat="Server" Title="Introduction" StepType="Start" AllowReturn="true">
                <div class="dnnFormItem">
                    <dnn:PropertyEditorControl ID="ctlPackage" runat="Server" AutoGenerate="false" SortMode="SortOrderAttribute" EditControlWidth="400px" ErrorStyle-CssClass="dnnFormError" HelpStyle-CssClass="dnnFormHelp">
                        <Fields>
                            <dnn:FieldEditorControl ID="fldName" runat="server" DataField="Name" />
                            <dnn:FieldEditorControl ID="fldPackageType" runat="server" DataField="PackageType" />
                            <dnn:FieldEditorControl ID="fldFriendlyName" runat="server" DataField="FriendlyName" />
                            <dnn:FieldEditorControl ID="fldIconFile" runat="server" DataField="IconFile" />
                            <dnn:FieldEditorControl ID="fldVersion" runat="server" DataField="Version" EditorTypeName="DotNetNuke.UI.WebControls.VersionEditControl"  />
                        </Fields>
                    </dnn:PropertyEditorControl>
                </div>
                <div class="dnnFormItem"><asp:Label ID="lblManifestHelp" runat="server" resourcekey="ManifestHelp" /></div>
                <div id="trUseManifest" runat="server" visible="false" class="dnnFormItem">
                    <dnn:Label ID="plUseManifest" runat="server" ControlName="chkUseManifest" />
                    <asp:CheckBox ID="chkUseManifest" runat="server" AutoPostBack="true" />
                </div>
                <div id="trManifestList" runat="server" visible="false" class="dnnFormItem">
                    <dnn:Label ID="plChooseManifest" runat="server" ControlName="cboManifests" />
                    <asp:DropDownList ID="cboManifests" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="plReviewManifest" runat="server" ControlName="chkReviewManifest" />
                    <asp:CheckBox ID="chkReviewManifest" runat="server" Checked="true" />
                </div>
            </asp:WizardStep>
            <asp:WizardStep ID="Step1" runat="server" Title="ChooseFiles" StepType="Step">
                <div class="dnnFormItem">
                    <dnn:Label ID="plBasePath" runat="server" ControlName="txtBasePath" />
                    <asp:TextBox ID="txtBasePath" runat="server" />
                    <dnn:CommandButton ID="cmdGetFiles" runat="server" ResourceKey="cmdGetFiles" IconKey="ActionRefresh" class="dnnSecondaryAction" />
                </div>
                <div class="dnnFormItem" id="includeSourceRow" runat="server"><asp:CheckBox ID="chkIncludeSource" runat="server" resourceKey="chkIncludeSource" TextAlign="Left" /></div>
                <div class="dnnFormItem extPWMiltiLine"><asp:TextBox ID="txtFiles" runat="server" TextMode="MultiLine" Rows="13" /></div>
            </asp:WizardStep>
            <asp:WizardStep ID="Step2" runat="server" Title="ChooseAssemblies" StepType="Step">
				<div class="dnnFormItem extPWMiltiLine"><asp:TextBox ID="txtAssemblies" runat="server" TextMode="MultiLine" Rows="15" /></div>
            </asp:WizardStep>
            <asp:WizardStep ID="Step3" runat="server" Title="CreateManifest" StepType="Step">
				<div class="dnnFormItem extPWMiltiLine"><asp:TextBox ID="txtManifest" runat="server" TextMode="MultiLine" Rows="15" /></div>
            </asp:WizardStep>
            <asp:WizardStep ID="StepLast" runat="Server" Title="FinalStep" StepType="Step" AllowReturn="false">
                <div class="dnnFormItem" id="trManifest1" runat="server">
                    <dnn:label id="plManifest" controlname="chkManifest" runat="server" />
                    <asp:CheckBox ID="chkManifest" runat="server" Checked="true" />
                </div>
                <div class="dnnFormItem" id="trManifest2" runat="server">
                    <dnn:label id="plManifestName" controlname="txtManifestName" runat="server" />
                    <asp:TextBox ID="txtManifestName" runat="server" Style="width: 250px" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plPackage" controlname="chkPackage" runat="server" />
                    <asp:CheckBox ID="chkPackage" runat="server" Checked="true" />
                </div>
                <div class="dnnFormItem">         
                    <dnn:label id="plArchiveName" controlname="txtArchiveName" runat="server" />
                    <asp:TextBox ID="txtArchiveName" runat="server" Style="width: 250px" />
                </div>                            
                <div class="dnnFormMessage dnnFormValidationSummary" runat="server" Visible="false"><asp:Label ID="lblMessage" runat="server" EnableViewState="False" /></div>
            </asp:WizardStep>
            <asp:WizardStep ID="StepFinish" runat="Server" Title="WriterResults" StepType="Finish">
                <div class="dnnFormItem"><asp:PlaceHolder ID="phInstallLogs" runat="server" /></div>
            </asp:WizardStep>
        </WizardSteps>
    </asp:Wizard>
</div>