<%@ Control Language="C#" AutoEventWireup="false" Explicit="true" Inherits="DotNetNuke.Modules.SearchResults.ResultsSettings" CodeBehind="ResultsSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls.Internal" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnClear">
    <div class="dnnFormItem">
        <dnn:Label ID="plTitleLinkTarget" runat="server" ControlName="comboBoxLinkTarget" />
        <dnn:DnnComboBox ID="comboBoxLinkTarget" runat="server" Width="437px">
            <Items>
                <asp:ListItem ResourceKey="linkTargetOnSamePage.Text" Value="0" />
                <asp:ListItem ResourceKey="linkTargetOpenNewPage.Text" Value="1" />
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
    
    <div class="dnnFormItem" id="scopeForRolesRow">
        <dnn:Label ID="plResultsScopeForRoles" runat="server" ControlName="comboBoxRoles" />
        <dnn:DnnComboBox ID="comboBoxRoles" runat="server" CheckBoxes="true" Width="437px">
        </dnn:DnnComboBox>
    </div>

    <div class="dnnFormItem">
        <dnn:Label ID="plEnableWildSearch" runat="server" ControlName="chkEnableWildSearch" />
        <asp:CheckBox runat="server" ID="chkEnableWildSearch" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plShowDescription" runat="server" ControlName="chkShowDescription" />
        <asp:CheckBox runat="server" ID="chkShowDescription" />
    </div>
    <div class="dnnFormItem" id="maxDescriptionLengthRow">
        <dnn:Label ID="plMaxDescriptionLength" runat="server" ControlName="txtMaxDescriptionLength" />
        <asp:TextBox runat="server" ID="txtMaxDescriptionLength"></asp:TextBox>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plShowSnippet" runat="server" ControlName="chkShowSnippet" />
        <asp:CheckBox runat="server" ID="chkShowSnippet" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plShowFriendlyTitle" runat="server" ControlName="chkShowFriendlyTitle" />
        <asp:CheckBox runat="server" ID="chkShowFriendlyTitle" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plShowLastUpdated" runat="server" ControlName="chkShowLastUpdated" />
        <asp:CheckBox runat="server" ID="chkShowLastUpdated" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plShowSource" runat="server" ControlName="chkShowSource" />
        <asp:CheckBox runat="server" ID="chkShowSource" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="plShowTags" runat="server" ControlName="chkShowTags" />
        <asp:CheckBox runat="server" ID="chkShowTags" />
    </div>
</div>
<script type="text/javascript">
    (function($) {
        $(document.body).ready(function() {
            (function() {
                var $showDescription = $('#<%=chkShowDescription.ClientID%>');
                var updateState = function() {
                    var $maxDescriptionLengthRow = $('#maxDescriptionLengthRow');
                    $maxDescriptionLengthRow.toggle($showDescription.is(':checked'));
                }

                updateState();
                $showDescription.change(function() {
                    updateState();
                });
            })();
            
            (function() {
                var $filters = $('#<%=comboBoxFilters.ClientID%>');
                var updateState = function() {
                    var $scopeForRolesRow = $('#scopeForRolesRow');
                    var usersSelected = $filters.val().toLowerCase().split(',').filter(function(i) {
                            return i === "users";
                        }).length > 0;
                    $scopeForRolesRow.toggle(usersSelected);
                }

                updateState();
                $filters.change(function() {
                    updateState();
                });
            })();
        });
    })(jQuery);
</script>
