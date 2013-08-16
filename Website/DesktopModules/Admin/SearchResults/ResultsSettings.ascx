<%@ Control Language="C#" AutoEventWireup="false" Explicit="true" Inherits="DotNetNuke.Modules.SearchResults.ResultsSettings" CodeFile="ResultsSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnClear">
    <div class="dnnFormItem">
        <dnn:Label ID="plTitleLinkTarget" runat="server" ControlName="comboBoxLinkTarget" />
        <dnn:DnnComboBox ID="comboBoxLinkTarget" runat="server" Width="437px">
            <Items>
                <dnn:DnnComboBoxItem ResourceKey="linkTargetOnSamePage.Text" Value="0" />
                <dnn:DnnComboBoxItem ResourceKey="linkTargetOpenNewPage.Text" Value="1" />
            </Items>
        </dnn:DnnComboBox>
    </div>
    <div class="dnnFormItem" id="divPortalGroup" runat="server">
        <dnn:Label ID="plResultsScopeForPortals" runat="server" ControlName="comboBoxPortals" />
        <dnn:DnnComboBox ID="comboBoxPortals" runat="server" CheckBoxes="true" Width="437px">
        </dnn:DnnComboBox>
        <asp:RequiredFieldValidator runat="server" ID="portalsRequiedValidator" CssClass="dnnFormMessage dnnFormError" Display="Dynamic"
            resourceKey="portalsRequired" ControlToValidate="comboBoxPortals"></asp:RequiredFieldValidator>
    </div>

    <div class="dnnFormItem">
        <dnn:Label ID="plResultsScopeForFilters" runat="server" ControlName="comboBoxFilters" />
        <dnn:DnnComboBox ID="comboBoxFilters" runat="server" CheckBoxes="true" Width="437px">
        </dnn:DnnComboBox>
        <asp:RequiredFieldValidator runat="server" ID="filtersRequiredFieldValidator" CssClass="dnnFormMessage dnnFormError" Display="Dynamic"
            resourceKey="filtersRequired" ControlToValidate="comboBoxFilters"></asp:RequiredFieldValidator>
    </div>
    
      <div class="dnnFormItem">
        <dnn:Label ID="plEnableWildSearch" runat="server" ControlName="chkEnableWildSearch" />
        <asp:CheckBox runat="server" ID="chkEnableWildSearch" />
    </div>
</div>
