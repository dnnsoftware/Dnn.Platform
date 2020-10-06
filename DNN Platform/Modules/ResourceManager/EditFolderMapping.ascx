<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EditFolderMapping.ascx.cs" Inherits="Dnn.Modules.ResourceManager.EditFolderMapping" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnEditFolderMapping dnnClear" id="dnnEditFolderMapping">
	<div class="dnnFormMessage dnnFormInfo"><asp:Label ID="DescriptionLabel" runat="server" resourcekey="Description"></asp:Label></div>
    <asp:PlaceHolder id="SyncWarningPlaceHolder" runat="server">
        <div class="dnnFormMessage dnnFormInfo"><asp:Label ID="SyncLabel" runat="server" Visible="true" ResourceKey="SyncWarning" /></div>
    </asp:PlaceHolder>
    <div class="EditFolderMappingContent dnnClear">
        <div class="dnnFormItem dnnFormHelp dnnClear"><p class="dnnFormRequired"><span><%=LocalizeString("RequiredFields")%></span></p></div>
        <div id="exFolderMappingSettings" class="exFolderMappingSettings dnnClear">
            <div class="exfmsContent dnnClear">
                <h2 id="Panel-GeneralSettings" class="dnnFormSectionHead"><%=LocalizeString("GeneralSettings")%></h2>
                <fieldset>
                    <div class="dnnFormItem">
                        <dnn:label id="NameLabel" runat="server" controlname="txtName" CssClass="dnnFormRequired"  />
                        <asp:TextBox ID="NameTextbox" runat="server" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="NameValidator" runat="server" ControlToValidate="NameTextbox" Display="Dynamic" CssClass="dnnFormMessage dnnFormError" resourcekey="NameValidator.ErrorMessage" ValidationGroup="vgEditFolderMapping"></asp:RequiredFieldValidator>
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="FolderProviderLabel" runat="server" controlname="cboFolderProviders" />  
                        <asp:DropDownList ID="FolderProvidersComboBox" runat="server" AutoPostBack="true" CausesValidation="false" OnSelectedIndexChanged="CboFolderProviders_SelectedIndexChanged" />
                        <asp:RequiredFieldValidator ID="FolderProviderValidator" runat="server" ControlToValidate="FolderProvidersComboBox" InitialValue="" Display="Dynamic" CssClass="dnnFormMessage dnnFormError" resourcekey="FolderProviderValidator.ErrorMessage" EnableClientScript="false" ValidationGroup="vgEditFolderMapping"></asp:RequiredFieldValidator>
                    </div>
                </fieldset>
                <h2 id="Panel-FolderProviderSettings" class="dnnFormSectionHead"><%=LocalizeString("FolderProviderSettings")%></h2>
                <fieldset>
                    <asp:PlaceHolder ID="ProviderSettingsPlaceHolder" runat="server"></asp:PlaceHolder>
                </fieldset>
            </div>
        </div>
    </div>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="UpdateButton" runat="server" CssClass="dnnPrimaryAction" ResourceKey="cmdUpdate" IconKey="Add" ValidationGroup="vgEditFolderMapping" />        
        </li>
        <li><asp:HyperLink ID="CancelHyperLink" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
    </ul>
</div>