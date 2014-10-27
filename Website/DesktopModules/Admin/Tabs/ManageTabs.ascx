<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Tabs.ManageTabs" CodeFile="ManageTabs.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="URL" Src="~/controls/DnnUrlControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Security.Permissions.Controls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" TagName="Audit" Src="~/controls/ModuleAuditControl.ascx" %>
<%@ Register tagPrefix="dnnext" Namespace="DotNetNuke.ExtensionPoints" Assembly="DotNetNuke"%>
<%@ Register TagPrefix="dnn" Src="~/DesktopModules/Admin/Languages/CLControl.ascx" TagName="CLControl" %>

<div class="dnnForm dnnPageSettings dnnClear" id="tabSettingsForm">
	<ul class="dnnAdminTabNav dnnClear" id="TabStrip">
		<li id="settingTab" runat="server"><a href="#dnnPageDetails"><%=LocalizeString("PageDetails")%></a></li>
        <li id="copyTab" runat="server"><a href="#dnnCopyPage"><%=LocalizeString("CopyPage")%></a></li>
		<li id="permissionsTab" runat="server"><a href="#dnnPermissions"><%=LocalizeString("Permissions")%></a></li>
		<li id="localizationTab" runat="server"><a href="#dnnLocalization"><%=LocalizeString("Localization")%></a></li>
		<li id="advancedTab" runat="server"><a href="#dnnAdvancedSettings"><%=LocalizeString("AdvancedSettings")%></a></li>
	</ul>
	<div id="dnnPageDetails" class="dnnPageDetails dnnClear">
	   <div class="psdContent dnnClear">
		   <fieldset>
				<div class="dnnFormItem">
					<dnn:Label ID="plTabName" runat="server" ResourceKey="TabName" Suffix=":" HelpKey="TabNameHelp" ControlName="txtTabName" CssClass="dnnFormRequired"  />
					<asp:TextBox ID="txtTabName" runat="server" MaxLength="200" />
					<asp:RequiredFieldValidator ID="valTabName" CssClass="dnnFormMessage dnnFormError" runat="server" resourcekey="valTabName.ErrorMessage" Display="Dynamic" ControlToValidate="txtTabName" />
				</div>    
				<div class="dnnFormItem">
					<dnn:Label ID="plTitle" runat="server" ResourceKey="Title" Suffix=":" HelpKey="TitleHelp" ControlName="txtTitle" />
					<asp:TextBox ID="txtTitle" runat="server" MaxLength="200" />
				</div>
				<asp:Panel cssClass="dnnFormItem" id="pageUrlPanel" runat="Server">
					<dnn:Label ID="urlLabel" runat="server" ControlName="urlTextBox" />
                    <div id="UrlContainer" runat="server" ViewStateMode="Disabled">
                        <asp:TextBox ID="PortalAliasCaption" runat="server" CssClass="um-alias-caption" ReadOnly="True" MaxLength="200" ViewStateMode="Disabled" />
                        <asp:TextBox ID="urlTextBox" runat="server" CssClass="um-page-url-textbox" MaxLength="200" />
                    </div>
				</asp:Panel>
				<asp:Panel cssClass="dnnFormItem" id="doNotRedirectPanel" runat="Server">
					<dnn:Label ID="doNotRedirectLabel" runat="server" ControlName="doNotRedirectCheckBox" />
					<asp:CheckBox ID="doNotRedirectCheckBox" runat="server" />
				</asp:Panel>    
				<div class="dnnFormItem">
					<dnn:Label ID="plDescription" runat="server" ResourceKey="Description" Suffix=":" HelpKey="DescriptionHelp" ControlName="txtDescription" />
					<asp:TextBox ID="txtDescription" runat="server" MaxLength="500" TextMode="MultiLine" Rows="2" />
				</div>    
				<div class="dnnFormItem">
					<dnn:Label ID="plKeywords" runat="server" ResourceKey="KeyWords" Suffix=":" HelpKey="KeyWordsHelp" ControlName="txtKeyWords" />
					<asp:TextBox ID="txtKeyWords" runat="server" MaxLength="500" TextMode="MultiLine" Rows="2" />
				</div>    
				<div class="dnnFormItem">
					<dnn:Label ID="plTags" runat="server" ControlName="termsSelector" />
					<dnn:TermsSelector ID="termsSelector" runat="server" IncludeTags="False" />
				</div>    
               <dnnext:UserControlExtensionControl  runat="server" ID="PageDetailsExtensionControl" Module="ManageTabs" Group="PageSettingsPageDetails"/>
				<div class="dnnFormItem">
					<dnn:Label ID="plParentTab" runat="server" ResourceKey="ParentTab" ControlName="cboParentTab" />
                    <dnn:DnnPageDropDownList ID="cboParentTab" runat="server" IncludeAllTabTypes="True" IncludeDisabledTabs="True" IncludeActiveTab="true"/>
				</div>    
				<div id="insertPositionRow" class="dnnFormItem" runat="server" ViewStateMode="Disabled">
                    <div>
					    <dnn:Label ID="plInsertPosition" runat="server" ResourceKey="InsertPosition" ControlName="cboPositionTab" />
					    <asp:RadioButtonList ID="rbInsertPosition" runat="server" CssClass="dnnFormRadioButtons" RepeatDirection="Horizontal" RepeatLayout="Flow" ViewStateMode="Disabled">
                            <Items>
                                <asp:ListItem Value="Before" Text="InsertBefore" resourcekey="InsertBefore"></asp:ListItem>
                                <asp:ListItem Value="After" Text="InsertAfter" resourcekey="InsertAfter" Selected="True"></asp:ListItem>
                                <asp:ListItem Value="AtEnd" Text="InsertAtEnd" resourcekey="InsertAtEnd"></asp:ListItem>
                            </Items>
                        </asp:RadioButtonList>
                    </div>
                    <div class="dnnFormItem">
                        <div class="dnnLabel"></div>
                        <dnn:DnnComboBox ID="cboPositionTab" CssClass="dnnPositionTab" runat="server" DataTextField="LocalizedTabName" DataValueField="TabId" ViewStateMode="Disabled" />
                    </div>
				</div>    
				<div id="templateRow1" class="dnnFormItem" runat="server" visible="false">
					<dnn:label id="plFolder" runat="server" controlname="cboFolders" />
                    <dnn:DnnFolderDropDownList ID="cboFolders" runat="server" AutoPostBack="True" />
				</div>    
				<div  id="templateRow2" class="dnnFormItem" runat="server" visible="false">
					<dnn:label id="plTemplate" runat="server" controlname="cboTemplate" />
                    <dnn:DnnComboBox id="cboTemplate" runat="server" ViewStateMode="Disabled" />
				</div>    
				<div class="dnnFormItem">
					<dnn:Label ID="plMenu" runat="server" ResourceKey="Menu" Suffix="?" HelpKey="MenuHelp" ControlName="chkMenu" />
					<asp:CheckBox ID="chkMenu" runat="server" />
				</div>    
			</fieldset>
	   </div>   
	</div>
    <div id="dnnCopyPage" class="dnnCopyPage dnnClear">
        <div class="pslContent dnnClear">
            <fieldset>
				<div id="copyPanel" runat="server">
					<div class="dnnFormItem">
						<dnn:Label ID="plCopyPage" runat="server" ResourceKey="CopyModules" Suffix=":" HelpKey="CopyModulesHelp" ControlName="cboCopyPage" />
                        <dnn:DnnPageDropDownList ID="cboCopyPage" runat="server" AutoPostBack="True" CausesValidation="False" IncludeAllTabTypes="True" IncludeDisabledTabs="True" />
					</div>    	
					<div id="modulesRow" runat="server" class="dnnFormItem">
						<dnn:Label ID="plModules" runat="server" ResourceKey="CopyContent" Suffix=":" HelpKey="CopyContentHelp" ControlName="grdModules" />
                        <asp:DataGrid ID="grdModules" runat="server" DataKeyField="ModuleID" AutoGenerateColumns="false" CssClass="dnnGrid dnnLeft">
							<headerstyle cssclass="dnnGridHeader" verticalalign="Top" />
							<itemstyle cssclass="dnnGridItem" horizontalalign="Left" />
							<alternatingitemstyle cssclass="dnnGridAltItem" />
							<edititemstyle cssclass="dnnFormInput" />
							<selecteditemstyle cssclass="dnnFormError" />
							<footerstyle cssclass="dnnGridFooter" />
							<pagerstyle cssclass="dnnGridPager" />
							<Columns>
								<asp:TemplateColumn>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkModule" runat="server" HeaderText="" Checked="True" /></ItemTemplate>
								</asp:TemplateColumn>
								<asp:TemplateColumn HeaderText="Title"  >
									<ItemTemplate>
                                        <asp:TextBox ID="txtCopyTitle" Width="90%" runat="server"  Text='<%# DataBinder.Eval(Container.DataItem,"ModuleTitle")%>' />
									</ItemTemplate>
									<ItemStyle Wrap="False"></ItemStyle>
								</asp:TemplateColumn>
								<asp:BoundColumn DataField="PaneName"  HeaderText="Pane" />
                                <asp:TemplateColumn ItemStyle-Width="200px"  HeaderText="Action" >
									<ItemTemplate>
                                       
										<asp:RadioButton ID="optNew" runat="server" GroupName="Copy" resourcekey="ModuleNew" />
										<asp:RadioButton ID="optCopy" runat="server" GroupName="Copy" resourcekey="ModuleCopy" Enabled='<%# DataBinder.Eval(Container.DataItem, "IsPortable") %>' Checked="True" />
										<asp:RadioButton ID="optReference" runat="server" GroupName="Copy" resourcekey="ModuleReference" Enabled='<%# Convert.ToInt32(DataBinder.Eval(Container.DataItem, "ModuleID")) != -1  %>' />
                                     
									</ItemTemplate>
									<ItemStyle Wrap="False"></ItemStyle>
								</asp:TemplateColumn>
							</Columns>
						</asp:DataGrid>
					</div>
				</div>
			</fieldset>
	   </div>   
	</div>
	<div id="dnnPermissions" class="dnnPermissions dnnClear">
		<div class="pspContent dnnClear">
			<fieldset>
				<div id="permissionRow" runat="server"><dnn:TabPermissionsGrid ID="dgPermissions" runat="server" /></div>    
				<div id="copyPermissionRow" runat="server">
					<div class="dnnFormItem"><dnn:Label ID="plCopyPerm" runat="server" ResourceKey="plCopyPerm" /></div>
					<asp:LinkButton ID="cmdCopyPerm" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCopyPerm" />
				</div>
			</fieldset>
		</div>
	</div>
	<div id="dnnLocalization" class="dnnLocalization dnnClear">
	   <div class="pslContent dnnClear">     
			<fieldset id="localizationPanel" runat="server">
				<legend></legend>
				<div id="cultureTypeRow" runat="server" visible="false" class="dnnFormItem" ViewStateMode="Disabled">
					<dnn:Label ID="cultureTypeLabel" runat="server" ControlName="cultureTypeList"></dnn:Label>
					<asp:RadioButtonList ID="cultureTypeList" runat="server" CssClass="dnnFormRadioButtons" RepeatDirection="Vertical">
						<asp:ListItem Value="Neutral" resourcekey="Neutral" Selected="True" />
						<asp:ListItem Value="Culture" resourcekey="Culture" />
						<asp:ListItem Value="Localized" resourcekey="Localized" />
					</asp:RadioButtonList>
				</div>
				<div id="cultureRow" runat="server" class="dnnFormItem" ViewStateMode="Disabled">
					<dnn:Label ID="cultureLabel" runat="server" ControlName="cultureName"></dnn:Label>
					<dnn:DnnLanguageLabel ID="cultureLanguageLabel" runat="server"  />&nbsp;&nbsp;&nbsp;
					<asp:Label ID="defaultCultureMessageLabel" runat="server" CssClass="dnnFormError" Text="**" />
					<asp:Label ID="defaultCultureMessage" runat="server" resourcekey="DefaultCulture" />
				</div>
                <dnn:CLControl ID="CLControl1" runat="server" />
                <ul class="dnnActions dnnClear">
                    <li><asp:LinkButton ID="cmdUpdateLocalization" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdateLocalization" OnClick="cmdUpdateLocalization_Click"></asp:LinkButton></li>
                    <li><asp:LinkButton runat="server" ID="MakeTranslatable" CssClass="dnnSecondaryAction" resourcekey="MakeTranslatable" OnClick="MakeTranslatable_Click"></asp:LinkButton></li>
                    <li><asp:LinkButton runat="server" ID="MakeNeutral" CssClass="dnnSecondaryAction" resourcekey="MakeNeutral" OnClick="MakeNeutral_Click"></asp:LinkButton></li>
                    <li><asp:LinkButton runat="server" ID="AddMissing" CssClass="dnnSecondaryAction" resourcekey="AddMissingLanguages" OnClick="AddMissing_Click"></asp:LinkButton></li>
                    <li><asp:LinkButton ID="readyForTranslationButton" runat="server" ResourceKey="ReadyForTranslation"  CssClass="dnnSecondaryAction"/></li>
                </ul>
                <div id="sendTranslationMessageConfirm" runat="server" class=""><asp:Label runat="server" id="sendTranslationMessageConfirmMessage"></asp:Label></div>
                <div id="sendTranslationMessageRow" runat="server" visible="false" class="dnnFormItem">                    
                    <asp:Label ID="TranslationCommentLabel" runat="server" resourcekey="TranslationComment" EnableViewState="False" />		            
			        <div class="dnnFormItem">
				        <asp:textbox id="txtTranslationComment" runat="server" textmode="multiline" />				            
			        </div>			            
                    <ul class="dnnActions dnnClear">		                    
		                <li><asp:LinkButton id="cmdSubmitTranslation" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdSubmit" /></li>
		                <li><asp:LinkButton id="cmdCancelTranslation" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
	                </ul>
                </div>
			</fieldset>   
		</div>
   </div>
	<div id="dnnAdvancedSettings" class="dnnAdvancedSettings dnnClear">
		<div class="dnnFormExpandContent"><a href=""><%=Localization.GetString("ExpandAll", Localization.SharedResourceFile)%></a></div>
		<div class="psasContent dnnClear">
			<h2 id="dnnPanel-TabsAppearance" class="dnnFormSectionHead"><a href="" class="dnnLabelExpanded"><%=LocalizeString("Appearance")%></a></h2>
			<fieldset>
				<legend></legend>
				<div class="dnnFormItem">
					<dnn:Label ID="plIcon" runat="server" ResourceKey="Icon" Suffix=":" HelpKey="IconHelp" ControlName="ctlIcon" />
					<div class="dnnLeft"><dnn:URL ID="ctlIcon" runat="server" ShowLog="False" /></div>
				</div>       
				<div class="dnnFormItem">
					<dnn:Label ID="plIconLarge" runat="server" ResourceKey="IconLarge" Suffix=":" HelpKey="IconLargeHelp" ControlName="ctlIconLarge" />
					<div class="dnnLeft"><dnn:URL ID="ctlIconLarge" runat="server" ShowLog="False" /></div>
				</div>       
				<div id="tabSkinSettings">
					<div class="dnnFormItem">
						<dnn:Label ID="plSkin" ControlName="pageSkinCombo" runat="server" />
                        <dnn:DnnSkinComboBox ID="pageSkinCombo" runat="Server" ViewStateMode="Disabled" />
					</div>
					<div class="dnnFormItem">
						<dnn:Label ID="plContainer" ControlName="pageContainerCombo" runat="server" />
                        <dnn:DnnSkinComboBox ID="pageContainerCombo" runat="Server" ViewStateMode="Disabled"  />
                    </div>
					<div class="dnnFormItem">
						<dnn:Label ID="plCustomStylesheet" ControlName="txtCustomStylesheet" runat="server" />
                        <asp:TextBox runat="server" ID="txtCustomStylesheet"></asp:TextBox>
                    </div>
                    <div class="dnnFormItem">
                        <div class="dnnLabel"></div>
						<a href="#" class="dnnSecondaryAction"><%=LocalizeString("SkinPreview")%></a>
					</div>
				</div>
				<div id="rowCopySkin" runat="server" class="dnnFormItem">
					<dnn:Label ID="plCopySkin" runat="server" ResourceKey="plCopySkin" />
					<asp:LinkButton ID="cmdCopySkin" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCopySkin" />
				</div>       
				<div class="dnnFormItem">
					<dnn:Label ID="plDisable" runat="server" ResourceKey="Disabled" Suffix=":" HelpKey="DisabledHelp" ControlName="chkDisableLink" />
					<asp:CheckBox ID="chkDisableLink" runat="server" />
				</div>       
				<div class="dnnFormItem">
					<dnn:Label ID="plRefreshInterval" runat="server" ResourceKey="RefreshInterval" Suffix=":" HelpKey="RefreshInterval.Help" ControlName="cboRefreshInterval" />
					<asp:TextBox ID="txtRefreshInterval" runat="server" />
				    <asp:RegularExpressionValidator ID="valRefreshInterval" runat="server" ControlToValidate="txtRefreshInterval" 
                        resourcekey="RefreshInterval.Invalid" ValidationExpression="^\d+$" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" />
				</div>       
				<div class="dnnFormItem">
					<dnn:Label ID="plPageHeadText" runat="server" ResourceKey="PageHeadText" Suffix=":" HelpKey="PageHeadText.Help" ControlName="txtPageHeadText" />
					<asp:TextBox ID="txtPageHeadText" runat="server" TextMode="MultiLine" Rows="4" Columns="50" />
				</div>   
                
			</fieldset>
			<h2 id="dnnPanel-TabsCacheSettings"  class="dnnFormSectionHead"><a href="" class=""><%=LocalizeString("CacheSettings")%></a></h2>
			<fieldset>
				<legend></legend>
				<div class="dnnFormItem">
					<dnn:Label ID="lblCacheProvider" runat="server" ControlName="cboCacheProvider" ResourceKey="CacheProvider" HelpKey="CacheProvider.Help"></dnn:Label>
                    <dnn:DnnComboBox ID="cboCacheProvider" runat="server" AutoPostBack="true" DataValueField="Key" DataTextField="Key" CausesValidation="False"  />
				</div>        
				<div id="CacheStatusRow" runat="server" visible="false" class="dnnFormItem">
					<dnn:Label ID="lblCacheStatus" runat="server" ResourceKey="CacheStatus" HelpKey="CacheStatus.Help"></dnn:Label>
					<asp:Label ID="lblCachedItemCount" runat="server" />
					<asp:LinkButton ID="cmdClearPageCache" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdClearPageCache" />&nbsp;|&nbsp;<asp:LinkButton ID="cmdClearAllPageCache" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdClearAllPageCache"/>
				</div>        
				<div id="CacheDurationRow" runat="server" visible="false" class="dnnFormItem">
					<dnn:Label ID="lblCacheDuration" runat="server" ControlName="txtCacheDuration" ResourceKey="CacheDuration" HelpKey="CacheDuration.Help"></dnn:Label>
					<asp:TextBox ID="txtCacheDuration" runat="server" />
					<asp:CompareValidator ID="valCacheTime" ControlToValidate="txtCacheDuration" Operator="DataTypeCheck" Type="Integer" Runat="server" Display="Dynamic" resourcekey="valCacheTime.ErrorMessage" CssClass="dnnFormMessage dnnFormError" />
					<div class="CacheDurationInfo dnnClear"><asp:Label ID="lblCacheDurationInfo" runat="server" ResourceKey="CacheDurationInfo.Text" CssClass="dnnFormMessage dnnFormWarning" Width="200px"/></div>
				</div>        
				<div id="CacheIncludeExcludeRow" runat="server" visible="false" class="dnnFormItem">
					<dnn:Label ID="lblCacheIncludeExclude" runat="server" ControlName="rblCacheIncludeExclude" ResourceKey="CacheIncludeExclude" HelpKey="CacheIncludeExclude.Help"></dnn:Label>
					<asp:RadioButtonList ID="rblCacheIncludeExclude" runat="server" AutoPostBack="true" CssClass="dnnFormRadioButtons" RepeatLayout="Flow">
						<asp:ListItem Text="Exclude" Value="0" />
						<asp:ListItem Text="Include" Value="1" />
					</asp:RadioButtonList>
				</div>        
				<div id="IncludeVaryByRow" runat="server" visible="false" class="dnnFormItem">
					<dnn:Label ID="lblIncludeVaryBy" runat="server" ControlName="txtIncludeVaryBy" ResourceKey="IncludeVaryBy" HelpKey="IncludeVaryBy.Help"></dnn:Label>
					<asp:TextBox ID="txtIncludeVaryBy" runat="server" />
				</div>        
				<div id="ExcludeVaryByRow" runat="server" visible="false" class="dnnFormItem">
					<dnn:Label ID="lblExcludeVaryBy" runat="server" ControlName="txtExcludeVaryBy" ResourceKey="ExcludeVaryBy" HelpKey="ExcludeVaryBy.Help"></dnn:Label>
					<asp:TextBox ID="txtExcludeVaryBy" runat="server" />
				</div>        
				<div id="MaxVaryByCountRow" runat="server" visible="false" class="dnnFormItem">
					<dnn:Label ID="lblMaxVaryByCount" runat="server" ControlName="txtMaxVaryByCount" ResourceKey="CacheMaxVaryByCount" HelpKey="CacheMaxVaryByCount.Help"></dnn:Label>
					<asp:TextBox ID="txtMaxVaryByCount" runat="server" />
					<asp:CompareValidator ID="valMaxVaryByCount" ControlToValidate="txtMaxVaryByCount" Operator="DataTypeCheck" Type="Integer" Runat="server" Display="Dynamic" resourcekey="valCacheTime.ErrorMessage" />
				</div>        
			</fieldset>
            <dnnext:EditPagePanelExtensionControl runat="server" ID="AdvancedSettingExtensionControl" Module="ManageTabs" Group="AdvancedSettings"/>
			<h2 id="dnnPanel-TabsOtherSettings" class="dnnFormSectionHead"><a href="" class=""><%=LocalizeString("OtherSettings")%></a></h2>
			<fieldset>
				<legend></legend>
				<div class="dnnFormItem">
					<dnn:Label ID="plSecure" runat="server" ControlName="chkSecure" />
					<asp:CheckBox ID="chkSecure" runat="server" />
				</div>
				<div class="dnnFormItem">
					<dnn:Label ID="plAllowIndex" runat="server" ControlName="chkAllowIndex" />
					<asp:CheckBox ID="chkAllowIndex" runat="server" Checked="True" />
				</div>          
				<div class="dnnFormItem">
					<dnn:Label ID="plPriority" runat="server" ControlName="txtPriority" />
					<asp:TextBox ID="txtPriority" runat="server" MaxLength="11" />
                    <asp:RequiredFieldValidator ID="valPriorityRequired" runat="server" ControlToValidate="txtPriority" 
                        resourcekey="valPriorityRequired.ErrorMessage" CssClass="dnnFormMessage dnnFormError" Display="Dynamic"  />
                    <asp:CompareValidator ID="valPriority" runat="server" ControlToValidate="txtPriority" Operator="DataTypeCheck" Type="Double" 
                        resourcekey="valPriority.ErrorMessage" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" />
				</div>        
				<div class="dnnFormItem">
					<dnn:Label ID="plStartDate" runat="server" ControlName="txtStartDate" />
					<dnn:dnndatetimepicker ID="startDatePicker" runat="server" ViewStateMode="Disabled" />&nbsp;
				</div>        
				<div class="dnnFormItem">
					<dnn:Label ID="plEndDate" runat="server" ControlName="txtEndDate" />
					<dnn:dnndatetimepicker ID="endDatePicker" runat="server" ViewStateMode="Disabled" />&nbsp;
                    <asp:CustomValidator ID="CustomValidator1" runat="server" ControlToValidate="endDatePicker" ClientValidationFunction="compareDate" 
						Display="Dynamic" resourcekey="valEndDate2.ErrorMessage" CompareControl="startDatePicker"
                        CssClass="dnnFormMessage dnnFormError"></asp:CustomValidator>    
				</div>
				<div class="dnnFormItem">
					<dnn:Label ID="plURL" runat="server" ResourceKey="Url" Suffix=":" HelpKey="UrlHelp" ControlName="ctlURL" />
					<div class="dnnLeft"><dnn:URL ID="ctlURL" runat="server" Width="300" ShowNewWindow="True"
                        ShowLog="False" ShowNone="True" ShowTrack="False" /></div>
				</div>        
				<div id="redirectRow" class="dnnFormItem" runat="server">
					<dnn:Label ID="plPermanentRedirect" runat="server" ControlName="chkPermanentRedirect" />
					<asp:CheckBox ID="chkPermanentRedirect" runat="server" />
				</div>             
			</fieldset>
		</div>
	</div>
	<ul class="dnnActions dnnClear">
		<li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction"  /></li>
		<li><asp:LinkButton id="cmdDelete" runat="server" CssClass="dnnSecondaryAction dnnDeletePage" resourcekey="cmdDelete" Causesvalidation="False" /></li>
		<li><asp:Hyperlink id="cancelHyperLink" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
	</ul>
</div>
<dnn:audit id="ctlAudit" runat="server"  ViewStateMode="Disabled" />
<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
    function setUpDnnManageTabs() {
        $('#tabSettingsForm').dnnTabs().dnnPanels();
        $('#dnnAdvancedSettings .dnnFormExpandContent a').dnnExpandAll({
            expandText: '<%=Localization.GetSafeJSString("ExpandAll", Localization.SharedResourceFile)%>',
            collapseText: '<%=Localization.GetSafeJSString("CollapseAll", Localization.SharedResourceFile)%>',
            targetArea: '#dnnAdvancedSettings'
        });
        $('#<%= cmdDelete.ClientID %>').dnnConfirm({
            text: '<%= Localization.GetSafeJSString("DeleteItem.Text", Localization.SharedResourceFile) %>',
            yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
            noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
            title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
        });
        $('#<%= cmdCopyPerm.ClientID %>').dnnConfirm({
            text: '<%= Localization.GetSafeJSString("CopyPermissionsToChildren.Text", Localization.SharedResourceFile) %>',
            yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
            noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
            title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
        });
        $('#<%= cmdCopySkin.ClientID %>').dnnConfirm({
            text: '<%= Localization.GetSafeJSString("CopyDesignToChildren.Confirm", LocalResourceFile) %>',
            yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
            noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
            title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
        });
        $('#tabSkinSettings').dnnPreview({
            skinSelector: '<%= pageSkinCombo.ClientID %>',
            containerSelector: '<%= pageContainerCombo.ClientID %>',
            baseUrl: '<%= DotNetNuke.Common.Globals.NavigateURL(this.TabId) %>',
            noSelectionMessage: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("PreviewNoSelectionMessage.Text")) %>',
            alertCloseText: '<%= Localization.GetSafeJSString("Close.Text", Localization.SharedResourceFile)%>',
            alertOkText: '<%= Localization.GetSafeJSString("Ok.Text", Localization.SharedResourceFile)%>',
            useComboBox: true
        });

	    $("ul.dnnAdminTabNav > li > a").click(function(e) {
	    	if ($(this).parent().attr("id").indexOf("propertiesTab") > -1) {
	    		$("ul.dnnMainActions, div.dnnModuleAuditControl").hide();
	    	} else {
	    		$("ul.dnnMainActions, div.dnnModuleAuditControl").show();
	    	}
	    });

	    $("#<%=rbInsertPosition.ClientID%> input[type=radio]").click(function (e) {
	    	var ddlPosition = $find("<%=cboPositionTab.ClientID%>");
		    var container = $("#<%=cboPositionTab.ClientID%>");
		    if ($(this).val() == "AtEnd") {
		    	ddlPosition.disable();
			    container.css("cssText", "display: none !important");
		    } else {
		    	ddlPosition.enable();
			    container.css("display", "");
		    }
	    });

	    if ($("ul.dnnAdminTabNav > li.ui-tabs-active").attr("id").indexOf("propertiesTab") > -1) {
	    	$("ul.dnnMainActions, div.dnnModuleAuditControl").hide();
	    }

        $('#<%=urlTextBox.ClientID%>').prefixInput({ prefix: "/" });
    }

    $(document).ready(function () {
        setUpDnnManageTabs();

        //set active tab
        var activeTab = '<%= ActiveDnnTab %>';
        if (activeTab) {
            $('#' + activeTab + ' a').click();
        }
        
        $('#<%= MakeNeutral.ClientID %>').dnnConfirm({ text: '<%= Localization.GetSafeJSString("MakeNeutral.Confirm", LocalResourceFile) %>', yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>', noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>', title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>' });


        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setUpDnnManageTabs();
        });
        
        window.compareDate = function (source, arg) {
            	var id = source.controltovalidate;
            	var compareId = source.getAttribute("CompareControl");
            	var time = $find(id).get_timeView().getTime();
            	var compareTime = $find(id.substr(0, id.lastIndexOf("_") + 1) + compareId).get_timeView().getTime();
            	arg.IsValid = compareTime == null || time > compareTime;
            };
    });
} (jQuery, window.Sys));
</script>