<%@ Control Inherits="DesktopModules.Admin.Portals.SiteSettings" Language="C#"
    AutoEventWireup="false" EnableViewState="True" CodeFile="SiteSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Audit" Src="~/controls/ModuleAuditControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="FilePickerUploader" Src="~/controls/filepickeruploader.ascx" %>
<%@ Register TagPrefix="dnn" TagName="ProfileDefinitions" Src="~/DesktopModules/Admin/Security/ProfileDefinitions.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Register tagPrefix="dnnext" Namespace="DotNetNuke.ExtensionPoints" Assembly="DotNetNuke"%>

<dnn:DnnJsInclude id="DnnJsInclude1" runat="server" filepath="~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js" priority="103" />
<dnn:DnnCssInclude id="DnnCssInclude1" runat="server" filepath="~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css" />

<%-- Custom CodeMirror CSS Registration --%>
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/lib/codemirror.css" />

<%-- Custom CodeMirror JavaScript Registration --%>
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/lib/codemirror.js" Priority="104" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/css/css.js" Priority="105" />


<div class="dnnForm dnnSiteSettings dnnClear" id="dnnSiteSettings">
    <dnnext:EditPageTabExtensionControl runat="server"  ID="SiteSettingsTabExtensionControl" 
            Module="SiteSettings" Group="SiteSettingsTabExtensions"
            TabControlId="siteSettingsTabs" PanelControlId="siteSettingsPanes" />
        <ul id="siteSettingsTabs" runat="Server" class="dnnAdminTabNav dnnClear">
            <li><a href="#ssBasicSettings"><asp:Label id="basicSettingsLink" runat="server" resourcekey="BasicSettings" /></a></li>
            <li><a href="#ssAdvancedSettings"><asp:Label id="advancedSettingsLink" runat="server" resourcekey="AdvancedSettings" /></a></li>
            <li><a href="#ssUserAccountSettings"><asp:Label id="userSettingsLink" runat="server" resourcekey="UserAccountSettings" /></a></li>
            <li><a href="#ssStylesheetEditor"><asp:Label id="stylesheetLink" runat="server" resourcekey="StylesheetEditor" /></a></li>
        </ul>
        <div class="ssBasicSettings dnnClear" id="ssBasicSettings">
            <div class="dnnFormExpandContent">
                <a href=""><%=Localization.GetString("ExpandAll", Localization.SharedResourceFile)%></a>
            </div>     
            <div class="ssbsContent dnnClear">
                <h2 id="dnnSitePanel-SiteDetails" class="dnnFormSectionHead">
                    <a href="" class="dnnSectionExpanded"><%=LocalizeString("SiteDetails")%></a>
                </h2>
                <fieldset>
                    <div class="dnnFormItem">
                        <dnn:label id="plPortalName" runat="server" controlname="txtPortalName" CssClass="dnnFormRequired" />
                        <asp:TextBox ID="txtPortalName" runat="server" MaxLength="128" />
                        <asp:RequiredFieldValidator ID="valPortalName" CssClass="dnnFormMessage dnnFormError"
                            runat="server" resourcekey="valPortalName.ErrorMessage" Display="Dynamic" ControlToValidate="txtPortalName" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plDescription" runat="server" controlname="txtDescription" />
                        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plKeyWords" runat="server" controlname="txtKeyWords" />
                        <asp:TextBox ID="txtKeyWords" runat="server" TextMode="MultiLine" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plFooterText" runat="server" controlname="txtFooterText" />
                        <asp:TextBox ID="txtFooterText" runat="server" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plGUID" controlname="lblGUID" runat="server" />
                        <asp:Label ID="lblGUID" runat="server" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plHomeDirectory" runat="server" controlname="lblHomeDirectory" />
                        <asp:Label ID="lblHomeDirectory" runat="server" />
                    </div>
                </fieldset>
                <h2 id="dnnSitePanel-Marketing" class="dnnFormSectionHead">
                    <a href=""><%=LocalizeString("Marketing")%></a>
                </h2>
                <fieldset>
                    <div class="dnnFormItem">
                        <dnn:label id="plSearchEngine" runat="server" controlname="cboSearchEngine" />
                        <dnn:DnnComboBox ID="cboSearchEngine" runat="server" DataTextField="Key" DataValueField="Value" CssClass="dnnFixedSizeComboBox" />
                        <a href="#" id="submitToSearchEngine" class="dnnSecondaryAction"><%=LocalizeString("Submit")%></a>
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plSiteMap" runat="server" controlname="txtSiteMap" />
                        <asp:TextBox ID="txtSiteMap" runat="server" ReadOnly="true" CssClass="dnnFixedSizeComboBox" />
                        <a href="http://www.google.com/webmasters/sitemaps/" target="_blank" class="dnnSecondaryAction">
                            <%=LocalizeString("Submit")%></a>
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plVerification" runat="server" controlname="txtVerification" />
                        <asp:TextBox ID="txtVerification" runat="server" CssClass="dnnFixedSizeComboBox" />
                        <asp:LinkButton ID="cmdVerification" resourcekey="cmdVerification" runat="server"
                            CssClass="dnnSecondaryAction" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plBanners" runat="server" controlname="optBanners" />
                        <asp:RadioButtonList ID="optBanners" CssClass="dnnFormRadioButtons" runat="server"
                            EnableViewState="False" RepeatDirection="Horizontal">
                            <asp:ListItem Value="0" resourcekey="None">None</asp:ListItem>
                            <asp:ListItem Value="1" resourcekey="Site">Site</asp:ListItem>
                            <asp:ListItem Value="2" resourcekey="Host">Host</asp:ListItem>
                        </asp:RadioButtonList>
                        <asp:Label ID="lblBanners" runat="server" resourcekey="lblBanners" />
                    </div>
                </fieldset>
                <h2 id="dnnSitePanel-Appearance" class="dnnFormSectionHead">
                    <a href="">
                        <%=LocalizeString("Appearance")%></a></h2>
                <fieldset class="ssbsPortalAppearance">
                    <div class="dnnFormItem">
                        <dnn:label id="plLogo" runat="server" controlname="ctlLogo" />
                        <dnn:FilePickerUploader ID="ctlLogo" runat="server" Required="True" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plBackground" runat="server" controlname="cboBackground" />
                        <dnn:FilePickerUploader ID="ctlBackground" runat="server" Required="True" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plFavIcon" runat="server" controlname="ctlFavIcon" />
                        <dnn:FilePickerUploader ID="ctlFavIcon" runat="server" Required="True" FileFilter="ico" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plSkinWidgestEnabled" runat="server" controlname="chkSkinWidgestEnabled" />
                        <asp:CheckBox ID="chkSkinWidgestEnabled" runat="server" />
                    </div>
                    <div id="siteSkinSettings">
                        <div class="dnnFormItem">
                            <dnn:label id="plPortalSkin" controlname="portalSkinCombo" runat="server" />
                            <dnn:DnnSkinComboBox ID="portalSkinCombo" runat="Server" Width="300px" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plPortalContainer" controlname="portalContainerCombo" runat="server" />
                            <dnn:DnnSkinComboBox ID="portalContainerCombo" runat="Server" Width="300px" CssClass="dnnFixedSizeComboBox" />
                            <a href="#" class="dnnSecondaryAction"><%=LocalizeString("SkinPreview")%></a>
                        </div>
                    </div>
                    <div id="editSkinSettings">
                        <div class="dnnFormItem">
                            <dnn:label id="plAdminSkin" controlname="editSkinCombo" runat="server" />
                            <dnn:DnnSkinComboBox ID="editSkinCombo" runat="Server" Width="300px" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plAdminContainer" controlname="editContainerCombo" runat="server" />
                            <dnn:DnnSkinComboBox ID="editContainerCombo" runat="Server" Width="300px" CssClass="dnnFixedSizeComboBox" />
                            <a href="#" class="dnnSecondaryAction"><%=LocalizeString("EditSkinPreview")%></a>
                        </div>
                    </div>
                    <div id="iconSetSettings">
                        <div class="dnnFormItem">
                            <dnn:label id="plIconSet" controlname="iconSetCombo" runat="server" />
                            <asp:DropDownList ID="iconSetCombo" runat="server" Width="300px" CssClass="dnnFixedSizeComboBox" />
                        </div>
                    </div>
                </fieldset>
            </div>
        </div>
        <div class="ssAdvancedSettings dnnClear" id="ssAdvancedSettings">
            <div class="dnnFormExpandContent">
                <a href=""><%=Localization.GetString("ExpandAll", Localization.SharedResourceFile)%></a>
            </div>
            <div class="ssasContent">
                <h2 id="dnnSitePanel-Pages" class="dnnFormSectionHead">
                    <a href="" class="dnnSectionExpanded"><%=LocalizeString("Pages")%></a>
                </h2>
                <fieldset>
                    <div class="dnnFormItem">
                        <dnn:label id="plSplashTabId" runat="server" controlname="cboSplashTabId" ResourceKey="plSplashTabId" />
                        <dnn:DnnPageDropDownList ID="cboSplashTabId" runat="server" IncludeDisabledTabs="True" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plHomeTabId" runat="server" controlname="cboHomeTabId" />
                        <dnn:DnnPageDropDownList ID="cboHomeTabId" runat="server"  IncludeDisabledTabs="True"/>
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plLoginTabId" runat="server" controlname="cboLoginTabId" />
                        <dnn:DnnComboBox ID="cboLoginTabId" runat="server" DataTextField="IndentedTabName" DataValueField="TabId" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plRegisterTabId" runat="server" controlname="cboRegisterTabId" />
                        <dnn:DnnPageDropDownList ID="cboRegisterTabId" runat="server"  IncludeDisabledTabs="True"/>
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plUserTabId" runat="server" controlname="cboUserTabId" />
                        <dnn:DnnPageDropDownList ID="cboUserTabId" runat="server"  IncludeDisabledTabs="True"/>
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plSearchTabId" runat="server" controlname="cboSearchTabId" />
                        <dnn:DnnComboBox ID="cboSearchTabId" runat="server" DataTextField="IndentedTabName" DataValueField="TabId" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="pl404TabId" runat="server" controlname="cbo404TabId" />
                        <dnn:DnnPageDropDownList ID="cbo404TabId" runat="server" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="pl500TabId" runat="server" controlname="cbo500TabId" />
                        <dnn:DnnPageDropDownList ID="cbo500TabId" runat="server" />
                    </div>
                </fieldset>
                <h2 id="dnnSitePanel-SecuritySettings" class="dnnFormSectionHead">
                    <a href=""><%=LocalizeString("SecuritySettings")%></a>
                </h2>
                <fieldset class="ssasSecuritySettings">
                    <div class="dnnFormItem">
                        <dnn:label id="plAdministrator" runat="server" controlname="cboAdministratorId" />
                        <dnn:DnnComboBox ID="cboAdministratorId" runat="server" DataTextField="FullName" DataValueField="UserId" />
                    </div>
				    <div class="dnnFormItem">
                        <dnn:label id="plHideLoginControl" runat="server" controlname="enablePopUpsCheckBox" />
                        <asp:CheckBox ID="chkHideLoginControl" runat="server" />
                    </div>
                </fieldset>
                <h2 id="dnnSitePanel-Payment" class="dnnFormSectionHead">
                    <a href=""><%=LocalizeString("Payment")%></a>
                </h2>
                <fieldset>
                    <div class="dnnFormItem">
                        <dnn:label id="plCurrency" runat="server" controlname="currencyCombo" />
                       <dnn:DnnComboBox ID="currencyCombo" runat="server" DataTextField="text" DataValueField="value" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plProcessor" controlname="processorCombo" runat="server" />
                        <dnn:DnnComboBox ID="processorCombo" DataTextField="value" DataValueField="text" runat="server" CssClass="dnnFixedSizeComboBox" />
                        <asp:HyperLink ID="processorLink" Target="new" CssClass="dnnSecondaryAction" ResourceKey="ProcessorWebSite" runat="server" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plUserId" runat="server" controlname="txtUserId" />
                        <asp:TextBox ID="txtUserId" runat="server" MaxLength="50" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plPassword" runat="server" controlname="txtPassword" />
                        <asp:TextBox ID="txtPassword" runat="server" MaxLength="50" TextMode="Password" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plPayPaylReturnURL" runat="server" controlname="txtPayPaylReturnURL" />
                        <asp:TextBox ID="txtPayPalReturnURL" runat="server" MaxLength="255" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plPayPaylCancelURL" runat="server" controlname="txtPayPaylCancelURL" />
                        <asp:TextBox ID="txtPayPalCancelURL" runat="server" MaxLength="255" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plPayPalSandboxEnabled" runat="server" controlname="chkPayPalSandboxEnabled" />
                        <asp:CheckBox ID="chkPayPalSandboxEnabled" runat="server" AutoPostBack="True" />
                    </div>
                </fieldset>
                <h2 id="dnnSitePanel-Usability" class="dnnFormSectionHead">
                    <a href=""><%=LocalizeString("Usability")%></a>
                </h2>
                <fieldset class="ssasUsabilitySettings">
	                <div class="dnnFormItem">
                        <dnn:label id="plTimeZone" runat="server" controlname="cboTimeZone" />
                        <dnn:dnntimezonecombobox id="cboTimeZone" runat="server" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="enablePopUpsLabel" runat="server" controlname="enablePopUpsCheckBox" />
                        <asp:CheckBox ID="enablePopUpsCheckBox" runat="server" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="enableModuleEffectLabel" runat="server" controlname="enableModuleEffectCheckBox" />
                        <asp:CheckBox ID="enableModuleEffectCheckBox" runat="server" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plInlineEditor" runat="server" controlname="chkInlineEditor" />
                        <asp:CheckBox ID="chkInlineEditor" runat="server" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plHideSystemFolders" runat="server" controlname="chkHideSystemFolders" />
                        <asp:CheckBox ID="chkHideSystemFolders" runat="server" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plControlPanelMode" runat="server" controlname="optControlPanelMode" />
                        <asp:RadioButtonList ID="optControlPanelMode" runat="server" RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons">
                            <asp:ListItem Value="VIEW" resourcekey="ControlPanelModeView" />
                            <asp:ListItem Value="EDIT" resourcekey="ControlPanelModeEdit" />
                        </asp:RadioButtonList>
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plControlPanelVisibility" runat="server" controlname="optControlPanelVisibility" />
                        <asp:RadioButtonList ID="optControlPanelVisibility" runat="server" RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons">
                            <asp:ListItem Value="MIN" resourcekey="ControlPanelVisibilityMinimized" />
                            <asp:ListItem Value="MAX" resourcekey="ControlPanelVisibilityMaximized" />
                        </asp:RadioButtonList>
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="plControlPanelSecurity" runat="server" controlname="optControlPanelSecurity" />
                        <asp:RadioButtonList ID="optControlPanelSecurity" runat="server" RepeatLayout="Flow" CssClass="dnnFormRadioButtons">
                            <asp:ListItem Value="TAB" resourcekey="ControlPanelSecurityTab" />
                            <asp:ListItem Value="MODULE" resourcekey="ControlPanelSecurityModule" />
                        </asp:RadioButtonList>
                    </div>
                </fieldset>
                <dnnext:EditPagePanelExtensionControl runat="server" ID="SiteSettingAdvancedSettingExtensionControl" Module="SiteSettings" Group="SiteSettingsAdvancedSettingsExtensions"/>
                <div id="hostSections" runat="server">
                    <h2 id="dnnSitePanel-PortalAliases" class="dnnFormSectionHead">
                        <a href=""><%=LocalizeString("PortalAliases")%></a>
                    </h2>
                    <fieldset class="ssasPortalAlias">
                        <div class="dnnFormItem">
                            <dnn:label id="portalAliasModeButtonListLabel" runat="server" controlname="portalAliasModeButtonList" />
                            <asp:RadioButtonList ID="portalAliasModeButtonList" runat="server" EnableViewState="False" RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons">
                                <asp:ListItem Value="CANONICALURL" resourcekey="Canonical" />
                                <asp:ListItem Value="REDIRECT" resourcekey="Redirect" />
                                <asp:ListItem Value="NONE" resourcekey="None" />
                            </asp:RadioButtonList>
                        </div>
                        <div id="autoAddAlias" runat="server" class="dnnFormItem">
                            <dnn:label id="plAutoAddPortalAlias" runat="server" controlname="chkAutoAddPortalAlias" />
                            <asp:CheckBox ID="chkAutoAddPortalAlias" runat="server" resourcekey="enable" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="manageAliasesLabel" runat="server" controlname="portalAliases" />
                            <div class="dnnFormGroup">
                                <dnnext:UserControlExtensionControl runat="server" ID="portalAliasesExtensionPoint" runat="server" Module="SiteSettings" Name="PortalAliasesExtensionPoint"/>
                            </div>
                        </div>
                    </fieldset>
                    <h2 id="dnnSitePanel-SMTP" class="dnnFormSectionHead"><a href="#" class=""><%=LocalizeString("SMTP")%></a></h2>
                    <fieldset>
                        <div class="dnnFormItem">
                            <dnn:label id="plSMTPMode" controlname="rblSMTPmode" runat="server" />
                             <asp:RadioButtonList ID="rblSMTPmode" runat="server" AutoPostBack="true" RepeatDirection="Horizontal">
                                    <asp:ListItem Value="h">Host</asp:ListItem>
                                    <asp:ListItem Value="p">Portal</asp:ListItem>
                             </asp:RadioButtonList>
                        </div>
                        <div runat="server" id="SmtpSettings">
                            <div class="dnnFormItem">
                                <dnn:label id="plSMTPServer" controlname="txtSMTPServer" runat="server" />
                                <asp:TextBox ID="txtSMTPServer" runat="server" MaxLength="256" Width="225" />
                            </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plConnectionLimit" controlname="ConnectionLimit" runat="server" />
                            <asp:TextBox ID="txtConnectionLimit" runat="server" MaxLength="256" Width="225" />
                            <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" validationexpression="^\d*" ControlToValidate="txtConnectionLimit"/>
                            <asp:RangeValidator runat="server" id="rexNumber1" Type="Integer" controltovalidate="txtConnectionLimit" validationexpression="^\d*" MinimumValue="1" MaximumValue="2147483647" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" resourcekey="ConnectionLimitValidation" />
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
                        </div>
                        <ul class="dnnActions dnnClear">
                            <li>
                                <asp:LinkButton ID="cmdEmail" resourcekey="EmailTest" runat="server" CssClass="dnnPrimaryAction" /></li>
                        </ul>
                    </fieldset>
                    <h2 id="dnnSitePanel-SSLSettings" class="dnnFormSectionHead">
                        <a href=""><%=LocalizeString("SSLSettings")%></a>
                    </h2>
                    <fieldset>
                        <div class="dnnFormItem">
                            <dnn:label id="plSSLEnabled" runat="server" controlname="chkSSLEnabled" />
                            <asp:CheckBox ID="chkSSLEnabled" runat="server" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plSSLEnforced" runat="server" controlname="chkSSLEnforced" />
                            <asp:CheckBox ID="chkSSLEnforced" runat="server" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plSSLURL" runat="server" controlname="txtSSLURL" />
                            <asp:TextBox ID="txtSSLURL" runat="server" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plSTDURL" runat="server" controlname="txtSTDURL" />
                            <asp:TextBox ID="txtSTDURL" runat="server" />
                        </div>
                    </fieldset>
                    <h2 id="dnnSitePanel-MessagingSettings" class="dnnFormSectionHead">
                        <a href=""><%=LocalizeString("MessagingSettings")%></a>
                    </h2>
                    <fieldset>
                        <div class="dnnFormItem">
                            <dnn:label id="plMsgThrottlingInterval" runat="server" controlname="cboMsgThrottlingInterval" />
                            <dnn:DnnComboBox ID="cboMsgThrottlingInterval" runat="server" DataTextField="Key" DataValueField="Value" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plMsgRecipientLimit" runat="server" controlname="cboMsgRecipientLimit" />
                            <dnn:DnnComboBox ID="cboMsgRecipientLimit" runat="server" DataTextField="Key" DataValueField="Value" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plMsgProfanityFilters" runat="server" controlname="optMsgProfanityFilters" />
                            <asp:RadioButtonList ID="optMsgProfanityFilters" runat="server" RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons">
                                <asp:ListItem Value="YES" resourcekey="MsgProfanityFiltersYes" />
                                <asp:ListItem Value="NO" resourcekey="MsgProfanityFiltersNo" />
                            </asp:RadioButtonList>
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plMsgAllowAttachments" runat="server" controlname="optMsgAllowAttachments" />
                            <asp:RadioButtonList ID="optMsgAllowAttachments" runat="server" RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons">
                                <asp:ListItem Value="YES" resourcekey="MsgAllowAttachmentsYes" />
                                <asp:ListItem Value="NO" resourcekey="MsgAllowAttachmentsNo" />
                            </asp:RadioButtonList>
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plMsgSendEmail" runat="server" controlname="optMsgSendEmails" />
                            <asp:RadioButtonList ID="optMsgSendEmail" runat="server" RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons">
                                <asp:ListItem Value="YES" resourcekey="MsgSendEmailYes" />
                                <asp:ListItem Value="NO" resourcekey="MsgSendEmailsNo" />
                            </asp:RadioButtonList>
                        </div>
                    </fieldset>
                    <h2 id="dnnSitePanel-HostSettings" class="dnnFormSectionHead">
                        <a href=""><%=LocalizeString("HostSettings")%></a>
                    </h2>
                    <fieldset>
                        <div class="dnnFormItem">
                            <dnn:label id="plExpiryDate" runat="server" controlname="txtExpiryDate" />
                            <dnn:dnndatepicker id="datepickerExpiryDate" runat="server" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plHostFee" runat="server" controlname="txtHostFee" />
                            <asp:TextBox ID="txtHostFee" runat="server" MaxLength="10" />
                            <asp:CompareValidator ID="valHostFee" runat="server" ControlToValidate="txtHostFee"
                                CssClass="dnnFormMessage dnnFormError" Display="Dynamic" ResourceKey="valHostFee.Error"
                                Operator="DataTypeCheck" Type="Currency" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plHostSpace" runat="server" controlname="txtHostSpace" />
                            <asp:TextBox ID="txtHostSpace" runat="server" MaxLength="6" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plPageQuota" runat="server" controlname="txtPageQuota" />
                            <asp:TextBox ID="txtPageQuota" runat="server" MaxLength="6" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plUserQuota" runat="server" controlname="txtUserQuota" />
                            <asp:TextBox ID="txtUserQuota" runat="server" MaxLength="6" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plSiteLogHistory" runat="server" controlname="txtSiteLogHistory" />
                            <asp:TextBox ID="txtSiteLogHistory" runat="server" MaxLength="3" />
                        </div>
                        <div class="dnnFormItem ssasPremiumModule">
                            <dnn:label id="plDesktopModules" runat="server" controlname="ctlDesktopModules" />
                            <dnn:DnnComboBox CheckBoxes="True" id="ctlDesktopModules" runat="server" DataValueField="DesktopModuleID" DataTextField="FriendlyName" AutoPostBack="true" />
                        </div>
                    </fieldset>
                    <h2 id="dnnSitePanel-ClientResourceManagement" class="dnnFormSectionHead">
                        <a href="" class="dnnSectionExpanded"><%=LocalizeString("ClientResourceManagement")%></a>
                    </h2>
                    <fieldset>
                        <div class="dnnFormMessage dnnFormInfo crmHostSettingsSummary">
                            <asp:Literal runat="server" ID="CrmHostSettingsSummary"></asp:Literal>
                        </div>
                        <div class="dnnFormMessage dnnFormWarning">
                            <div>
                                <strong><%=LocalizeString("MinificationSettingsInfo.Title") %></strong>
                            </div>
                            <%= LocalizeString("MinificationSettingsInfo.Text")%>
                        </div>
                        <div runat="server" id="DebugEnabledRow" class="dnnFormMessage dnnFormWarning">
                            <div>
                                <strong><%=LocalizeString("DebugEnabled.Title") %></strong>
                            </div>
                            <%= LocalizeString("DebugEnabled.Text")%>
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plOverrideDefaultSettings" runat="server" controlname="chkOverrideDefaultSettings" />
                            <asp:CheckBox runat="server" ID="chkOverrideDefaultSettings" AutoPostBack="true" />
                        </div>
                        <div class="dnnFormItem" runat="server" id="CrmVersionRow">
                            <dnn:label runat="server" id="plCrmVersion" />
                            <asp:Label runat="server" ID="CrmVersionLabel" />
                            <asp:LinkButton runat="server" CssClass="dnnSecondaryAction" ID="IncrementCrmVersionButton" ResourceKey="IncrementCrmVersionButton" />
                        </div>
                        <div class="dnnFormItem" runat="server" id="EnableCompositeFilesRow">
                            <dnn:label id="plEnableCompositeFiles" runat="server" controlname="chkEnableCompositeFiles" />
                            <asp:CheckBox runat="server" ID="chkEnableCompositeFiles" AutoPostBack="True" />
                        </div>
                        <div class="dnnFormItem" runat="server" id="MinifyCssRow">
                            <dnn:label id="plMinifyCss" runat="server" controlname="chkMinifyCss" />
                            <asp:CheckBox runat="server" ID="chkMinifyCss" />
                        </div>
                        <div class="dnnFormItem" runat="server" id="MinifyJsRow">
                            <dnn:label id="plMinifyJs" runat="server" controlname="chkMinifyJs" />
                            <asp:CheckBox runat="server" ID="chkMinifyJs" />
                        </div>
                    </fieldset>
                    <h2 id="dnnSitePanel-PageHeaders" class="dnnFormSectionHead">
                        <a href="" class="dnnSectionExpanded"><%=LocalizeString("PageHeaders")%></a>
                    </h2>
                    <fieldset>
                        <div class="dnnFormItem">
                            <dnn:label id="plPageHeadText" runat="server" controlname="txtPageHeadText" />
                            <asp:TextBox ID="txtPageHeadText" runat="server" TextMode="MultiLine" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plInjectModuleHyperLink" runat="server" controlname="chkInjectModuleHyperLink" />
                            <asp:CheckBox runat="server" ID="chkInjectModuleHyperLink" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:label id="plAddCompatibleHttpHeader" runat="server" controlname="txtAddCompatibleHttpHeader" />
                            <asp:TextBox ID="txtAddCompatibleHttpHeader" runat="server" />                            
                        </div>
                    </fieldset>
                </div>
            </div>
        </div>
        <div class="ssUserAccountSettings dnnClear" id="ssUserAccountSettings">
            <div class="dnnFormExpandContent">
                <a href="">
                    <%=Localization.GetString("ExpandAll", Localization.SharedResourceFile)%></a></div>
            <h2 id="dnnSitePanel-Registration" class="dnnFormSectionHead">
                <a href="" class="dnnSectionExpanded">
                    <%=LocalizeString("Registration")%></a></h2>
            <fieldset>
                <div class="dnnFormItem">
                    <dnn:label id="plUserRegistration" runat="server" controlname="optUserRegistration" />
                    <asp:RadioButtonList ID="optUserRegistration" CssClass="dnnFormRadioButtons" runat="server"
                        EnableViewState="False" RepeatDirection="Horizontal">
                        <asp:ListItem Value="0" resourcekey="None" />
                        <asp:ListItem Value="1" resourcekey="Private" />
                        <asp:ListItem Value="2" resourcekey="Public" />
                        <asp:ListItem Value="3" resourcekey="Verified" />
                    </asp:RadioButtonList>
                </div>
			    <div class="dnnFormItem">
                    <dnn:label id="plEnableRegisterNotification" runat="server" controlname="chkEnableRegisterNotification" />
                    <asp:CheckBox runat="server" ID="chkEnableRegisterNotification" />
                </div>
                <div class="dnnFormItem">
                    <fieldset>
                        <dnn:dnnformeditor id="basicRegistrationSettings" runat="Server" formmode="Short">
                            <Items>
                                <dnn:DnnFormToggleButtonItem ID="DnnFormToggleButtonItem1" runat="server" DataField="Registration_UseAuthProviders" />
                                <dnn:DnnFormTextBoxItem ID="DnnFormTextBoxItem1" runat="server" DataField="Registration_ExcludeTerms" />
                                <dnn:DnnFormToggleButtonItem ID="DnnFormToggleButtonItem2" runat="server" DataField="Registration_UseProfanityFilter" />
                            </Items>
                        </dnn:dnnformeditor>
                    </fieldset>
                </div>
                <div class="dnnFormItem">
                    <fieldset>
                        <div class="dnnFormItem">
                            <dnn:label id="registrationFormTypeLabel" runat="server" controlname="registrationFormType" />
                            <asp:RadioButtonList ID="registrationFormType" CssClass="dnnFormRadioButtons" runat="server"
                                EnableViewState="False" RepeatDirection="Horizontal">
                                <asp:ListItem Value="0" resourcekey="Standard" />
                                <asp:ListItem Value="1" resourcekey="Custom" />
                            </asp:RadioButtonList>
                        </div>
                    </fieldset>
                    <div id="standardRegistration">
                        <dnn:dnnformeditor id="standardRegistrationSettings" runat="Server" formmode="Short">
                            <Items>
                                <dnn:DnnFormToggleButtonItem ID="DnnFormToggleButtonItem3" runat="server" DataField="Registration_UseEmailAsUserName" />
                            </Items>
                        </dnn:dnnformeditor>
                    </div>
                    <fieldset id="customRegistrationFieldSet">
                        <div class="dnnFormItem">
                            <dnn:label id="registrationFieldsLabel" runat="server" controlname="registrationFields" />
                            <asp:TextBox ID="registrationFields" runat="server" />
                        </div>
                    </fieldset>
                    <dnn:dnnformeditor id="validationRegistrationSettings" runat="Server" formmode="Short">
                        <Items>
                            <dnn:DnnFormToggleButtonItem ID="requireUniqueDisplayName" runat="server" DataField="Registration_RequireUniqueDisplayName" />
                            <dnn:DnnFormTextBoxItem ID="DnnFormTextBoxItem2" runat="server" DataField="Security_DisplayNameFormat" />
                            <dnn:DnnFormTextBoxItem ID="DnnFormTextBoxItem3" runat="server" DataField="Security_UserNameValidation" />
                            <dnn:DnnFormTextBoxItem ID="DnnFormTextBoxItem4" runat="server" DataField="Security_EmailValidation" />
                        </Items>
                    </dnn:dnnformeditor>
                    <fieldset id="passwordRegistrationFieldSet">
                        <dnn:dnnformeditor id="passwordRegistrationSettings" runat="Server" formmode="Short">
                            <Items>
                                <dnn:DnnFormToggleButtonItem ID="DnnFormToggleButtonItem4" runat="server" DataField="Registration_RandomPassword" />
                                <dnn:DnnFormToggleButtonItem ID="DnnFormToggleButtonItem5" runat="server" DataField="Registration_RequireConfirmPassword" />
                            </Items>
                        </dnn:dnnformeditor>
                    </fieldset>
                    <fieldset>
                        <dnn:dnnformeditor id="otherRegistrationSettings" runat="Server" formmode="Short">
                            <Items>
                                <dnn:DnnFormToggleButtonItem ID="DnnFormToggleButtonItem6" runat="server" DataField="Security_RequireValidProfile" />
                                <dnn:DnnFormToggleButtonItem ID="DnnFormToggleButtonItem7" runat="server" DataField="Security_CaptchaRegister" />
                            </Items>
                        </dnn:dnnformeditor>
                    </fieldset>
                    <div class="dnnFormItem">
                        <dnn:label id="RedirectAfterRegistrationLabel" runat="server" controlname="cboHomeTabId" ResourceKey="Redirect_AfterRegistration"/>
                        <dnn:DnnPageDropDownList ID="RedirectAfterRegistration" runat="server" />
                    </div>
                    <div id="uniqueEmailRow" class="dnnFormItem">
                        <dnn:label id="RequiresUniqueEmail" runat="server" controlname="RequiresUniqueEmailLabel" />
                        <asp:Label runat="server" ID="RequiresUniqueEmailLabel" />
                    </div>
                    <div class="dnnFormItem" runat="server" id="PasswordFormatRow">
                        <dnn:label id="PasswordFormatTitle" runat="server" controlname="PasswordFormatLabel" />
                        <asp:Label runat="server" ID="PasswordFormatLabel" />
                    </div>
                     <div class="dnnFormItem" runat="server" id="PasswordRetrievalEnabledRow">
                        <dnn:label id="PasswordRetrievalEnabledTitle" runat="server" controlname="PasswordRetrievalEnabledLabel" />
                        <asp:Label runat="server" ID="PasswordRetrievalEnabledLabel" />
                    </div>
                    <div class="dnnFormItem" runat="server" id="PasswordResetEnabledRow">
                        <dnn:label id="PasswordResetEnabledTitle" runat="server" controlname="PasswordResetEnabledLabel" />
                        <asp:Label runat="server" ID="PasswordResetEnabledLabel" />
                    </div>
                    <div class="dnnFormItem" runat="server" id="MinPasswordLengthRow">
                        <dnn:label id="MinPasswordLengthTitle" runat="server" controlname="MinPasswordLengthLabel" />
                        <asp:Label runat="server" ID="MinPasswordLengthLabel" />
                    </div>
                    <div class="dnnFormItem" runat="server" id="MinNonAlphanumericCharactersRow">
                        <dnn:label id="MinNonAlphanumericCharactersTitle" runat="server" controlname="MinNonAlphanumericCharactersLabel" />
                        <asp:Label runat="server" ID="MinNonAlphanumericCharactersLabel" />
                    </div>
                    <div class="dnnFormItem" runat="server" id="RequiresQuestionAndAnswerRow">
                        <dnn:label id="RequiresQuestionAndAnswerTitle" runat="server" controlname="RequiresQuestionAndAnswerLabel" />
                        <asp:Label runat="server" ID="RequiresQuestionAndAnswerLabel" />
                    </div>
                    <div class="dnnFormItem" runat="server" id="PasswordStrengthRegularExpressionRow">
                        <dnn:label id="PasswordStrengthRegularExpressionTitle" runat="server" controlname="PasswordStrengthRegularExpressionLabel" />
                        <asp:Label runat="server" ID="PasswordStrengthRegularExpressionLabel" />
                    </div>
                    <div class="dnnFormItem" runat="server" id="MaxInvalidPasswordAttemptsRow">
                        <dnn:label id="MaxInvalidPasswordAttemptsTitle" runat="server" controlname="MaxInvalidPasswordAttemptsLabel" />
                        <asp:Label runat="server" ID="MaxInvalidPasswordAttemptsLabel" />
                    </div>
                    <div class="dnnFormItem" runat="server" id="PasswordAttemptWindowRow">
                        <dnn:label id="PasswordAttemptWindowTitle" runat="server" controlname="PasswordAttemptWindowLabel" />
                        <asp:Label runat="server" ID="PasswordAttemptWindowLabel" />
                    </div>
                </div>
            </fieldset>
            <h2 id="dnnSitePanel-Login" class="dnnFormSectionHead">
                <a href="" class="dnnSectionExpanded"><%=LocalizeString("Login")%></a>
            </h2>
            <fieldset>
                <div class="dnnFormItem">
                    <fieldset>
                        <dnn:dnnformeditor id="loginSettings" runat="Server" formmode="Short">
                            <Items>
                                <dnn:DnnFormToggleButtonItem ID="DnnFormToggleButtonItem8" runat="server" DataField="Security_CaptchaLogin" />
                                <dnn:DnnFormToggleButtonItem ID="DnnFormToggleButtonItem9" runat="server" DataField="Security_RequireValidProfileAtLogin" />
                                <dnn:DnnFormToggleButtonItem ID="DnnFormToggleButtonItem10" runat="server" DataField="Security_CaptchaRetrivePassword" />
                                <dnn:DnnFormToggleButtonItem ID="DnnFormToggleButtonItem12" runat="server" DataField="Security_CaptchaChangePassword" />
                            </Items>
                        </dnn:dnnformeditor>
                    </fieldset>
                    <div class="dnnFormItem">
                        <dnn:label id="DefaultAuthProviderLabel" runat="server" controlname="authProviderCombo" ResourceKey="DefaultAuthProvider"/>
                        <dnn:DnnComboBox ID="authProviderCombo" runat="server" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="RedirectAfterLoginLabel" runat="server" controlname="cboHomeTabId" ResourceKey="Redirect_AfterLogin"/>
                        <dnn:DnnPageDropDownList ID="RedirectAfterLogin" runat="server" />
                    </div>
                    <div class="dnnFormItem">
                        <dnn:label id="RedirectAfterLogoutLabel" runat="server" controlname="cboHomeTabId" ResourceKey="Redirect_AfterLogout"/>
                        <dnn:DnnPageDropDownList ID="RedirectAfterLogout" runat="server" />
                    </div>
                </div>
            </fieldset>
            <h2 id="dnnSitePanel-Profile" class="dnnFormSectionHead">
                <a href="" class="dnnSectionExpanded"><%=LocalizeString("Profile")%></a>
            </h2>
            <fieldset>
                <div class="dnnFormItem">
                    <dnn:label id="redirectOldProfileUrlsLabel" runat="server" controlname="redirectOldProfileUrls" />
                    <asp:CheckBox runat="server" ID="redirectOldProfileUrls" resourcekey="enable" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="vanilyUrlPrefixLabel" runat="server" controlname="vanilyUrlPrefixTextBox" />
                    <div class="dnnFormGroup" id="VanityUrlPanel" runat="Server">
                        <asp:Label runat="server" ID="VanityUrlHeader" CssClass="NormalBold" resourcekey="VanityUrlPrefixHeader"/>
                        <div>
                            <asp:Label runat="server" ID="VanityUrlAlias" />
                            <asp:TextBox CssClass="dnnUserVanityUrl" runat="server" ID="vanilyUrlPrefixTextBox" />
                            <asp:Label runat="server" ID="VanityUrlExample"/>
                        </div>
                    </div>
                </div>
                <div class="dnnFormItem">
                    <fieldset>
                        <dnn:dnnformeditor id="profileSettings" runat="Server" formmode="Short">
                            <Items>
                                <dnn:DnnFormEnumItem id="userVisiblity" runat="server" DataField="Profile_DefaultVisibility" />
                                <dnn:DnnFormToggleButtonItem ID="DnnFormToggleButtonItem11" runat="server" DataField="Profile_DisplayVisibility" />
                            </Items>
                        </dnn:dnnformeditor>
                    </fieldset>
                    <dnn:profiledefinitions id="profileDefinitions" runat="server" />
                </div>
            </fieldset>
        </div>
        <div class="ssStylesheetEditor dnnClear" id="ssStylesheetEditor">
            <div class="ssseContent dnnClear">
                <fieldset>
                    <div class="editor">
                        <asp:TextBox ID="txtStyleSheet" runat="server" Rows="30" TextMode="MultiLine" Wrap="False" Columns="100" />
                    </div>
                    <ul class="dnnActions dnnClear">
                        <li><asp:LinkButton ID="cmdSave" CssClass="dnnPrimaryAction" runat="server" resourcekey="SaveStyleSheet" EnableViewState="False" /></li>
                        <li><asp:LinkButton ID="cmdRestore" CssClass="dnnSecondaryAction" runat="server" resourcekey="RestoreDefaultStyleSheet" EnableViewState="False" /></li>
                    </ul>
                </fieldset>
            </div>
        </div>
        <asp:PlaceHolder ID="siteSettingsPanes" runat="server" >
        </asp:PlaceHolder>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
        <li>
            <asp:LinkButton ID="cmdDelete" runat="server" CssClass="dnnSecondaryAction dnnDeleteSite"
                resourcekey="cmdDelete" CausesValidation="False" /></li>
        <li>
            <asp:HyperLink ID="cancelHyperLink" runat="server" CssClass="dnnSecondaryAction"
                resourcekey="cmdCancel" /></li>
        <li>
            <asp:HyperLink ID="uploadSkinLink" CssClass="dnnSecondaryAction" ResourceKey="SkinUpload"
                runat="server" /></li>
    </ul>
    <div class="dnnssStat dnnClear">
        <dnn:audit id="ctlAudit" runat="server" />
    </div>
</div>
<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
    function toggleRegistrationFormType(animation) {
        var registrationType = $('#<%= registrationFormType.ClientID %> input:checked').val(); /*0,1*/
        if (registrationType == "0") {
            animation ? $('#standardRegistration').slideDown() : $('#standardRegistration').show();
            animation ? $('#passwordRegistrationFieldSet').slideDown() : $('#passwordRegistrationFieldSet').show();
            animation ? $('#customRegistrationFieldSet').slideUp('fast') : $('#customRegistrationFieldSet').hide();
        }
        else {
            animation ? $('#standardRegistration').slideUp('fast') : $('#standardRegistration').hide();
            animation ? $('#passwordRegistrationFieldSet').slideUp('fast') : $('#passwordRegistrationFieldSet').hide();
            animation ? $('#customRegistrationFieldSet').slideDown() : $('#customRegistrationFieldSet').show();
        }
    }

    function toggleSmtpCredentials(animation) {
        var smtpVal = $('#<%= optSMTPAuthentication.ClientID %> input:checked').val(); /*0,1,2*/
        if (smtpVal == "1") {
            animation ? $('#SMTPUserNameRow,#SMTPPasswordRow').slideDown() : $('#SMTPUserNameRow,#SMTPPasswordRow').show();
        }
        else {
            animation ? $('#SMTPUserNameRow,#SMTPPasswordRow').slideUp() : $('#SMTPUserNameRow,#SMTPPasswordRow').hide();
        }
    }

    function toggleSection(id, isToggled) {
        $("div[id$='" + id + "']").toggle(isToggled);
    }

    function setupDnnSiteSettings() {
        setupCodeEditor();

        $('#dnnSiteSettings').dnnTabs().dnnPanels();
        $('#siteSkinSettings').dnnPreview({
            skinSelector: '<%= portalSkinCombo.ClientID %>',
            containerSelector: '<%= portalContainerCombo.ClientID %>',
            baseUrl: '//<%= this.PortalAlias.HTTPAlias %>',
            noSelectionMessage: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("PreviewNoSelectionMessage.Text")) %>',
            alertCloseText: '<%= Localization.GetSafeJSString("Close.Text", Localization.SharedResourceFile)%>',
            alertOkText: '<%= Localization.GetSafeJSString("Ok.Text", Localization.SharedResourceFile)%>',
            useComboBox: true
        });
         $('#editSkinSettings').dnnPreview({
            skinSelector: '<%= editSkinCombo.ClientID %>',
            containerSelector: '<%= editContainerCombo.ClientID %>',
            baseUrl: '//<%= this.PortalAlias.HTTPAlias %>',
            noSelectionMessage: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("PreviewNoSelectionMessage.Text")) %>',
            alertCloseText: '<%= Localization.GetSafeJSString("Close.Text", Localization.SharedResourceFile)%>',
            alertOkText: '<%= Localization.GetSafeJSString("Ok.Text", Localization.SharedResourceFile)%>',
            useComboBox: true
        });

        $('#ssBasicSettings .dnnFormExpandContent a').dnnExpandAll({ expandText: '<%=Localization.GetSafeJSString("ExpandAll", Localization.SharedResourceFile)%>', collapseText: '<%=Localization.GetSafeJSString("CollapseAll", Localization.SharedResourceFile)%>', targetArea: '#ssBasicSettings' });
        $('#ssAdvancedSettings .dnnFormExpandContent a').dnnExpandAll({ expandText: '<%=Localization.GetSafeJSString("ExpandAll", Localization.SharedResourceFile)%>', collapseText: '<%=Localization.GetSafeJSString("CollapseAll", Localization.SharedResourceFile)%>', targetArea: '#ssAdvancedSettings' });
		$('#ssUserAccountSettings .dnnFormExpandContent a').dnnExpandAll({ expandText: '<%=Localization.GetSafeJSString("ExpandAll", Localization.SharedResourceFile)%>', collapseText: '<%=Localization.GetSafeJSString("CollapseAll", Localization.SharedResourceFile)%>', targetArea: '#ssUserAccountSettings' });

        var yesText = '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>';
        var noText = '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>';
        var titleText = '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>';

        $('.dnnDeleteSite').dnnConfirm({
            text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("DeleteMessage")) %>',
            yesText: yesText,
            noText: noText,
            title: titleText
        });

        $('#<%= cmdRestore.ClientID %>').dnnConfirm({
            text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("RestoreCCSMessage")) %>',
            yesText: yesText,
            noText: noText,
            title: titleText
        });

        $('#<%= IncrementCrmVersionButton.ClientID %>').dnnConfirm({
            text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("IncrementCrmVersionConfirm")) %>',
            yesText: yesText,
            noText: noText,
            title: titleText
        });

        $('#submitToSearchEngine').click(function (e) {
            e.preventDefault();
            var searchEngine = $find('<%= cboSearchEngine.ClientID %>').get_value();

            if (searchEngine.indexOf("google") > 0) {
                searchEngine += "&dq=";
                var name = $("input[id$='txtPortalName']").val();
                if (name != "") {
                    searchEngine += encodeURI(name);
                }
                var description = $("textarea[id$='txtDescription']").val();
                if (description != "") {
                    searchEngine += encodeURI(" " + description);
                }
                var keyWords = $("textarea[id$='txtKeyWords']").val();
                if (keyWords != "") {
                    searchEngine += encodeURI(" " + keyWords);
                }
                searchEngine += "&submit=Add+URL";
            }
            window.open(searchEngine, 'new');
        });

        toggleRegistrationFormType(false);
        $('#<%= registrationFormType.ClientID %>').change(function () {
            toggleRegistrationFormType(true);
        });

        toggleSmtpCredentials(false);
        $('#<%= optSMTPAuthentication.ClientID %>').change(function () {
            toggleSmtpCredentials(true);
        });
        
        var serviceFramework = $.ServicesFramework(<%=ModuleContext.ModuleId %>);
        var baseServicepath = serviceFramework.getServiceRoot('InternalServices') + 'ProfileService/';
        $('input[id$="registrationFields"]').tokenInput(baseServicepath + "Search", {
            theme: "facebook",
            prePopulate: <% = CustomRegistrationFields %>
        });         
        
    }

    function setupCodeEditor() {
        var styleSheetEditor = CodeMirror.fromTextArea($("textarea[id$='txtStyleSheet']")[0], {
            lineNumbers: true,
            matchBrackets: true,
            lineWrapping: true,
            indentWithTabs: true,
            mode: 'text/css'
        });

        styleSheetEditor.on("blur", function (cm) {
            cm.save();
            return true;
        });
    }

    $(document).ready(function () {

        setupDnnSiteSettings();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setupDnnSiteSettings();
        });
    });

} (jQuery, window.Sys));
</script>
