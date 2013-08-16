<%@ Control Inherits="DotNetNuke.Modules.Admin.Users.EditProfileDefinition" Language="C#" AutoEventWireup="false" CodeFile="EditProfileDefinition.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="ListEntries" Src="~/DesktopModules/Admin/Lists/ListEntries.ascx" %>
<asp:Wizard ID="Wizard" runat="server" DisplaySideBar="false" ActiveStepIndex="0" CellPadding="0" CellSpacing="0" CssClass="dnnEditProfileDef" DisplayCancelButton="True" CancelButtonType="Link" StartNextButtonType="Link" StepNextButtonType="Link" FinishCompleteButtonType="Link">
    <CancelButtonStyle CssClass="dnnSecondaryAction" />
    <StartNextButtonStyle CssClass="dnnPrimaryAction" />
    <StepNextButtonStyle CssClass="dnnPrimaryAction" />
    <FinishCompleteButtonStyle CssClass="dnnPrimaryAction" />
    <StepStyle VerticalAlign="Top" />
    <NavigationButtonStyle BorderStyle="None" BackColor="Transparent" />
    <HeaderTemplate>
        <h2 class="dnnFormSectionHead"><asp:Label ID="lblTitle" runat="server"><% =GetText("Title") %></asp:Label></h2>
        <%--div class="dnnFormItem dnnFormHelp dnnClear"><p class="dnnFormRequired"><span><%=LocalizeString("RequiredFields")%></span></p></div>--%>
        <div class="dnnFormItem dnnClear"><asp:Label ID="lblHelp" CssClass="dnnFormMessage dnnFormInfo" runat="server"><% =GetText("Help") %></asp:Label></div>
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
        <asp:WizardStep ID="wizIntroduction" runat="server" Title="Introduction" StepType="Start" AllowReturn="false">
            <dnn:propertyeditorcontrol id="Properties" runat="Server" SortMode="SortOrderAttribute" 
                                ErrorStyle-cssclass="dnnFormMessage dnnFormError" helpstyle-cssclass="dnnFormHelpContent dnnClear" EnableClientValidation="True">

            </dnn:propertyeditorcontrol>
        </asp:WizardStep>
        <asp:WizardStep ID="wizLists" runat="server" Title="List" AllowReturn="false"><dnn:ListEntries id="lstEntries" runat="server" /></asp:WizardStep>
		<asp:WizardStep ID="wizLocalization" runat="server" Title="Localization" AllowReturn="false">
            <div class="dnnForm">
                <div class="dnnFormItem">
                    <dnn:Label ID="plLocales" runat="server" ControlName="cboLocales" />
                    <%--<asp:DropDownList ID="cboLocales" runat="server" AutoPostBack="True" DataValueField="key" DataTextField="name" />--%>
                    <dnn:DnnComboBox ID="cboLocales" runat="server" AutoPostBack="True" DataValueField="key" DataTextField="name" />
                    <asp:Label ID="lblLocales" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="plPropertyName" runat="server" ControlName="txtPropertyName" />
                    <asp:TextBox ID="txtPropertyName" runat="Server" CssClass="dnnFormRequired" />
                    <asp:requiredfieldvalidator id="valPropertyName" runat="server" controltovalidate="txtPropertyName" display="Dynamic" 
                    resourcekey="valPropertyName.ErrorMessage" CssClass="dnnFormMessage dnnFormError" />
                </div>
                <div class="dnnFormItem">				
                    <dnn:Label ID="plPropertyHelp" runat="server" ControlName="txtPropertyHelp" />
                    <asp:TextBox ID="txtPropertyHelp" runat="Server" Rows="5" TextMode="MultiLine" Wrap="true" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="plPropertyRequired" runat="server" ControlName="txtPropertyRequired" />
                    <asp:TextBox ID="txtPropertyRequired" runat="Server"  />
                </div>
                <div class="dnnFormItem">				
                    <dnn:Label ID="plPropertyValidation" runat="server" ControlName="txtPropertyValidation" />
                    <asp:TextBox ID="txtPropertyValidation" runat="Server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="plCategoryName" runat="server" ControlName="txtCategoryName" />
                    <asp:TextBox ID="txtCategoryName" runat="Server" />
                </div>
            </div>	            
        </asp:WizardStep>
    </WizardSteps>
</asp:Wizard>