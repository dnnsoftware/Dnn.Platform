<%@ Control Language="C#" AutoEventWireup="false" CodeFile="SitemapSettings.ascx.cs"
    Inherits="DotNetNuke.Modules.Admin.Sitemap.SitemapSettings" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnSiteMap dnnClear" id="dnnSiteMap">
    <div class="dnnFormExpandContent">
        <a href="">
            <%=Localization.GetString("ExpandAll", Localization.SharedResourceFile)%></a></div>
    <fieldset>
        <div class="dnnFormItem dnnSiteMapUrl">
            <dnn:Label ID="lblSiteMap" runat="server" ControlName="txtSiteMap" />
            <asp:HyperLink ID="lnkSiteMapUrl" runat="server" Target="_blank" />
        </div>
        <div class="dnnFormItem">
        </div>
        <dnn:DnnGrid ID="grdProviders" runat="Server" Width="100%" AutoGenerateColumns="false"
            AllowSorting="true">
            <MasterTableView EditMode="InPlace">
                <Columns>
                    <dnn:DnnGridEditColumn ButtonType="ImageButton" EditImageUrl="~/icons/sigma/edit_16X16_standard.png"
                        CancelImageUrl="~/icons/sigma/cancel_16X16_standard.png" UpdateImageUrl="~/icons/sigma/save_16X16_standard.png" />
                    <dnn:DnnGridBoundColumn DataField="Name" HeaderText="Name" ReadOnly="true" />
                    <dnn:DnnGridBoundColumn DataField="Description" HeaderText="Description" ReadOnly="true" />
                    <dnn:DnnGridCheckBoxColumn DataField="OverridePriority" HeaderText="OverridePriority"
                        HeaderStyle-Width="0" />
                    <dnn:DnnGridBoundColumn DataField="Priority" HeaderText="Priority" HeaderStyle-Width="0" />
                    <dnn:DnnGridCheckBoxColumn DataField="Enabled" HeaderText="Enabled" HeaderStyle-Width="0" />
                </Columns>
            </MasterTableView>
        </dnn:DnnGrid>
    </fieldset>
    <h2 id="dnnSiteMap-SectionCoreSettings" class="dnnFormSectionHead">
        <a href="" class="dnnSectionExpanded">
            <%=LocalizeString("SectionCoreSettings")%></a></h2>
    <fieldset>
        <div class="dnnFormItem">
            <p>
                <asp:Label ID="lblSectionCoreSettingsHelp" runat="server" ResourceKey="SectionCoreSettingsLbl"
                    EnableViewState="False" />
            </p>
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblLevelPriority" runat="server" ControlName="chkLevelPriority" />
            <asp:CheckBox ID="chkLevelPriority" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblMinPagePriority" runat="server" ControlName="txtMinPagePriority"
                CssClass="dnnFormRequired" />
            <%--<dnn:DnnTextBox ID="txtMinPagePriority" runat="server" CssClass="dnnFormRequired" MaxLength="10"/>--%>
            <asp:TextBox ID="txtMinPagePriority" runat="server" MaxLength="10" />
            <%--<asp:CompareValidator ID="val1" runat="server" ControlToValidate="txtMinPagePriority"
                Display="Dynamic" CssClass="dnnFormMessage dnnFormError" resourcekey="valPriority"
                Operator="DataTypeCheck" Type="Double" />--%>
            <asp:RequiredFieldValidator ID="val2" runat="server" ControlToValidate="txtMinPagePriority"
                Display="Dynamic" CssClass="dnnFormMessage dnnFormError" resourcekey="valPriority" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblIncludeHidden" runat="server" ControlName="chkIncludeHidden" />
            <asp:CheckBox ID="chkIncludeHidden" runat="server" />
        </div>
    </fieldset>
    <h2 id="dnnSiteMap-SectionGeneralSettings" class="dnnFormSectionHead">
        <a href="" class="dnnSectionExpanded">
            <%=LocalizeString("SectionGeneralSettings")%></a></h2>
    <fieldset>
        <legend></legend>
        <div class="dnnFormItem">
            <p>
                <asp:Label ID="lblSectionGeneralSettingsHelp" runat="server" ResourceKey="SectionGeneralSettingsLbl"
                    EnableViewState="False" />
            </p>
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblExcludePriority" runat="server" ControlName="txtExcludePriority"
                CssClass="dnnFormRequired" />
            <%--<dnn:DnnTextBox ID="txtExcludePriority" runat="server" MaxLength="10" Text="0"  />--%>
            <asp:TextBox ID="txtExcludePriority" runat="server" MaxLength="10" Text="0" />
            <%--<asp:CompareValidator ID="CompareValidator1" runat="server" ControlToValidate="txtExcludePriority"
                Display="Dynamic" CssClass="dnnFormMessage dnnFormError" resourcekey="valPriority"
                Operator="DataTypeCheck" Type="Double" />--%>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtExcludePriority"
                Display="Dynamic" CssClass="dnnFormMessage dnnFormError" resourcekey="valPriority" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblCache" runat="server" ControlName="chkCache" />
            <%--<asp:DropDownList ID="cmbDaysToCache" runat="server">
				<Items>
					<asp:ListItem Value="0" ResourceKey="DisableCaching" runat="server" />
					<asp:ListItem Value="1" ResourceKey="1Day" runat="server" />
					<asp:ListItem Value="2" ResourceKey="2Days" runat="server"/>
					<asp:ListItem Value="3" ResourceKey="3Days" runat="server"/>
					<asp:ListItem Value="4" ResourceKey="4Days" runat="server"/>
					<asp:ListItem Value="5" ResourceKey="5Days" runat="server"/>
					<asp:ListItem Value="6" ResourceKey="6Days" runat="server"/>
					<asp:ListItem Value="7" ResourceKey="7Days" runat="server"/>
				</Items>
			</asp:DropDownList>--%>
            <dnn:DnnComboBox ID="cmbDaysToCache" runat="server" CssClass="dnnFixedSizeComboBox">
                <Items>
                    <dnn:DnnComboBoxItem Value="0" ResourceKey="DisableCaching" />
                    <dnn:DnnComboBoxItem Value="1" ResourceKey="1Day" />
                    <dnn:DnnComboBoxItem Value="2" ResourceKey="2Days" />
                    <dnn:DnnComboBoxItem Value="3" ResourceKey="3Days" />
                    <dnn:DnnComboBoxItem Value="4" ResourceKey="4Days" />
                    <dnn:DnnComboBoxItem Value="5" ResourceKey="5Days" />
                    <dnn:DnnComboBoxItem Value="6" ResourceKey="6Days" />
                    <dnn:DnnComboBoxItem Value="7" ResourceKey="7Days" />
                </Items>
            </dnn:DnnComboBox>
            <asp:LinkButton ID="lnkResetCache" runat="server" CssClass="dnnSecondaryAction" resourcekey="lnkResetCache"
                Text="ResetCache" />
        </div>
    </fieldset>
    <h2 id="dnnSiteMap-SectionSubmissionSettings" class="dnnFormSectionHead">
        <a href="" class="dnnSectionExpanded">
            <%=LocalizeString("SectionSubmissionSettings")%></a></h2>
    <fieldset>
        <div class="dnnFormItem">
            <p>
                <asp:Label ID="lblSectionSubmissionSettings" runat="server" ResourceKey="SectionSubmissionSettingsLbl"
                    EnableViewState="False" />
            </p>
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblSearchEngine" runat="server" ControlName="cboSearchEngine" />
            <%-- <asp:DropDownList ID="cboSearchEngine" runat="server" AutoPostBack="true">
				<Items>
					<asp:ListItem Text="Google" />
					<asp:ListItem Text="Bing" />
					<asp:ListItem Text="Yahoo!" />
				</Items>
			</asp:DropDownList>--%>
            <dnn:DnnComboBox ID="cboSearchEngine" runat="server" AutoPostBack="true" CssClass="dnnFixedSizeComboBox">
                <Items>
                    <dnn:DnnComboBoxItem Text="Google" />
                    <dnn:DnnComboBoxItem Text="Bing" />
                    <dnn:DnnComboBoxItem Text="Yahoo!" />
                </Items>
            </dnn:DnnComboBox>
            <asp:HyperLink ID="cmdSubmitSitemap" runat="server" Target="_blank" CssClass="dnnSecondaryAction"
                ResourceKey="cmdSubmitToSearch" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="lblVerification" runat="server" ControlName="txtVerification" />
            <%--dnn:DnnTextBox ID="txtVerification" runat="server" />--%>
            <asp:TextBox CssClass="dnnFixedSizeComboBox" runat="server" ID="txtVerification" />
            <asp:LinkButton CssClass="dnnSecondaryAction" ID="cmdVerification" resourcekey="cmdVerification"
                runat="server" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="lnkSaveAll" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdSaveAll" /></li>
        <li>
            <asp:LinkButton ID="lnkRefresh" runat="server" CssClass="dnnSecondaryAction " resourcekey="cmdRefresh" /></li>
    </ul>
</div>
<script language="javascript" type="text/javascript">
    /*globals jQuery, window, Sys */
    (function ($, Sys) {
        $(document).ready(function () {
            function setUpSitemapSettings() {
                $('#dnnSiteMap').dnnPanels()
				.find('.dnnFormExpandContent a').dnnExpandAll({
				    expandText: '<%=Localization.GetSafeJSString("ExpandAll", Localization.SharedResourceFile)%>',
				    collapseText: '<%=Localization.GetSafeJSString("CollapseAll", Localization.SharedResourceFile)%>',
				    targetArea: '#dnnSiteMap'
				});
            }

            function setupPrioritySpinner() {
                var updatePrioritySpinner = function (clientId) {
                    var txtCtrl = $('#' + clientId);
                    var defaultVal = txtCtrl.val();
                    txtCtrl.dnnSpinner({
                        type: 'list',
                        defaultVal: defaultVal,
                        typedata: { list: '1,0.9,0.8,0.7,0.6,0.5,0.4,0.3,0.2,0.1,0' }
                    });
                };

                updatePrioritySpinner('<%= txtMinPagePriority.ClientID %>');
                updatePrioritySpinner('<%= txtExcludePriority.ClientID %>');
            }


            setUpSitemapSettings();
            setupPrioritySpinner();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setUpSitemapSettings();
                setupPrioritySpinner();
            });
        });
    } (jQuery, window.Sys));

    $(function () {

    
    });

</script>
