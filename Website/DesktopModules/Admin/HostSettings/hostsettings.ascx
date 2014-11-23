<%@ Control Inherits="DotNetNuke.Modules.Admin.Host.HostSettings" Language="C#" AutoEventWireup="false"
    CodeFile="HostSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="RequestFilters" Src="~/DesktopModules/Admin/HostSettings/RequestFilters.ascx" %>
<%@ Register TagPrefix="dnn" TagName="IPFilters" Src="~/DesktopModules/Admin/HostSettings/IPFilters.ascx" %>
<%@ Register TagPrefix="dnnext" Namespace="DotNetNuke.ExtensionPoints" Assembly="DotNetNuke" %>

<div class="dnnForm dnnHostSettings dnnClear" id="dnnHostSettings">
    <asp:ValidationSummary ID="valSummary" runat="server" CssClass="dnnFormMessage dnnFormValidationSummary"
        EnableClientScript="true" DisplayMode="BulletList" />
    <ul class="dnnAdminTabNav dnnClear">
        <li><a href="#basicSettings"><%=LocalizeString("BasicSettings")%></a></li>
        <li><a href="#advancedSettings"><%=LocalizeString("AdvancedSettings")%></a></li>
        <li><a href="#otherSettings"><%=LocalizeString("OtherSettings")%></a></li>
        <li><a href="#logSettings"><%=LocalizeString("LogSettings")%></a></li>
    </ul>
    <div id="basicSettings" class="ssBasicSettings dnnClear">
        <div class="dnnFormExpandContent">
            <a href=""><%=Localization.GetString("ExpandAll", Localization.SharedResourceFile)%></a>
        </div>
        <div class="ssasContent dnnClear">
            <h2 id="Panel-Configuration" class="dnnFormSectionHead">
                <a href="" class="">
                    <%=LocalizeString("Configuration")%></a></h2>
            <fieldset>
                <div class="dnnFormItem">
                    <dnn:label id="plProduct" controlname="lblProduct" runat="server" />
                    <asp:Label ID="lblProduct" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plVersion" controlname="lblVersion" runat="server" />
                    <asp:Label ID="lblVersion" runat="server" />
                </div>
                <div id="betaRow" class="dnnFormItem" runat="server">
                    <dnn:label id="plBetaNotice" controlname="chkBetaNotice" runat="server" />
                    <asp:CheckBox ID="chkBetaNotice" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plUpgrade" controlname="chkUpgrade" runat="server" />
                    <asp:CheckBox ID="chkUpgrade" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plAvailable" controlname="hypUpgrade" runat="server" />
                    <asp:HyperLink ID="hypUpgrade" Target="_new" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plDataProvider" controlname="lblDataProvider" runat="server" />
                    <asp:Label ID="lblDataProvider" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plFramework" controlname="lblFramework" runat="server" />
                    <asp:Label ID="lblFramework" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plIdentity" controlname="lblIdentity" runat="server" />
                    <asp:Label ID="lblIdentity" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plHostName" controlname="lblHostName" runat="server" />
                    <asp:Label ID="lblHostName" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plIPAddress" controlname="lblIPAddress" runat="server" />
                    <asp:Label ID="lblIPAddress" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plPermissions" controlname="lblPermissions" runat="server" />
                    <asp:Label ID="lblPermissions" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plApplicationPath" text="Relative Path:" controlname="lblApplicationPath"
                        runat="server" />
                    <asp:Label ID="lblApplicationPath" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plApplicationMapPath" text="Physical Path:" controlname="lblApplicationMapPath"
                        runat="server" />
                    <asp:Label ID="lblApplicationMapPath" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plServerTime" text="Server Time:" controlname="lblServerTime" runat="server" />
                    <asp:Label ID="lblServerTime" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plGUID" text="GUID:" controlname="lblGUID" runat="server" />
                    <asp:Label ID="lblGUID" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plWebFarm" text="Web Farm Enabled?" controlname="chkWebFarm" runat="server" />
                    <input id="chkWebFarm" type="checkbox" runat="server" disabled="disabled" />
                </div>
            </fieldset>
            <h2 id="Panel-HostDetails" class="dnnFormSectionHead">
                <a href="" class="">
                    <%=LocalizeString("HostDetails")%></a></h2>
            <fieldset>
                <div class="dnnFormItem">
                    <dnn:label id="plHostPortal" controlname="cboHostPortal" runat="server" />
                    <dnn:dnncombobox id="hostPortalsCombo" datatextfield="PortalName" datavaluefield="PortalID"
                        runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plHostTitle" controlname="txtHostTitle" runat="server" />
                    <asp:TextBox ID="txtHostTitle" runat="server" MaxLength="256" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plHostURL" controlname="txtHostURL" runat="server" />
                    <asp:TextBox ID="txtHostURL" runat="server" MaxLength="256" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plHostEmail" controlname="txtHostEmail" runat="server" />
                    <div>
                        <asp:TextBox ID="txtHostEmail" runat="server" MaxLength="256" />
                        <asp:RegularExpressionValidator ID="valHostEmail" CssClass="dnnFormMessage dnnFormError"
                            runat="server" ControlToValidate="txtHostEmail" Display="Dynamic" ResourceKey="HostEmail.Error" />
                    </div>
                </div>

                <div class="dnnFormItem">
                    <dnn:label id="plRememberMe" controlname="chkRemember" runat="server" />
                    <asp:CheckBox ID="chkRemember" runat="server" />
                </div>
            </fieldset>
            <h2 id="Panel-Appearance" class="dnnFormSectionHead">
                <a href="" class="">
                    <%=LocalizeString("Appearance")%></a></h2>
            <fieldset>
                <div class="dnnFormItem">
                    <dnn:label id="plCopyright" controlname="chkCopyright" runat="server" />
                    <asp:CheckBox ID="chkCopyright" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plUseCustomErrorMessages" controlname="chkUseCustomErrorMessages"
                        runat="server" />
                    <asp:CheckBox ID="chkUseCustomErrorMessages" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plUseCustomModuleCssClass" controlname="chkUseCustomModuleCssClass"
                        runat="server" />
                    <asp:CheckBox ID="chkUseCustomModuleCssClass" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plUpgradeForceSSL" controlname="chkUseCustomModuleCssClass"
                        runat="server" />
                    <asp:CheckBox ID="chkUpgradeForceSSL" runat="server" />
                </div>
                <div id="sslDomainRow" class="dnnFormItem">
                    <dnn:label id="plSSLDomain" controlname="chkUseCustomModuleCssClass"
                        runat="server" />
                    <asp:TextBox ID="txtSSLDomain" runat="server" MaxLength="256" />
                </div>
                <div id="hostSkinSettings">
                    <div class="dnnFormItem">
                        <dnn:label id="plHostSkin" controlname="hostSkinCombo" runat="server" />
                        <dnn:DnnSkinComboBox id="hostSkinCombo" runat="Server" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plHostContainer" controlname="hostContainerCombo" runat="server" />
                        <dnn:DnnSkinComboBox id="hostContainerCombo" runat="Server" cssclass="dnnFixedSizeComboBox" />
                        <a href="#" class="dnnSecondaryAction">
                            <%=LocalizeString("SkinPreview")%></a>
                    </div>
                </div>
                <div id="adminSkinSettings">
                    <div class="dnnFormItem">
                        <dnn:label id="plAdminSkin" controlname="editSkinCombo" runat="server" />
                        <dnn:DnnSkinComboBox id="editSkinCombo" runat="Server" />
                    </div>
                    <div class="dnnFormItem" id="adminContainerPreview">
                        <dnn:label id="plAdminContainer" controlname="editContainerCombo" runat="server" />
                        <dnn:DnnSkinComboBox id="editContainerCombo" runat="Server" cssclass="dnnFixedSizeComboBox" />
                        <a href="#" class="dnnSecondaryAction">
                            <%=LocalizeString("EditSkinPreview")%></a>
                    </div>
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plHostDefaultDocType" controlname="cboHostDefaultDocType" runat="server" />
                    <dnn:dnncombobox id="docTypeCombo" runat="server" datatextfield="Value" datavaluefield="Key" />
                </div>
            </fieldset>
            <h2 id="Panel-Payment" class="dnnFormSectionHead">
                <a href="" class="">
                    <%=LocalizeString("Payment")%></a></h2>
            <fieldset class="dnnhsPaymentSettings">
                <div class="dnnFormItem">
                    <dnn:label id="plProcessor" controlname="processorCombo" runat="server" />
                    <dnn:dnncombobox id="processorCombo" datatextfield="value" datavaluefield="text" cssclass="dnnFixedSizeComboBox"
                        runat="server" />
                    <asp:HyperLink ID="processorLink" Target="new" CssClass="dnnSecondaryAction" ResourceKey="ProcessorWebSite"
                        runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plUserId" controlname="txtUserId" runat="server" />
                    <asp:TextBox ID="txtUserId" runat="server" MaxLength="50" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plPassword" controlname="txtPassword" runat="server" />
                    <asp:TextBox ID="txtPassword" runat="server" MaxLength="50" TextMode="Password" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plHostFee" controlname="txtHostFee" runat="server" />
                    <asp:TextBox ID="txtHostFee" runat="server" MaxLength="10" />
                    <asp:CompareValidator ID="valHostFee" CssClass="dnnFormMessage dnnFormError" runat="server"
                        ControlToValidate="txtHostFee" Display="Dynamic" ResourceKey="valHostFee.Error"
                        Operator="DataTypeCheck" Type="Currency" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plHostCurrency" controlname="cboHostCurrency" runat="server" />
                    <dnn:dnncombobox id="currencyCombo" datavaluefield="value" datatextfield="text"
                        runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plHostSpace" controlname="txtHostSpace" runat="server" />
                    <asp:TextBox ID="txtHostSpace" runat="server" MaxLength="6" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plPageQuota" controlname="txtPageQuota" runat="server" />
                    <asp:TextBox ID="txtPageQuota" runat="server" MaxLength="6" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plUserQuota" controlname="txtUserQuota" runat="server" />
                    <asp:TextBox ID="txtUserQuota" runat="server" MaxLength="6" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plDemoPeriod" controlname="txtDemoPeriod" runat="server" />
                    <asp:TextBox ID="txtDemoPeriod" runat="server" MaxLength="3" />
                    <asp:Label ID="lblDemoPeriod" runat="server" resourcekey="Days" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plDemoSignup" controlname="chkDemoSignup" runat="server" />
                    <asp:CheckBox ID="chkDemoSignup" runat="server" />
                </div>
            </fieldset>
        </div>
    </div>
    <div id="advancedSettings" class="ssAdvancedSettings dnnClear">
        <div class="dnnFormExpandContent">
            <a href=""><%=Localization.GetString("ExpandAll", Localization.SharedResourceFile)%></a>
        </div>
        <div class="ssasContent dnnClear">
            <dnnext:EditPagePanelExtensionControl runat="server" ID="FriendlyUrlsExtensionControl" Module="HostSettings" Name="FriendlyUrlsExtensionPoint" />
            <h2 id="Panel-Proxy" class="dnnFormSectionHead"><a href="#" class=""><%=LocalizeString("Proxy")%></a></h2>
            <fieldset>
                <div class="dnnFormItem">
                    <dnn:label id="plProxyServer" controlname="txtProxyServer" runat="server" />
                    <asp:TextBox ID="txtProxyServer" runat="server" MaxLength="256" Width="300" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plProxyPort" controlname="txtProxyPort" runat="server" />
                    <asp:TextBox ID="txtProxyPort" runat="server" MaxLength="256" Width="300" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plProxyUsername" controlname="txtProxyUsername" runat="server" />
                    <asp:TextBox ID="txtProxyUsername" runat="server" MaxLength="256" Width="300" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plProxyPassword" controlname="txtProxyPassword" runat="server" />
                    <asp:TextBox ID="txtProxyPassword" runat="server" MaxLength="256" Width="300" TextMode="Password" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plWebRequestTimeout" controlname="txtWebRequestTimeout" runat="server" />
                    <asp:TextBox ID="txtWebRequestTimeout" runat="server" MaxLength="256" Width="300" />
                </div>
            </fieldset>
            <h2 id="Panel-SMTP" class="dnnFormSectionHead"><a href="#" class=""><%=LocalizeString("SMTP")%></a></h2>
            <fieldset class="dnnhsSMTPSettings">
                <div class="dnnFormItem">
                    <dnn:label id="plSMTPServer" controlname="txtSMTPServer" runat="server" />
                    <asp:TextBox ID="txtSMTPServer" runat="server" MaxLength="256" Width="225" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plConnectionLimit" controlname="ConnectionLimit" runat="server" />
                    <asp:TextBox ID="txtConnectionLimit" runat="server" MaxLength="256" Width="225" />
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" validationexpression="^\d*" ControlToValidate="txtConnectionLimit"/>
                    <asp:RangeValidator runat="server" id="rexNumber1" Type="Integer" controltovalidate="txtConnectionLimit" validationexpression="^\d*" MinimumValue="1" MaximumValue="2147483647" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" resourcekey="ConnectionLimitValidation"  />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plMaxIdleTime" controlname="MaxIdleTime" runat="server" />
                    <asp:TextBox ID="txtMaxIdleTime" runat="server" MaxLength="256" Width="225" />
                    <asp:RegularExpressionValidator runat="server" validationexpression="^\d*" ControlToValidate="txtMaxIdleTime"/>
                    <asp:RangeValidator runat="server" id="rexNumber2" Type="Integer" controltovalidate="txtMaxIdleTime" MinimumValue="0" MaximumValue="2147483647" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" resourcekey="MaxIdleTimeValidation" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plSMTPAuthentication" controlname="optSMTPAuthentication" runat="server" />
                    <asp:RadioButtonList ID="optSMTPAuthentication" CssClass="dnnHSRadioButtons" runat="server"
                        RepeatLayout="Flow">
                        <asp:ListItem Value="0" resourcekey="SMTPAnonymous" />
                        <asp:ListItem Value="1" resourcekey="SMTPBasic" />
                        <asp:ListItem Value="2" resourcekey="SMTPNTLM" />
                    </asp:RadioButtonList>
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plSMTPEnableSSL" controlname="chkSMTPEnableSSL" runat="server" />
                    <asp:CheckBox ID="chkSMTPEnableSSL" runat="server" />
                </div>
                <div id="SMTPUserNameRow" class="dnnFormItem">
                    <dnn:label id="plSMTPUsername" controlname="txtSMTPUsername" runat="server" />
                    <asp:TextBox ID="txtSMTPUsername" runat="server" MaxLength="256" Width="300" />
                </div>
                <div id="SMTPPasswordRow" class="dnnFormItem">
                    <dnn:label id="plSMTPPassword" controlname="txtSMTPPassword" runat="server" />
                    <asp:TextBox ID="txtSMTPPassword" runat="server" MaxLength="256" Width="300" TextMode="Password" />
                </div>
                <ul class="dnnActions dnnClear">
                    <li>
                        <asp:LinkButton ID="cmdEmail" resourcekey="EmailTest" runat="server" CssClass="dnnPrimaryAction" /></li>
                </ul>
				<div class="dnnFormItem">
                    <dnn:label id="plBatch" runat="server" controlname="txtBatch" />
                    <asp:TextBox ID="txtBatch" runat="server" MaxLength="6" />
                </div>
            </fieldset>
            <h2 id="Panel-Performance" class="dnnFormSectionHead"><a href="#" class=""><%=LocalizeString("Performance")%></a></h2>
            <fieldset class="dnnhsPerformanceSettings">
                <div class="dnnFormItem">
                    <dnn:label id="plPageState" runat="server" controlname="cboPageState" />
                    <asp:RadioButtonList ID="cboPageState" CssClass="dnnHSRadioButtons" runat="server"
                        RepeatLayout="Flow">
                        <asp:ListItem resourcekey="Page" Value="P" />
                        <asp:ListItem resourcekey="Memory" Value="M" />
                    </asp:RadioButtonList>
                    <div class="dnnFormItem psPageStateWarning dnnClear">
                        <asp:Label ID="plPsWarning" runat="server" CssClass="dnnFormMessage dnnFormWarning"
                            resourcekey="plPsWarning" />
                    </div>
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="lblModuleCacheProvider" runat="server" controlname="cboModuleCacheProvider"
                        resourcekey="ModuleCacheProvider" helpkey="ModuleCacheProvider.Help" />
                    <dnn:dnncombobox id="cboModuleCacheProvider" runat="server" datavaluefield="key"
                        datatextfield="filteredkey" />
                </div>
                <div id="PageCacheRow" class="dnnFormItem" runat="server">
                    <dnn:label id="lblPageCacheProvider" runat="server" controlname="cboPageCacheProvider"
                        resourcekey="PageCacheProvider" helpkey="PageCacheProvider.Help" />
                    <dnn:dnncombobox id="cboPageCacheProvider" runat="server" datavaluefield="key" datatextfield="filteredkey" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plPerformance" controlname="cboPerformance" runat="server" />
                    <dnn:dnncombobox id="cboPerformance" runat="server">
                        <Items>
                        <dnn:DnnComboBoxItem resourcekey="NoCaching" Value="0" />
                        <dnn:DnnComboBoxItem resourcekey="LightCaching" Value="1" />
                        <dnn:DnnComboBoxItem resourcekey="ModerateCaching" Value="3" />
                        <dnn:DnnComboBoxItem resourcekey="HeavyCaching" Value="6" />
                        </Items>
                    </dnn:dnncombobox>
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plCacheability" controlname="cboCacheability" runat="server" />
                    <dnn:dnncombobox id="cboCacheability" runat="server">
                        <Items>
                        <dnn:DnnComboBoxItem resourcekey="NoCache" Value="0" />
                        <dnn:DnnComboBoxItem resourcekey="Private" Value="1" />
                        <dnn:DnnComboBoxItem resourcekey="Public" Value="2" />
                        <dnn:DnnComboBoxItem resourcekey="Server" Value="3" />
                        <dnn:DnnComboBoxItem resourcekey="ServerAndNoCache" Value="4" />
                        <dnn:DnnComboBoxItem resourcekey="ServerAndPrivate" Value="5" />
                        </Items>
                    </dnn:dnncombobox>
                </div>
            </fieldset>
            <h2 id="Panel-JQuery" class="dnnFormSectionHead"><a href="#" class=""><%=LocalizeString("JQuery")%></a></h2>
            <fieldset>
                <div class="dnnFormItem">
                    <dnn:label id="plJQueryVersion" controlname="jQueryVersion" runat="server" />
                    <asp:Label ID="jQueryVersion" runat="server"></asp:Label>
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plJQueryUIVersion" controlname="jQueryUIVersion" runat="server" />
                    <asp:Label ID="jQueryUIVersion" runat="server"></asp:Label>
                </div>
            </fieldset>
            <h2 id="Panel-CdnSettings" class="dnnFormSectionHead"><a href="#" class=""><%=LocalizeString("CdnSettings")%></a></h2>
            <fieldset>
                <div class="dnnFormItem">
                    <dnn:label id="plMsAjaxCdn" controlname="chkMsAjaxCdn" runat="server" />
                    <asp:CheckBox ID="chkMsAjaxCdn" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plTelerikCdn" controlname="chkTelerikCdn" runat="server" />
                    <asp:CheckBox ID="chkTelerikCdn" runat="server" />
                </div>
                <div id="telerikCdnSettingsRow">
                    <div class="dnnFormItem">
                        <dnn:label id="plTelerikBasicUrl" controlname="chkTelerikBasicUrl" runat="server" />
                        <asp:TextBox ID="txtTelerikBasicUrl" runat="server" MaxLength="256" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plTelerikSecureUrl" controlname="chkTelerikSecureUrl" runat="server" />
                        <asp:TextBox ID="txtTelerikSecureUrl" runat="server" MaxLength="256" />
                    </div>
                </div>
                  <div class="dnnFormItem">
                    <dnn:label id="plEnableCDN" controlname="chkEnableCDN" runat="server" />
                    <asp:CheckBox ID="chkEnableCDN" runat="server" />
                </div>
            </fieldset>
            <h2 id="Panel-ClientResourceManagement" class="dnnFormSectionHead"><a href="#" class=""><%=LocalizeString("ClientResourceManagement")%></a></h2>
            <fieldset>
                <div class="dnnFormMessage dnnFormWarning">
                    <div><strong><%=LocalizeString("MinificationSettingsInfo.Title") %></strong></div>
                    <%= LocalizeString("MinificationSettingsInfo.Text")%>
                </div>
                <div runat="server" id="DebugEnabledRow" class="dnnFormMessage dnnFormWarning">
                    <div><strong><%=LocalizeString("DebugEnabled.Title") %></strong></div>
                    <%= LocalizeString("DebugEnabled.Text")%>
                </div>
                <div class="dnnFormItem">
                    <dnn:label runat="server" resourcekey="plCrmVersion" />
                    <asp:Label runat="server" ID="CrmVersion" />
                    <asp:LinkButton runat="server" CssClass="dnnSecondaryAction" ID="IncrementCrmVersionButton" ResourceKey="CrmIncrementCrmVersionButton" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label runat="server" resourcekey="plCrmEnableCompositeFiles" />
                    <asp:CheckBox runat="server" ID="chkCrmEnableCompositeFiles" AutoPostBack="true" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label runat="server" resourcekey="plCrmMinifyCss" />
                    <asp:CheckBox runat="server" ID="chkCrmMinifyCss" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label runat="server" resourcekey="plCrmMinifyJs" />
                    <asp:CheckBox runat="server" ID="chkCrmMinifyJs" />
                </div>
            </fieldset>
            <h2 id="Panel-MembershipManagement" class="dnnFormSectionHead"><a href="#" class=""><%=LocalizeString("MembershipManagement")%></a></h2>
            <fieldset>
                <div class="dnnFormItem">
                    <dnn:label id="plResetLinkValidity" controlname="txtResetLinkValidity" runat="server" />
                    <asp:TextBox ID="txtResetLinkValidity" runat="server" MaxLength="4" />
                    <asp:CompareValidator ID="valResetLink" runat="server" ValueToCompare="0" ControlToValidate="txtResetLinkValidity" CssClass="dnnFormMessage dnnFormError"
                        resourceKey="valResetLink.Error" Operator="GreaterThan" Type="Integer" Display="Dynamic"></asp:CompareValidator>
                    <asp:Label ID="lblResetLinkValidity" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plAdminResetLinkValidity" controlname="txtAdminResetLinkValidity" runat="server" />
                    <asp:TextBox ID="txtAdminResetLinkValidity" runat="server" MaxLength="8" />
                    <asp:CompareValidator ID="valAdminResetLink" runat="server" ValueToCompare="0" ControlToValidate="txtAdminResetLinkValidity" CssClass="dnnFormMessage dnnFormError"
                        resourceKey="valAdminResetLink.Error" Operator="GreaterThan" Type="Integer" Display="Dynamic"></asp:CompareValidator>
                    <asp:Label ID="lblAdminResetLinkValidity" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plEnablePasswordHistory" controlname="chkEnablePasswordHistory" runat="server" />
                    <asp:CheckBox ID="chkEnablePasswordHistory" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plNumberPasswords" controlname="txtNumberPasswords" runat="server" />
                    <asp:TextBox ID="txtNumberPasswords" runat="server" MaxLength="4" />
                    <asp:Label ID="lblNumberPasswords" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plEnableBannedList" controlname="chkBannedList" runat="server" />
                    <asp:CheckBox ID="chkBannedList" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plEnableStrengthMeter" controlname="chkStrengthMeter" runat="server" />
                    <asp:CheckBox ID="chkStrengthMeter" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plEnableIPChecking" controlname="chkIPChecking" runat="server" />
                    <asp:CheckBox ID="chkIPChecking" runat="server" />
                </div>
                <dnn:propertyeditorcontrol id="passwordSettings" runat="Server" valuedatafield="PropertyValue" namedatafield="Name" helpstyle-cssclass="dnnFormHelpContent dnnClear" sortmode="SortOrderAttribute" />
            </fieldset>
            <h2 id="Panel-IPFilters" class="dnnFormSectionHead"><a href="#" class=""><%=LocalizeString("IPFilters")%></a></h2>
            <fieldset class="asContentIPFilters">
                 <div class="dnnFormMessage dnnFormWarning">
                    <div><strong><%=LocalizeString("IPFiltersInfo.Title") %></strong></div>
                    <%= LocalizeString("IPFiltersInfo.Text")%>
                </div>
                <div runat="server" id="divFiltersDisabled" class="dnnFormMessage dnnFormWarning">
                    <div><strong><%=LocalizeString("IPFiltersDisabled.Title") %></strong></div>
                    <%= LocalizeString("IPFiltersDisabled.Text")%>
                </div>
                <div id="IPFiltersRow" class="dnnFormItem" runat="server">
                    <dnn:ipfilters id="IPFilters" runat="server" />
                </div>
            </fieldset>
            <h2 id="Panel-SearchIndex" class="dnnFormSectionHead">
                <a href="#" class=""><%=LocalizeString("SearchIndex")%></a>
            </h2>
            <fieldset class="asSearchIndex">
                <div class="dnnFormItem">
                    <dnn:label id="plIndexWordMinLength" runat="server" resourcekey="lblIndexWordMinLength" />
                    <asp:TextBox runat="server" ID="txtIndexWordMinLength"></asp:TextBox>
                    <asp:RequiredFieldValidator runat="server" ID="validatorIndexWordMinLengthRequired" ControlToValidate="txtIndexWordMinLength" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" resourceKey="valIndexWordMinLengthRequired.Error"  EnableClientScript="True"></asp:RequiredFieldValidator>
                    <asp:CompareValidator ID="validatorIndexWordMinLengthCompared" runat="server" ValueToCompare="0" ControlToValidate="txtIndexWordMinLength" CssClass="dnnFormMessage dnnFormError"
                        Operator="GreaterThan" Type="Integer" Display="Dynamic" EnableClientScript="True" resourceKey="valIndexWordMinLengthCompare.Error"></asp:CompareValidator>
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plIndexWordMaxLength" runat="server" resourcekey="lblIndexWordMaxLength" />
                    <asp:TextBox runat="server" ID="txtIndexWordMaxLength"></asp:TextBox>
                    <asp:RequiredFieldValidator runat="server" ID="validatorIndexWordMaxLengthRequired" ControlToValidate="txtIndexWordMaxLength" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" resourceKey="valIndexWordMaxLengthRequired.Error" EnableClientScript="True"></asp:RequiredFieldValidator>
                    <asp:CompareValidator ID="validatorIndexWordMaxLengthCompared" runat="server" ControlToCompare="txtIndexWordMinLength" ControlToValidate="txtIndexWordMaxLength" CssClass="dnnFormMessage dnnFormError"
                        Operator="GreaterThan" Type="Integer" Display="Dynamic" EnableClientScript="True" resourceKey="valIndexWordMaxLengthCompare.Error"></asp:CompareValidator>
                </div>
                <div class="dnnFormItem">
                    <dnn:Label ID="plCustomAnalyzer" runat="server" ResourceKey="lblCustomAnalyzer" ControlName="cbCustomAnalyzer" />
                    <dnn:dnncombobox ID="cbCustomAnalyzer" runat="server"></dnn:dnncombobox>
                </div>
                <div class="dnnTableHeader">
                    <div class="dnnFormItem">
                        <dnn:label id="plSearchIndexPath" runat="server" resourcekey="lblSearchIndexPath" />
                        <asp:Label runat="server" ID="lblSearchIndexPath"></asp:Label>
                    </div>
                    <div class="dnnFormItem" id="pnlSearchGetMoreButton" runat="server">
                        <div class="dnnLabel"></div>
                        <div class="dnnLeft">
                            <asp:LinkButton runat="server" ID="btnSearchGetMoreInfo" OnClick="GetSearchIndexStatistics"><%= LocalizeString("lblGetMoreInformation") %></asp:LinkButton>
                        </div>
                        <div class="dnnClear"></div>
                    </div>
                    <div id="pnlSearchStatistics" runat="server" visible="False">
                         <div class="dnnFormItem">
                            <dnn:label id="plSearchIndexDbSize" runat="server" resourcekey="lblSearchIndexDbSize" />
                            <asp:Label runat="server" ID="lblSearchIndexDbSize"></asp:Label>
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plSearchIndexTotalActiveDocuments" runat="server" resourcekey="lblSearchIndexActiveDocuments" />
                            <asp:Label runat="server" ID="lblSearchIndexTotalActiveDocuments"></asp:Label>
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plSearchIndexTotalDeletedDocuments" runat="server" resourcekey="lblSearchIndexDeletedDocuments" />
                            <asp:Label runat="server" ID="lblSearchIndexTotalDeletedDocuments"></asp:Label>
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plSearchIndexLastModifiedOn" runat="server" resourcekey="lblSearchIndexLastModifiedOn" />
                            <asp:Label runat="server" ID="lblSearchIndexLastModifedOn"></asp:Label>
                        </div>
                    </div>
                    <div class="dnnFormItem" style="margin-top: 12px;">
                        <div class="dnnFormMessage dnnFormWarning">
                            <%= Localization.GetString("MessageIndexWarning", LocalResourceFile) %>
                        </div>
                    </div>
                    <div class="dnnFormItem">
                        <asp:LinkButton runat="server" ID="btnCompactSearchIndex" CssClass="dnnSecondaryAction" resourcekey="btnCompactSearchIndex" OnClick="CompactSearchIndex" CausesValidation="False" />
                        <asp:LinkButton runat="server" ID="btnHostSearchReindex" CssClass="dnnSecondaryAction" resourcekey="btnHostSearchReindex" OnClick="HostSearchReindex" CausesValidation="False" />
                    </div>
                </div>                
            </fieldset>
            <dnnext:EditPagePanelExtensionControl runat="server" ID="FileCrawlerSettingsExtensionControl" Module="HostSettings" Name="FileCrawlerSettingsExtensionPoint" />            
        </div>
    </div>
    <div id="otherSettings" class="ssOtherSettings dnnClear">
        <div class="ssosContent dnnClear">
            <fieldset class="dnnhsOtherSettings">
                <div class="dnnFormItem">
                    <dnn:label id="plEnableRequestFilters" controlname="chkEnableRequestFilters" runat="server" />
                    <asp:CheckBox ID="chkEnableRequestFilters" runat="server" />
                </div>
                <div id="requestFiltersRow" class="dnnFormItem">
                    <dnn:requestfilters id="requestFilters" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plControlPanel" controlname="cboControlPanel" runat="server" />
                    <dnn:dnncombobox id="cboControlPanel" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plSiteLogStorage" controlname="optSiteLogStorage" runat="server" />
                    <asp:RadioButtonList ID="optSiteLogStorage" CssClass="dnnHSRadioButtons" runat="server"
                        RepeatLayout="Flow">
                        <asp:ListItem Value="D" resourcekey="Database" />
                        <asp:ListItem Value="F" resourcekey="FileSystem" />
                    </asp:RadioButtonList>
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plSiteLogBuffer" controlname="txtSiteLogBuffer" runat="server" />
                    <asp:TextBox ID="txtSiteLogBuffer" runat="server" MaxLength="4" />
                    <asp:Label ID="lblSiteLogBuffer" runat="server" resourcekey="Items" />
                    <asp:RangeValidator runat="server" id="valSiteLogBuffer" Type="Integer" controltovalidate="txtSiteLogBuffer" validationexpression="^\d*" MinimumValue="1" MaximumValue="999" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" resourcekey="SiteLogBufferValidation"  />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plSiteLogHistory" controlname="txtSiteLogHistory" runat="server" />
                    <asp:TextBox ID="txtSiteLogHistory" runat="server" MaxLength="3" />
                    <asp:Label ID="lblSiteLogHistory" runat="server" resourcekey="Days" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plUsersOnline" controlname="chkUsersOnline" runat="server" />
                    <asp:CheckBox ID="chkUsersOnline" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plUsersOnlineTime" controlname="txtUsersOnlineTime" runat="server" />
                    <asp:TextBox ID="txtUsersOnlineTime" runat="server" MaxLength="3" />
                    <asp:Label ID="lblUsersOnlineTime" runat="server" resourcekey="Minutes" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plAutoAccountUnlock" controlname="txtAutoAccountUnlock" runat="server" />
                    <asp:TextBox ID="txtAutoAccountUnlock" runat="server" MaxLength="3" />
                    <asp:Label ID="lblAutoAccountUnlock" runat="server" resourcekey="Minutes" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plFileExtensions" controlname="txtFileExtensions" runat="server" />
                    <asp:TextBox ID="txtFileExtensions" runat="server" MaxLength="256" TextMode="MultiLine"
                        Rows="3" />
                    <asp:RegularExpressionValidator ID="valFileExtensions" CssClass="dnnFormMessage dnnFormError"
                        runat="server" ControlToValidate="txtFileExtensions" EnableClientScript="true"
                        ValidationExpression="[A-Za-z0-9,_]*" resourceKey="valFileExtensions.Error" Display="Dynamic" />
                </div>              
                <div class="dnnFormItem">
                    <dnn:label id="plLogBuffer" controlname="chkLogBuffer" runat="server" />
                    <asp:CheckBox ID="chkLogBuffer" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plHelpUrl" controlname="txtHelpURL" runat="server" />
                    <asp:TextBox ID="txtHelpURL" runat="server" MaxLength="256" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plEnableHelp" controlname="chkEnableHelp" runat="server" />
                    <asp:CheckBox ID="chkEnableHelp" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plAutoSync" controlname="chkAutoSync" runat="server" />
                    <asp:CheckBox ID="chkAutoSync" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plEnableContentLocalization" controlname="chkEnableContentLocalization"
                        runat="server" />
                    <asp:CheckBox ID="chkEnableContentLocalization" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plDebugMode" controlname="chkDebugMode" runat="server" />
                    <asp:CheckBox ID="chkDebugMode" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plShowCriticalErrors" controlname="chkCriticalErrors" runat="server" />
                    <asp:CheckBox ID="chkCriticalErrors" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plMaxUploadSize" controlname="txtMaxUploadSize" runat="server" />
                    <asp:TextBox ID="txtMaxUploadSize" runat="server" />
                    <asp:Label ID="Label1" runat="server" resourcekey="Mb" />
                    <asp:RangeValidator runat="server" ID="rangeUploadSize" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" 
        ControlToValidate="txtMaxUploadSize"  MinimumValue="1"
        MaximumValue="99" Type="Integer" ></asp:RangeValidator>
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plAsyncTimeout" controlname="txtAsyncTimeout" runat="server" />
                    <asp:TextBox ID="txtAsyncTimeout" runat="server" MaxLength="4" />
                    <asp:Label runat="server" resourcekey="Seconds" />
                </div>
            </fieldset>
        </div>
    </div>
    <div id="logSettings" class="ssLogSettings dnnClear">
        <div class="dnnFormExpandContent">
            <h2 class="dnnFormSectionHead"><%=LocalizeString("LogLocations")%></h2>
            <fieldset>
                <div class="dnnFormItem">
                    <dnn:label id="plLogs" runat="server" controlname="ddlLogs" />
                    <dnn:dnncombobox id="ddlLogs" runat="server" autopostback="true" causesvalidation="False" />
                </div>
                <div class="dnnFormItem">
                    <asp:TextBox ID="txtLogContents" runat="server" TextMode="MultiLine" Rows="20" Columns="75" EnableViewState="True" Visible="False" Style="width: 70%; margin-left: 29%; max-width: none;" />
                </div>

            </fieldset>
            <div class="dnnFormItem dnnhsUpgradeLog">
                <div>
                    <dnn:label id="plLog" text="View Upgrade Log For Version:" controlname="cboUpgrade"
                        runat="server" />
                </div>
                <div>
                    <dnn:dnncombobox id="cboVersion" runat="server" cssclass="dnnFixedSizeComboBox" />
                    <dnn:commandbutton id="cmdUpgrade" resourcekey="cmdGo" runat="server" iconkey="View"
                        causesvalidation="false" />
                    <div class="dnnClear"></div>
                </div>
                <div>
                    <asp:Label ID="lblUpgrade" runat="server" />
                </div>
            </div>
        </div>
    </div>
    
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
        <li>
            <asp:HyperLink ID="uploadSkinLink" CssClass="dnnSecondaryAction" ResourceKey="SkinUpload" runat="server" /></li>
        <li>
            <asp:LinkButton ID="cmdCache" resourcekey="ClearCache" runat="server" CssClass="dnnSecondaryAction" CausesValidation="false" /></li>
        <li>
            <asp:LinkButton ID="cmdRestart" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdRestart" CausesValidation="False" /></li>
    </ul>
</div>
<script language="javascript" type="text/javascript">
    /*globals jQuery, window, Sys */
    (function ($, Sys) {
        function toggleSmtpCredentials(animation) {
            var smtpVal = $('#<%= optSMTPAuthentication.ClientID %> input:checked').val(); /*0,1,2*/
            if (smtpVal == "1") {
                animation ? $('#SMTPUserNameRow,#SMTPPasswordRow').slideDown() : $('#SMTPUserNameRow,#SMTPPasswordRow').show();
            }
            else {
                animation ? $('#SMTPUserNameRow,#SMTPPasswordRow').slideUp() : $('#SMTPUserNameRow,#SMTPPasswordRow').hide();
            }
        }

        function setUpDnnHostSettings() {
            $('#dnnHostSettings').dnnTabs().dnnPanels();
            $('#hostSkinSettings').dnnPreview({
                skinSelector: '<%= hostSkinCombo.ClientID %>',
                containerSelector: '<%= hostContainerCombo.ClientID %>',
                baseUrl: '//<%= this.PortalAlias.HTTPAlias %>',
                noSelectionMessage: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("PreviewNoSelectionMessage.Text")) %>',
                alertCloseText: '<%= Localization.GetSafeJSString("Close.Text", Localization.SharedResourceFile)%>',
                alertOkText: '<%= Localization.GetSafeJSString("Ok.Text", Localization.SharedResourceFile)%>',
                useComboBox: true
            });
            $('#adminSkinSettings').dnnPreview({
                skinSelector: '<%= editSkinCombo.ClientID %>',
                containerSelector: '<%= editContainerCombo.ClientID %>',
                baseUrl: '//<%= this.PortalAlias.HTTPAlias %>',
                noSelectionMessage: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("PreviewNoSelectionMessage.Text")) %>',
                alertCloseText: '<%= Localization.GetSafeJSString("Close.Text", Localization.SharedResourceFile)%>',
                alertOkText: '<%= Localization.GetSafeJSString("Ok.Text", Localization.SharedResourceFile)%>',
                useComboBox: true
            });

            $('#basicSettings .dnnFormExpandContent a').dnnExpandAll({ expandText: '<%=Localization.GetSafeJSString("ExpandAll", Localization.SharedResourceFile)%>', collapseText: '<%=Localization.GetSafeJSString("CollapseAll", Localization.SharedResourceFile)%>', targetArea: '#basicSettings' });
            $('#advancedSettings .dnnFormExpandContent a').dnnExpandAll({ expandText: '<%=Localization.GetSafeJSString("ExpandAll", Localization.SharedResourceFile)%>', collapseText: '<%=Localization.GetSafeJSString("CollapseAll", Localization.SharedResourceFile)%>', targetArea: '#advancedSettings' });

            toggleSmtpCredentials(false);
            $('#<%= optSMTPAuthentication.ClientID %>').change(function () {
                toggleSmtpCredentials(true);
            });

               
            toggleSection('requestFiltersRow', $("#<%=chkEnableRequestFilters.ClientID %>")[0].checked);
            $("#<%=chkEnableRequestFilters.ClientID %>").change(function (e) {
                toggleSection('requestFiltersRow', this.checked);
            });

            toggleSection('sslDomainRow', $("#<%=chkUpgradeForceSSL.ClientID %>")[0].checked);
            $("#<%=chkUpgradeForceSSL.ClientID %>").change(function (e) {
                toggleSection('sslDomainRow', this.checked);
            });

            $("#<%=chkTelerikCdn.ClientID %>").change(function (e) {
                toggleSection('telerikCdnSettingsRow', this.checked);
            });

            var yesText = '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
                noText = '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
                titleText = '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>';

            $('#<%= IncrementCrmVersionButton.ClientID %>').dnnConfirm({
                text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("IncrementCrmVersionConfirm")) %>',
                yesText: yesText,
                noText: noText,
                title: titleText
            });

            $('#<%= btnCompactSearchIndex.ClientID %>').dnnConfirm({
                text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("CompactIndexConfirmationMessage")) %>',
                yesText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("CompactIndexConfirmationYes")) %>',
                noText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("CompactIndexConfirmationCancel")) %>',
                title: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("CompactIndexConfirmationTitle")) %>'
            });

            $('#<%= btnHostSearchReindex.ClientID %>').dnnConfirm({
                text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ReIndexConfirmationMessage")) %>',
                yesText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ReIndexConfirmationYes")) %>',
                noText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ReIndexConfirmationCancel")) %>',
                title: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ReIndexConfirmationTitle")) %>'
            });

            // extensions
            var moduleId = <%= ModuleContext.ModuleId %>;
            if (dnn.searchAdmin && dnn.searchAdmin.extensions && dnn.searchAdmin.extensionsInitializeSettings) {
                for (var k in dnn.searchAdmin.extensions) {
                    var extensionSettings = dnn.searchAdmin.extensionsInitializeSettings[k];
                    var extensionObject = dnn.searchAdmin.extensions[k];
                    if (extensionSettings && typeof extensionObject.init == 'function') {
                        extensionSettings.moduleId = moduleId;
                        dnn.searchAdmin.extensions[k].init(extensionSettings);
                    }
                }
            }
        }

        $(document).ready(function () {
            setUpDnnHostSettings();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setUpDnnHostSettings();
            });
        });

        function toggleSection(id, isToggled) {
            $("div[id$='" + id + "']").toggle(isToggled);
        }
    }(jQuery, window.Sys));
</script>
