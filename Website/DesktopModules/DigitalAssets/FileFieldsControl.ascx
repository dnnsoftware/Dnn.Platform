<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileFieldsControl.ascx.cs" Inherits="DotNetNuke.Modules.DigitalAssets.FileFieldsControl" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<asp:Panel runat="server" ID="ScopeWrapper">
    <div class="dnnFormItem">
        <dnn:Label ID="FileNameLabel" ControlName="FileNameInput" CssClass="dnnFormRequired" ResourceKey="FileNameLabel" runat="server" Suffix=":" />
        <asp:TextBox type="text" ID="FileNameInput" runat="server"/>
        <asp:RequiredFieldValidator ID="FileNameValidator" CssClass="dnnFormMessage dnnFormError"
            runat="server" resourcekey="FileNameRequired.ErrorMessage" Display="Dynamic" ControlToValidate="FileNameInput" />
        <asp:RegularExpressionValidator runat="server" Display="Dynamic" ControlToValidate="FileNameInput" CssClass="dnnFormMessage dnnFormError" 
            ID="FileNameInvalidCharactersValidator"/>
    </div>
    <asp:Panel runat="server" ID="FileAttributesContainer">
        <div class="dnnFormItem">
            <dnn:Label ID="FileAttributesLabel" ControlName="FileAttributArchiveCheckBox" ResourceKey="FileAttributesLabel" runat="server" Suffix=":" />
            <div id="FileAttrbituesCheckBoxGroup" class="dnnModuleDigitalAssetsGeneralPropertiesGroupedFields">
                <asp:CheckBox ID="FileAttributeArchiveCheckBox" runat="server" resourcekey="FileAttributeArchive" /><br/>
                <asp:CheckBox ID="FileAttributeHiddenCheckBox" runat="server" resourcekey="FileAttributeHidden" /><br/>
                <asp:CheckBox ID="FileAttributeReadonlyCheckBox" runat="server" resourcekey="FileAttributeReadonly" /><br/>
                <asp:CheckBox ID="FileAttributeSystemCheckBox" runat="server" resourcekey="FileAttributeSystem" />                                
            </div>
        </div>
    </asp:Panel>
</asp:Panel>