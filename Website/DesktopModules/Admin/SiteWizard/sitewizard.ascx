<%@ Control Inherits="DotNetNuke.Modules.Admin.Portals.SiteWizard" Language="C#" AutoEventWireup="false" EnableViewState="True" CodeFile="SiteWizard.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Skin" Src="~/controls/SkinThumbNailControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="url" Src="~/controls/UrlControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/LabelControl.ascx" %>
<asp:Wizard ID="Wizard" runat="server" DisplaySideBar="false" ActiveStepIndex="0" 
    StartNextButtonType="Link" StepNextButtonType="Link" StepPreviousButtonType="Link" FinishPreviousButtonType="Link"
     FinishCompleteButtonType="Link" CssClass="dnnSiteWizard dnnClear">
    <StepStyle VerticalAlign="Top" />
    <CancelButtonStyle CssClass="dnnSecondaryAction" />
    <StartNextButtonStyle CssClass="dnnPrimaryAction" />
    <StepNextButtonStyle CssClass="dnnPrimaryAction" />
    <FinishCompleteButtonStyle CssClass="dnnPrimaryAction" />
    <StepPreviousButtonStyle CssClass="dnnSecondaryAction"></StepPreviousButtonStyle>
    <HeaderTemplate>
        <h2><% =Localization.GetString(Wizard.ActiveStep.Title + ".Title", LocalResourceFile)%></h2>
        <div class="dnnFormMessage dnnFormInfo">
            <% =Localization.GetString(Wizard.ActiveStep.Title + ".Help", LocalResourceFile)%>
        </div>
    </HeaderTemplate>
    <StartNavigationTemplate>
        <ul class="dnnActions dnnClear">
    	    <li><asp:LinkButton id="nextButtonStart" runat="server" CssClass="dnnPrimaryAction" CommandName="MoveNext" resourcekey="Next" /></li>
        </ul>
    </StartNavigationTemplate>
    <StepNavigationTemplate>
        <ul class="dnnActions dnnClear">
    	    <li><asp:LinkButton id="nextButtonStep" runat="server" CssClass="dnnPrimaryAction" CommandName="MoveNext" resourcekey="Next" /></li>
            <li><asp:LinkButton id="StepPreviousButton" runat="server" CssClass="dnnSecondaryAction" CommandName="MovePrevious" resourcekey="Previous" /></li>
        </ul>
    </StepNavigationTemplate>
    <FinishNavigationTemplate>
        <ul class="dnnActions dnnClear">
    	    <li><asp:LinkButton id="finishButtonStep" runat="server" CssClass="dnnPrimaryAction" CommandName="MoveComplete" resourcekey="Finish" /></li>
            <li><asp:LinkButton id="StepPreviousButton" runat="server" CssClass="dnnSecondaryAction" CommandName="MovePrevious" resourcekey="Previous" /></li>
        </ul>
    </FinishNavigationTemplate>
    <WizardSteps>
        <asp:WizardStep ID="wizIntroduction" runat="server" Title="Introduction" StepType="Start" AllowReturn="false" />
        <asp:WizardStep ID="wizTemplate" runat="server" Title="Template">
            <div class="dnnForm dnnSiteWizardStep2 dnnClear">
                <div class="dnnFormItem">
                    <dnn:Label ID="lblTemplateMessage" runat="server" ControlName="chkTemplate" />
                    <asp:CheckBox ID="chkTemplate" runat="server" AutoPostBack="True" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="lblTemplateList" runat="server" ControlName="lstTemplate" />
                    <asp:ListBox ID="lstTemplate" runat="server" Rows="8" Width="350" AutoPostBack="True" />
                    <asp:Label ID="lblTemplateDescription" runat="server"></asp:Label>
                </div>
                <div class="dnnFormItem dnnSWMergeModules">
                    <dnn:Label ID="lblMergeModule" runat="server" ControlName="optMerge" />    
                    <asp:RadioButtonList ID="optMerge" CssClass="dnnFormRadioButtons" runat="server" RepeatDirection="Horizontal">
                        <asp:ListItem Value="Ignore" resourcekey="Ignore" Selected="true"></asp:ListItem>
                        <asp:ListItem Value="Replace" resourcekey="Replace"></asp:ListItem>
                        <asp:ListItem Value="Merge" resourcekey="Merge"></asp:ListItem>
                    </asp:RadioButtonList>
                </div>
                <div class="dnnFormItem">
                    <asp:Label ID="lblMergeWarning" runat="server" resourcekey="MergeWarning" CssClass="dnnFormMessage dnnFormValidationSummary" />
                </div>
            </div>
        </asp:WizardStep>
        <asp:WizardStep ID="wizSkin" runat="server" Title="Skin">
            <div class="dnnForm dnnSiteWizardStep3 dnnClear">
                <dnn:Skin ID="ctlPortalSkin" runat="server"></dnn:Skin>
            </div>
        </asp:WizardStep>
        <asp:WizardStep ID="wizContainer" runat="server" Title="Container">
            <div class="dnnForm dnnSiteWizardStep4 dnnClear">
                <div class="dnnFormItem">
                    <asp:CheckBox ID="chkIncludeAll" runat="server" resourcekey="IncludeAll" TextAlign="Left" AutoPostBack="True" />
                </div>
                <div>
                    <dnn:Skin ID="ctlPortalContainer" runat="server"></dnn:Skin>
                </div>
            </div>
        </asp:WizardStep>
        <asp:WizardStep ID="wizDetails" runat="server" Title="Details">
            <div class="dnnForm dnnSiteWizardStep5 dnnClear">
                <div class="dnnFormItem">
                    <dnn:label ID="lblPortalName" runat="server" ControlName="txtPortalName" />
                    <asp:TextBox ID="txtPortalName" runat="server" MaxLength="128" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label ID="lblDescription" runat="server" ControlName="txtDescription" />
                    <asp:TextBox ID="txtDescription" runat="server" Width="50%" MaxLength="475" Rows="3" TextMode="MultiLine" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label ID="lblKeyWords" runat="server" ControlName="txtKeyWords" />
                    <asp:TextBox ID="txtKeyWords" runat="server" Width="50%" MaxLength="475" Rows="3" TextMode="MultiLine" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label ID="lblLogo" runat="server" ControlName="urlLogo" />
                    <dnn:DnnFilePicker ID="ctlLogo" runat="server" Required="False" />
                </div>
            </div>
        </asp:WizardStep>
        <asp:WizardStep ID="wizComplete" runat="server" StepType="Complete">
            <div class="dnnForm dnnSiteWizardStep6 dnnClear">
                <h2><asp:Label ID="lblWizardTitle" resourcekey="Complete.Title" runat="server" /></h2>
                <div class="dnnFormMessage dnnFormSuccess">
                    <asp:Label ID="lblHelp" resourcekey="Complete.Help" runat="server" />
                </div>            
            </div>
        </asp:WizardStep>
    </WizardSteps>
</asp:Wizard>