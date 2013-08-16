<%@ Page Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Services.Install.InstallWizard" CodeFile="InstallWizard.aspx.cs" %>
<%@ Import Namespace="DotNetNuke.UI.Utilities" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnncrm" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <asp:PlaceHolder runat="server" ID="ClientDependencyHeadCss"></asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="ClientDependencyHeadJs"></asp:PlaceHolder>
    <link rel="stylesheet" type="text/css" href="../Portals/_default/default.css?refresh" />    
    <link rel="stylesheet" type="text/css" href="Install.css?refresh" />    
     <!--[if IE]>
	<link rel="stylesheet" type="text/css" href="../Portals/_default/ie.css?refresh" />
    <![endif]-->
    <link rel="stylesheet" type="text/css" href="../Portals/_default/skins/_default/WebControlSkin/default/combobox.default.css?refresh" />
    <script type="text/javascript" src="../Resources/Shared/scripts/jquery/jquery.min.js"></script>
	<script type="text/javascript" src="../Resources/Shared/scripts/jquery/jquery-migrate.min.js"></script>
    <script type="text/javascript" src="../Resources/Shared/Scripts/jquery/jquery-ui.min.js"></script>
    <script type="text/javascript" src="../Resources/Shared/Scripts/jquery/jquery.hoverIntent.min.js"></script>
    <asp:placeholder id="SCRIPTS" runat="server"></asp:placeholder>
</head>  
<body>
    <asp:placeholder runat="server" id="ClientResourceIncludes" />
    <form id="form1" runat="server">
        <asp:ScriptManager ID="scManager" runat="server" EnablePageMethods="true"></asp:ScriptManager>       
        <asp:placeholder id="BodySCRIPTS" runat="server">    
              <script type="text/javascript" src="../Resources/Shared/Scripts/dnn.jquery.js"></script>                          
        </asp:placeholder>        
                          
    <br/>
    <img src="../images/Branding/DNN_logo.png" alt="DotNetNuke" />
            
    <div id="languageFlags" style="float: right;">       
        <asp:LinkButton  id="lang_en_US" class="flag" runat="server" value="en-US" title="English (United States)" OnClientClick="installWizard.changePageLocale('lang_en_US','en-US');" CausesValidation="false"><img src="../images/flags/en-US.gif" alt="en-US" class="flagimage"/></asp:LinkButton>
        <asp:LinkButton  id="lang_de_DE" class="flag" runat="server" value="de-DE" title="Deutsch (Deutschland)" OnClientClick="installWizard.changePageLocale('lang_de_DE','de-DE');" CausesValidation="false"><img src="../images/flags/de-DE.gif" alt="de-DE" class="flagimage"/></asp:LinkButton>
        <asp:LinkButton  id="lang_es_ES" class="flag" runat="server" value="es-ES" title="Espanol (Espana)" OnClientClick="installWizard.changePageLocale('lang_es_ES','es-ES');" CausesValidation="false"><img src="../images/flags/es-ES.gif" alt="es-ES" class="flagimage"/></asp:LinkButton> 
        <asp:LinkButton  id="lang_fr_FR" class="flag" runat="server" value="fr-FR" title="francais (France)" OnClientClick="installWizard.changePageLocale('lang_fr_FR','fr-FR');" CausesValidation="false"><img src="../images/flags/fr-FR.gif" alt="fr-FR" class="flagimage"/></asp:LinkButton>             
        <asp:LinkButton  id="lang_it_IT" class="flag" runat="server" value="it-IT" title="italiano (Italia)" OnClientClick="installWizard.changePageLocale('lang_it_IT','it-IT');" CausesValidation="false"><img src="../images/flags/it-IT.gif" alt="it-IT" class="flagimage"/></asp:LinkButton> 
        <asp:LinkButton  id="lang_nl_NL" class="flag" runat="server" value="nl-NL" title="Nederlands (Nederland)" OnClientClick="installWizard.changePageLocale('lang_nl_NL','nl-NL');" CausesValidation="false"><img src="../images/flags/nl-NL.gif" alt="nl-NL" class="flagimage"/></asp:LinkButton>
    </div>
         
    <div class="install">
        <h2 class="dnnForm dnnInstall dnnClear" >
            <asp:Label id="lblDotNetNukeInstalltion" runat="server" ResourceKey="InstallTitle" />
            <hr/>
        </h2>
        <asp:Label ID="lblError" runat="server" CssClass="dnnFormMessage dnnFormError" />
		<div id="permissionCheckMessage">
            <span class="NormalBold promptMessage permissionCheck"></span>
        </div>
        <div id="tabs" class="dnnWizardTab">
            <ul>
                <li id="accountInfo"><a href="#installAccountInfo">
                    <div class="dnnWizardStep">
                        <span class="dnnWizardStepNumber">1</span>
                        <span class="dnnWizardStepTitle"><%= LocalizeString("AccountInfo")%></span>
                        <span class="dnnWizardStepArrow"></span>
                    </div>                    
                    </a>
                </li>
                <li id="installInfo"><a href="#installInstallation">
                     <div class="dnnWizardStep">
                        <span class="dnnWizardStepNumber">2</span>
                        <span class="dnnWizardStepTitle"><%= LocalizeString("Installation")%></span>
                        <span class="dnnWizardStepArrow"></span>
                    </div>      
                    </a>                    
                 </li>
                <li id="webInfo"><a href="#installViewWebsite">
                     <div class="dnnWizardStep">
                        <span><img id="finishImage" src="../images/finishflag.png" alt="" /></span>
                        <span class="dnnWizardStepTitle"><%= LocalizeString("ViewWebsite")%></span>
                    </div>      
                </a>
                </li>
            </ul>
            <div class="installAccountInfo dnnClear" id="installAccountInfo">
                <asp:Label ID="lblAccountInfoIntro" runat="server" ResourceKey="AccountInfoIntro" />
				<asp:Label ID="lblIntroDetail" runat="Server" ResourceKey="IntroDetail" />
                <p style="display: block; margin: 10px 0 10px 0;">
                    <asp:Label ID="lblAccountInfoError" runat="server" CssClass="dnnFormMessage dnnFormError" />                 
                </p>
                <div id="adminInfo" runat="Server" visible="True" class="dnnForm">
                    <dnn:Label ID="lblAdminInfo" runat="server" CssClass="tabSubTitle" ResourceKey="AdminInfo" />
                    <asp:Label ID="lblAdminInfoError" runat="server" CssClass="NormalRed"/>
                    <div class="dnnFormItem">
                        <div class="dnnFormItem">
                            <dnn:Label ID="lblUsername" runat="server" ControlName="txtUsername" ResourceKey="UserName" CssClass="dnnFormRequired"/>
                            <asp:TextBox ID="txtUsername" runat="server"/>
                            <asp:RequiredFieldValidator ID="valUsername" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="UserName.Required" Display="Dynamic" ControlToValidate="txtUsername" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:Label ID="lblPassword" runat="server" ControlName="txtPassword" ResourceKey="Password" CssClass="dnnFormRequired"/>
                            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"/>
                            <asp:RequiredFieldValidator ID="valPassword" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="Password.Required" Display="Dynamic" ControlToValidate="txtPassword" />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:Label ID="lblConfirmPassword" runat="server" ControlName="txtConfirmPassword" ResourceKey="Confirm" CssClass="dnnFormRequired" />
                            <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" />
                            <asp:RequiredFieldValidator ID="valConfirmPassword" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="Confirm.Required" Display="Dynamic" ControlToValidate="txtConfirmPassword" />
                        </div>                      
                    </div>
                </div>
                <div id="websiteInfo" runat="Server" visible="True" class="dnnForm">                    
                    <dnn:Label ID="lblWebsiteInfo" runat="server" CssClass="tabSubTitle" ResourceKey="WebsiteInfo"/>
                    <asp:Label ID="lblWebsiteInfoError" runat="server" CssClass="dnnFormMessage dnnFormError" />
                    <div class="dnnFormItem">
                        <div class="dnnFormItem">
                            <dnn:Label ID="lblWebsiteName" runat="server" ControlName="txtWebsiteName" ResourceKey="WebsiteName" CssClass="dnnFormRequired" />
                            <asp:TextBox ID="txtWebsiteName" runat="server"/>
                            <asp:RequiredFieldValidator ID="valWebsiteName" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="WebsiteName.Required" Display="Dynamic" ControlToValidate="txtWebsiteName"  />
                        </div>
                        <div class="dnnFormItem">
                            <dnn:Label ID="lblTemplate" runat="server" ControlName="ddlTemplate" ResourceKey="WebsiteTemplate" />                      
                            <dnn:DnnComboBox ID="templateList" runat="server">
                                <Items>
                                    <dnn:DnnComboBoxItem ResourceKey="TemplateDefault" Value="Default Website.template"/>
                                    <dnn:DnnComboBoxItem ResourceKey="TemplateMobile" Value="Mobile Website.template"/>
                                    <dnn:DnnComboBoxItem ResourceKey="TemplateBlank" Value="Blank Website.template" />
                                </Items>
                            </dnn:DnnComboBox>
                        </div>
                        <div class="dnnFormItem">
                            <dnn:Label ID="lblLanguage" runat="server" ControlName="ddlLanguage" ResourceKey="Language" />
                            <dnn:DnnComboBox ID="languageList" runat="server" DataTextField="Text" DataValueField="Code">
                            </dnn:DnnComboBox>
                            <br/>
                            <asp:Label ID="lblLegacyLangaugePack" runat="server" CssClass="NormalBold" />      
                        </div>
                    </div>
                </div>
                <div id="databaseInfo" runat="Server" visible="True" class="dnnForm">
                    <dnn:Label id="lblDatabaseInfo" runat="server" CssClass="tabSubTitle" ResourceKey="DatabaseInfo" />
                    <div>
                        <asp:Label ID="lblDatabaseInfoMsg" runat="server" CssClass="databaseCheck promptMessage NormalBold" />      
                    </div>
                    <div class="dnnFormItem">
                        <div class="dnnFormItem">
                            <dnn:Label ID="lblDatabaseSetup" runat="server" ControlName="rblDatabaseSetup" ResourceKey="DatabaseSetup"/>
                            <asp:RadioButtonList ID="databaseSetupType" runat="server" RepeatDirection="Horizontal" >
                                <asp:ListItem Value="standard" ResourceKey="DatabaseStandard" Selected="True" />
                                <asp:ListItem Value="advanced" ResourceKey="DatabaseAdvanced" />
                            </asp:RadioButtonList>                            
                        </div>
                        <div id="StandardDatabaseMsg" class="dnnFormItem">
                            <dnn:Label ID="lblStandardDatabase" runat="server"/>
                            <asp:Label ID="lblStandardDatabaseMsg" runat="server" CssClass="dnnFormMessage" ResourceKey="StandardDatabaseMsg" />
                        </div>
                        <div id="advancedDatabase" class="dnnFormItem" style="display:none">
                            <div class="dnnFormItem">
                                <dnn:Label ID="lblDatabaseType" runat="server" ControlName="rblDatabaseType" ResourceKey="DatabaseType"/>
                                <asp:RadioButtonList ID="databaseType" runat="server" RepeatDirection="Horizontal">
                                    <asp:ListItem Value="express" ResourceKey="SqlTypeExpress" Selected="True" />
                                    <asp:ListItem Value="server" ResourceKey="SqlTypeServer"/>
                                </asp:RadioButtonList>
                            </div>
                            <div class="dnnFormItem">
                                <dnn:Label ID="lblDatabaseServerName" runat="server" ControlName="txtDatabaseServerName" ResourceKey="DatabaseServer" CssClass="dnnFormRequired"/>
                                <asp:TextBox ID="txtDatabaseServerName" runat="server"/>
                                <asp:RequiredFieldValidator ID="valDatabaseServerName" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="DatabaseServer.Required" Display="Dynamic" ControlToValidate="txtDatabaseServerName" />
                            </div>                        
                            <div class="dnnFormItem" id="databaseFilename">
                                <dnn:Label ID="lblDatabaseFilename" runat="server" ControlName="txtDatabaseFilename" ResourceKey="DatabaseFilename" CssClass="dnnFormRequired"/>
                                <asp:TextBox ID="txtDatabaseFilename" runat="server"/>
                                <asp:RequiredFieldValidator ID="valDatabaseFilename" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="DatabaseFilename.Required" Display="Dynamic" ControlToValidate="txtDatabaseFilename" />
                            </div>
                            <div class="dnnFormItem" id="databaseName" style="display: none">
                                <dnn:Label ID="lblDatabaseName" runat="server" ControlName="txtDatabaseName" ResourceKey="DatabaseName" CssClass="dnnFormRequired"/>
                                <asp:TextBox ID="txtDatabaseName" runat="server"/>
                                <asp:RequiredFieldValidator ID="valDatabaseName" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="DatabaseName.Required" Display="Dynamic" ControlToValidate="txtDatabaseName" />
                            </div>

                            <div class="dnnFormItem">
                                <dnn:Label ID="lblDatabaseObjectQualifier" runat="server" ControlName="txtDatabaseObjectQualifier" ResourceKey="DatabaseObjectQualifier"/>
                                <asp:TextBox ID="txtDatabaseObjectQualifier" runat="Server" MaxLength="20" />
                                <asp:RegularExpressionValidator ID="valQualifier" runat="server"
                                  resourcekey="InvalidQualifier.Text" 
                                  CssClass="dnnFormMessage dnnFormError"                                   
                                  ControlToValidate="txtDatabaseObjectQualifier"
                                  ValidationExpression="^[a-zA-Z][a-zA-Z0-9_]{0,19}$"
                                  Display="Dynamic"
                                ></asp:RegularExpressionValidator>
                            </div>
                            <div id="dbSecurityTypeRow" class="dnnFormItem">
                                <dnn:Label ID="lblDatabaseSecurity" runat="server" ControlName="rblDatabaseSecurity" ResourceKey="DatabaseSecurity"/>
                                <asp:RadioButtonList ID="databaseSecurityType" runat="server" RepeatDirection="Horizontal">
                                    <asp:ListItem Value="integrated" ResourceKey="DbSecurityIntegrated" Selected="True" />
                                    <asp:ListItem Value="userDefined" ResourceKey="DbSecurityUserDefined" />
                                </asp:RadioButtonList>
                            </div>
                            <div id="securityUserDefined" class="dnnFormItem" style="display:none; padding-top: 5px;">
                                <div class="dnnFormItem"> 
                                    <dnn:Label ID="lblDatabaseUsername" runat="server" ControlName="txtDatabaseUsername" ResourceKey="DatabaseUsername" CssClass="dnnFormRequired" />
                                    <asp:TextBox ID="txtDatabaseUsername" runat="server"/>
                                    <asp:RequiredFieldValidator ID="valDatabaseUsername" CssClass="dnnFormMessage dnnFormError dnnRequired" runat="server" resourcekey="DatabaseUsername.Required" Display="Dynamic" ControlToValidate="txtDatabaseUsername" />
                                </div>
                                <div class="dnnFormItem">
                                    <dnn:Label ID="lblDatabasePassword" runat="server" ControlName="txtDatabasePassword" ResourceKey="DatabasePassword" />
                                    <asp:TextBox ID="txtDatabasePassword" runat="server" TextMode="Password"/>
                                </div>
                            </div>
                            <div class="dnnFormItem">
                                <dnn:Label ID="lblDatabaseRunAs" runat="server" ControlName="txtDatabaseRunAs" ResourceKey="DatabaseRunAs"  />
                                <asp:CheckBox ID="databaseRunAs" runat="server" ResourceKey="DatabaseOwner" />
                            </div>
                        </div>                        
                        <div id="databaseError" class="dnnFormItem">
                            <dnn:Label ID="lblDatabaseConnectionError" runat="server" ResourceKey="DatabaseConnectionError"/>
                            <asp:Label ID="lblDatabaseError" runat="server" CssClass="NormalRed" />
                        </div>
                        
                    </div>
                </div> 
                <hr/>
                <ul class="dnnActions dnnClear">                    
                    <li><asp:LinkButton id="continueLink" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdContinue" /></li>
                </ul>                           
            </div>
            <div class="installInstallation dnnClear" id="installInstallation">
                <asp:Label ID="lblInstallationIntroInfo" runat="server" CssClass="installIntro" ResourceKey="InstallationIntroInfo" />
                <div id="installInstallationPanel" runat="Server" visible="True" class="dnnForm ui-tabs-panel">
                    <div class="dnnFormItem">
                        <div id="installation-progress">
                            <span id="timer"> </span>&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;<span id="percentage" style="height: auto; max-height: 200px; overflow: auto"> </span>
                            <div class="dnnProgressbar">
                                <div id="progressbar"></div>
                            </div>    
                            <div id="installation-buttons">
                                <a id="retry" href="javascript:void(0)" class="dnnPrimaryAction"><%= LocalizeString("Retry") %></a>
                                <a id="seeLogs" href="javascript:void(0)" class="dnnSecondaryAction"><%= LocalizeString("SeeLogs") %></a>
								<asp:LinkButton ID="visitSite" runat="server" resourcekey="VisitWebsite" CssClass="dnnPrimaryAction visitSiteLink" />                               
                            </div> 
							<hr />   
                            <div id="installation-log-container" class="dnnScroll">                                
                                <div id="installation-log" ></div>
                            </div>
                        </div>               
                        <div id="installation-steps">   
                            <p class="step-notstarted" id="FileAndFolderPermissionCheck"><span class="states-icons"></span><%= LocalizeString("FileAndFolderPermissionCheck")%></p>
                            <p class="step-notstarted" id="DatabaseInstallation"><span class="states-icons"></span><%= LocalizeString("DatabaseInstallation") %></p>                                    
                            <p class="step-notstarted" id="ExtensionsInstallation"><span class="states-icons"></span><%= LocalizeString("ExtensionsInstallation") %></p>
                            <p class="step-notstarted" id="WebsiteCreation"><span class="states-icons"></span><%= LocalizeString("WebsiteCreation") %></p>                                
                            <p class="step-notstarted" id="SuperUserCreation"><span class="states-icons"></span><%= LocalizeString("SuperUserCreation") %></p>
                            <p class="step-notstarted" id="LicenseActivation" runat="server"><span class="states-icons"></span><%= LocalizeString("LicenseActivation") %></p>                                                                                
                        </div>                        
                        <div id="banners">
                            <a id="bannerLink" runat="server" href="" target="">
                                <img id="bannerImage" runat="server" class="banner" src="../images/branding/DNN_logo.png" alt="" onerror="installWizard.bannerError(this);" />
                            </a>
                        </div>                       
                    </div>
                </div>
            </div>

            <div class="installViewWebsite" id="installViewWebsite">
                <div id="installViewWebsite" runat="Server" visible="True" class="dnnForm">
                </div>
            </div>
       </div>
    </div>
 
    <br/><br/><br/>

        <input id="ScrollTop" runat="server" name="ScrollTop" type="hidden" />
        <input type="hidden" id="__dnnVariable" runat="server" />
        <input id="PageLocale" runat="server" name="PageLanguage" type="hidden" value="" />
        <asp:Label ID="txtErrorMessage" runat="server" />
    </form> 

    <!-- InstallWizard() -->
    <script type="text/javascript">
        var installWizard = new InstallWizard();
        function InstallWizard() {
            this.installInfo = { };
            //****************************************************************************************
            // PAGE FUNCTIONS
            //****************************************************************************************
            this.changePageLocale = function (flagId, locale) {
                $('.flag').removeClass("selectedFlag");
                $('#' + flagId).addClass("selectedFlag");
                $("#PageLocale")[0].value = locale;
            };
            this.confirmPasswords = function () {
                if ($('#<%= txtPassword.ClientID %>')[0].value != $('#<%= txtConfirmPassword.ClientID %>')[0].value) {
                    $('#<%= lblAdminInfoError.ClientID %>').text('<%= Localization.GetSafeJSString(LocalizeString("PasswordMismatch"))%>');
                    $("#continueLink").addClass('dnnDisabledAction');
                } else {
                    $('#<%= lblAdminInfoError.ClientID %>').text('');
                    $("#continueLink").removeClass('dnnDisabledAction');
                }
                return ($('#<%= txtPassword.ClientID %>')[0].value == $('#<%= txtConfirmPassword.ClientID %>')[0].value);
            };
            this.validatePassword = function () {
                var validate = false;
                if ($('#<%= txtConfirmPassword.ClientID %>')[0].value !== '') {
                    validate = installWizard.confirmPasswords();
                } else {
                    validate = ($('#<%= txtPassword.ClientID %>')[0].value !== '');
                }
                if (validate) {
                    PageMethods.ValidatePassword($('#<%= txtPassword.ClientID %>')[0].value, function(result) {
                        if (result) {
                            $('#<%= lblAdminInfoError.ClientID %>').text('');
                            $("#continueLink").removeClass('dnnDisabledAction');
                        } else {
                            $('#<%= lblAdminInfoError.ClientID %>').text('<%= Localization.GetSafeJSString(LocalizeString("InputErrorInvalidPassword"))%>');
                            $("#continueLink").addClass('dnnDisabledAction');
                        }
                    });
                }
            };            
            this.validateInput = function () {
                $.each($(".dnnRequired"), function () {
                    var id = $(this).attr("id");
                    if ($("#" + id.replace("val", "txt")).val().length < 1) {
                        $(this).show();
                    }
                });
            };
            this.disableValidators = function () {
                if ((typeof (Page_Validators) != 'undefined') && (Page_Validators != null)) {
                    for (var i = 0; i < Page_Validators.length; i++) {
                        ValidatorEnable(Page_Validators[i], false);
                    }
                }
            };
            this.InitializeInstallScreen = function () {
                //Page Locale
                $('.flag').removeClass("selectedFlag");
                $('.flag[value=' + $("#PageLocale")[0].value + ']').addClass("selectedFlag");
                //Reset Validation
                $('.dnnRequired').hide();
                //Database Init
                this.toggleDatabaseType(true);
                this.toggleDatabaseSecurity(true);
                this.checkingDatabase();
	            this.checkingPermission();
                $('#<%= lblDatabaseInfoMsg.ClientID %>').text('');
                $("#databaseError").hide();
                $('#StandardDatabaseMsg').hide();
                $('#valQualifier').hide();
                PageMethods.VerifyDatabaseConnectionOnLoad(function (result) {
                	clearInterval(installWizard.loadingIntervalId);
                    $('#<%= lblDatabaseInfoMsg.ClientID %>').text('');
                    if (result) {
                    	$('#<%= lblDatabaseInfoMsg.ClientID %>').removeClass("promptMessage");
                        $('#advancedDatabase').slideUp('fast');
                        $('#advancedDatabase').hide();
                        $('#StandardDatabaseMsg').hide();
                        installWizard.ValidDatabaseConnection = true;
                    } else {
                    	$('#<%= lblDatabaseInfoMsg.ClientID %>').removeClass("promptMessage");  
                        $('#StandardDatabaseMsg').show();
                        $('#advancedDatabase').slideDown();
                        $('#advancedDatabase').show();
                        $('#databaseSetupType').find("input[value='advanced']").attr("checked", "checked");
                        $('#databaseSetupType').find("input[value='standard']").attr("disabled", "true");
                        $('#databaseSetupType input:radio:checked').trigger('click');
                    }
                });
            	PageMethods.ValidatePermissions(function (result) {
            		clearInterval(installWizard.checkPermissionIntervalId);
            		$(".permissionCheck").html('');
	            	if (!result.Item1) {
	            		$(".permissionCheck").html(result.Item2).removeClass("promptMessage").dialog({
	            			dialogClass: "dnnFormPopup",
	            			modal: true,
	            			width: 950,
	            			height: 550,
	            			position: "center",
	            			autoOpen: true,
	            			resizable: false,
	            			closeOnEscape: false,
	            			draggable: false
	            		});
	            	} else {
	            		$(".permissionCheck").parent().hide();
	            	}
	            });
            };
            this.toggleAdvancedDatabase = function(animation) {
                var databaseType = $('#<%= databaseSetupType.ClientID %> input:checked').val(); /*standard, advanced*/
                if (databaseType == "advanced") {
                    animation ? $('#advancedDatabase').slideDown() : $('#advancedDatabase').show();
                } else {
                    animation ? $('#advancedDatabase').slideUp('fast') : $('#advancedDatabase').hide();
                }
            };
            this.toggleDatabaseType = function() {
                var databaseType = $('#<%= databaseType.ClientID %> input:checked').val(); /*express, server*/
                if (databaseType == "express") {
                    $('#databaseFilename').show();
                    $('#databaseName').hide();
                    $("#dbSecurityTypeRow").hide();
                    $("#databaseRunAs").attr("checked", true).attr("disabled", true);
                } else {
                    $('#databaseName').show();
                    $('#databaseFilename').hide();
                    $("#dbSecurityTypeRow").show();
                    $("#databaseRunAs").attr("disabled", false);
                }
            };
            this.toggleDatabaseSecurity = function(animation) {
                var databaseSecurityType = $('#<%= databaseSecurityType.ClientID %> input:checked').val(); /*integrated, userDefined*/
                if (databaseSecurityType == "userDefined") {
                    animation ? $('#securityUserDefined').slideDown() : $('#securityUserDefined').show();
                } else {
                    animation ? $('#securityUserDefined').slideUp('fast') : $('#securityUserDefined').hide();
                }
            };
            this.loadingIntervalId = null;
            this.checkingDatabase = function () {
                clearInterval(installWizard.loadingIntervalId);
                $('#<%= lblDatabaseInfoMsg.ClientID %>').removeClass("promptMessageError");
            	$('#<%= lblDatabaseInfoMsg.ClientID %>').addClass("promptMessage");
                var i = 0;
                $(".databaseCheck").html('<%= Localization.GetSafeJSString(LocalizeString("TestingDatabase"))%>');
            	var origtext = $(".databaseCheck").html();
                var text = origtext;
                installWizard.loadingIntervalId = setInterval(function () {
                	$(".databaseCheck").html(text + Array((++i % 6) + 1).join("."));
                    if (i === 6) text = origtext;
                }, 500);
            };
        	this.checkingPermission = function () {
        		clearInterval(installWizard.checkPermissionIntervalId);
        		$('.permissionCheck').removeClass("promptMessageError").addClass("promptMessage").parent().show();
	        	var i = 0;
	        	$(".permissionCheck").html('<%= Localization.GetSafeJSString(LocalizeString("FileAndFolderPermissionCheckTitle.Text"))%>');
        		var origtext = $(".permissionCheck").html();
                var text = origtext;
                installWizard.checkPermissionIntervalId = setInterval(function () {
                	$(".permissionCheck").html(text + Array((++i % 6) + 1).join("."));
                	if (i === 6) text = origtext;
                }, 500);
	        };
            this.showInstallationTab = function () {
                $("#tabs").tabs('enable', 1);
                $("#tabs").tabs('option', 'active', 1);
                $("#tabs").tabs('disable', 0);
                installWizard.disableValidators();
                $("#languageFlags").hide();
            };
            this.showAccountInfoTab = function() {
                $("#tabs").tabs('enable', 0);
                $("#tabs").tabs('option', 'active', 0);
                $("#tabs").tabs('disable', 1);
                $("#languageFlags").show();
            };
            this.finishInstall = function () {
                installWizard.stopProgressBar();
                $('#seeLogs, #visitSite').removeClass('dnnDisabledAction');
                $('#installation-steps > p').attr('class', 'step-done');
                $('#tabs ul li a[href="#installInstallation"]').parent().removeClass('ui-tabs-active ui-state-active');
                $('#tabs ul li a[href="#installInstallation"]').parent().addClass('ui-state-disabled');
                $('#tabs ul li a[href="#installViewWebsite"]').parent().addClass('ui-tabs-active ui-state-active');
                $('.dnnWizardStepArrow', $('#tabs ul li a[href="#installAccountInfo"]')).css('background-position', '0 -401px');
                $('.dnnWizardStepArrow', $('#tabs ul li a[href="#installInstallation"]')).css('background-position', '0 -401px');
                $('.dnnWizardStepArrow', $('#tabs ul li a[href="#installInstallation"]')).css('background-position', '0 -201px');
                $('#tabs ul').css('background-position', '0 -100px');
            };
            this.progressBarIntervalId = {};
            this.timerIntervalId = {};
            this.startProgressBar = function () {
                $("#timer").html('0:00 ' + '<%=LocalizeString("TimerMinutes") %>');
                var totalSeconds = 0;
                var minutes = 0;

                installWizard.progressBarIntervalId = setInterval(function () {
                    $.getInstallProgress();
                }, 100);

                installWizard.timerIntervalId = setInterval(function () {
                    totalSeconds = totalSeconds + 1;
                    minutes = minutes + Math.floor(totalSeconds / 60);
                    totalSeconds = totalSeconds % 60;
                    var seconds = Math.floor(totalSeconds);
                    // Pad the minutes and seconds with leading zeros, if required
                    seconds = (seconds < 10 ? "0" : "") + seconds;

                    $("#timer").html(minutes + ":" + seconds + ' <%=LocalizeString("TimerMinutes") %>');
                }, 1000);
            };
            this.stopProgressBar = function () {
                clearInterval(installWizard.timerIntervalId);
                clearInterval(installWizard.progressBarIntervalId);
            };
            this.install = function () {
                $.startProgressbar();
                //Call PageMethod which triggers long running operation
                PageMethods.RunInstall(function () {
                }, function (err) {
                    $.stopProgressbarOnError();
                });
                $('#seeLogs, #visitSite, #retry').addClass('dnnDisabledAction');
                //Making sure that progress indicate 0
                $("#progressbar").progressbar('value', 0);
                $("#percentage").text('0% ');
                $("#timer").html('0:00 ' + '<%=LocalizeString("TimerMinutes") %>');
            };

            // Banner Rotator
            this.online = navigator.onLine;
            this.bannerIndex = 1;
            this.bannerMaxIndex = 0;
            this.bannerError = function (image) {
                this.bannerMaxIndex = this.bannerIndex - 1;
                if (this.bannerIndex == 2) {
                    this.bannerIndex = -1;
                } else {
                    this.bannerIndex = 1;
                }
                image.src = "../images/branding/DNN_logo.png";
                $("#bannerLink").attr("href", "");
                $("#bannerLink").attr("target", "");
                $("#bannerLink").click(function(){ return false;});
            };
        }
    </script>
    
    <!-- Page Level -->
    <script type="text/javascript">
            
        function LegacyLangaugePack(version) {
            $('#<%= lblLegacyLangaugePack.ClientID %>')[0].innerText = '<%= Localization.GetSafeJSString(LocalizeString("LegacyLangaugePack"))%>' + version;
        }
        function ClearLegacyLangaugePack() {
            $('#<%= lblLegacyLangaugePack.ClientID %>')[0].innerText = '';
        }
        
        // Banner Rotator
        jQuery(document).ready(function ($) {
            if (installWizard.online) {
            	installWizard.bannerTimer = setInterval(function () {
                    if (installWizard.bannerIndex != -1) {
                        if (installWizard.bannerIndex == installWizard.bannerMaxIndex) {
                            installWizard.bannerIndex = 1;
                            $("#bannerImage").attr("src", "../images/branding/DNN_logo.png");
                            $("#bannerLink").attr("href", "");
                            $("#bannerLink").attr("target", "");
                            $("#bannerLink").click(function () { return false; });
                        }
                        else {
                            $("#bannerLink").unbind('click');// click(function () { return false; });
                            $("#bannerImage").attr("src", "http://cdn.dotnetnuke.com/installer/banners/banner_" + installWizard.bannerIndex + ".jpg");
                            $("#bannerLink").attr("href", "http://www.dotnetnuke.com/installation/Banner" + installWizard.bannerIndex + ".aspx");
                            $("#bannerLink").attr("target", "_blank");
                            if (installWizard.bannerIndex != -1) {
                                $("#bannerLink").hide();
                                $("#bannerLink").show('slow');
                                installWizard.bannerIndex += 1;
                            }
                        }
                    }
                }, 5000);
            }
        });

        /*globals jQuery, window, Sys */
        (function ($, Sys) {
            $(function () {
                $("#tabs").bind("tabscreate", function (event, ui) {
                    var index = 0, selectedIndex = 0;
                    $('.ui-tabs-nav li', $(this)).each(function () {
                        if ($(this).hasClass('ui-tabs-active'))
                            selectedIndex = index;
                        index++;
                    });
                    $('.dnnWizardStepArrow', $(this)).eq(selectedIndex).css('background-position', '0 -299px');
                    if (selectedIndex)
                        $('.dnnWizardStepArrow', $(this)).eq(selectedIndex - 1).css('background-position', '0 -201px');
                });

                $("#tabs").bind("tabsactivate", function (event, ui) {
                    var index = ui.newTab.index();
                    $('.dnnWizardStepArrow', $(this)).css('background-position', '0 -401px');
                    $('.dnnWizardStepArrow', $(this)).eq(index).css('background-position', '0 -299px');
                    if (index) {
                        $('.dnnWizardStepArrow', $(this)).eq(index - 1).css('background-position', '0 -201px');
                    }
                });
                $("#tabs").tabs();
                $("#tabs").tabs({ disabled: [1, 2] });
                $('.dnnFormMessage.dnnFormError').each(function () {
                    if ($(this).html().length)
                        $(this).css('display', 'block');
                });
                installWizard.dnnProgressbar = $(".dnnProgressbar").dnnProgressbar();
            });

            $(document).ready(function () {
                //Reset Validation
                $('.dnnRequired').hide();
                
                if(window.location.href.indexOf("&executeinstall")>-1) {                    
                    installWizard.showInstallationTab();
                    installWizard.install();
                }
                else {
                    //Go to installation page when installation is already in progress
                    PageMethods.IsInstallerRunning(function(result) {
                        if (result == true) {
                            installWizard.showInstallationTab();
                            $.startProgressbar();
                        } else {
                            installWizard.InitializeInstallScreen();
                        }
                    });
                }
            });

            //****************************************************************************************
            // EVENT HANDLER FUNCTIONS
            //****************************************************************************************
            //Database Functions
            $('#<%= databaseSetupType.ClientID %>').change(function () {
                installWizard.toggleAdvancedDatabase(true);
            });
            $('#<%= databaseType.ClientID %>').change(function () {
                installWizard.toggleDatabaseType(true);
            });
            $('#<%= databaseSecurityType.ClientID %>').change(function () {
                installWizard.toggleDatabaseSecurity(true);
            });
            //Password Check
            $('#<%= txtPassword.ClientID %>').focusout(function () {
                installWizard.validatePassword();
            });
            $('#<%= txtConfirmPassword.ClientID %>').focusout(function () {
                installWizard.validatePassword();
            });
            //Next Step
            $('#<%= continueLink.ClientID %>').click(function () {
            	if (!$("#continueLink").hasClass('dnnDisabledAction')) {
                    $("#continueLink").addClass('dnnDisabledAction');                    
                    if (installWizard.confirmPasswords()) {
                        installWizard.installInfo = {
                            username: $('#<%= txtUsername.ClientID %>')[0].value,
                            password: $('#<%= txtPassword.ClientID %>')[0].value,
                            confirmPassword: $('#<%= txtConfirmPassword.ClientID %>')[0].value,
                            websiteName: $('#<%= txtWebsiteName.ClientID %>')[0].value,
                            template: $find('<%= templateList.ClientID %>').get_value(),
                            language: $find('<%= languageList.ClientID %>').get_value(),
                            databaseSetup: $('#<%= databaseSetupType.ClientID %> input:checked').val(),
                            threadCulture: $("#PageLocale")[0].value,
                            databaseServerName: "",
                            databaseFilename: "",
                            databaseType: "",
                            databaseName: "",
                            databaseObjectQualifier: "",
                            databaseSecurity: "",
                            databaseUsername: "",
                            databasePassword: "",
                            databaseRunAsOwner: null
                        };
                        $('#<%= lblAccountInfoError.ClientID %>').css('display', 'none');
                        var databaseType = $('#<%= databaseSetupType.ClientID %> input:checked').val();
                        if (databaseType == "advanced") {
                            installWizard.installInfo.databaseServerName = $('#<%= txtDatabaseServerName.ClientID %>')[0].value;
                            installWizard.installInfo.databaseFilename = $('#<%= txtDatabaseFilename.ClientID %>')[0].value;
                            installWizard.installInfo.databaseType = $('#<%= databaseType.ClientID %> input:checked').val();
                            installWizard.installInfo.databaseName = $('#<%= txtDatabaseName.ClientID %>')[0].value;
                            installWizard.installInfo.databaseObjectQualifier = $('#<%= txtDatabaseObjectQualifier.ClientID %>')[0].value;
                            installWizard.installInfo.databaseSecurity = $('#<%= databaseSecurityType.ClientID %> input:checked').val();
                            installWizard.installInfo.databaseUsername = $('#<%= txtDatabaseUsername.ClientID %>')[0].value;
                            installWizard.installInfo.databasePassword = $('#<%= txtDatabasePassword.ClientID %>')[0].value;
                            installWizard.installInfo.databaseRunAsOwner = $('#<%= databaseRunAs.ClientID %>')[0].value;
                        }

                        PageMethods.ValidateInput(installWizard.installInfo, function(result) {
                            if (result.Item1) {
                                $('#<%= lblAccountInfoError.ClientID %>').text('');
                                $('#<%= lblDatabaseConnectionError.ClientID %>').html("");
                                $("#databaseError").hide();

                                installWizard.checkingDatabase();
                                PageMethods.VerifyDatabaseConnection(installWizard.installInfo, function (valid) {
                                	clearInterval(installWizard.loadingIntervalId);
                                    $('#<%= lblDatabaseInfoMsg.ClientID %>').text('');
                                    if (valid.Item1) {
                                    	$('#<%= lblDatabaseInfoMsg.ClientID %>').removeClass("promptMessage");
                                        //Restart app to refresh config from web.config
                                        window.location.replace(window.location + "?culture=" + $("#PageLocale")[0].value + "&initiateinstall");
                                    } else {
                                        $("#databaseError").show();
                                        $('#<%= lblDatabaseInfoMsg.ClientID %>').removeClass("promptMessage");
                                    	$('#<%= lblDatabaseInfoMsg.ClientID %>').addClass("promptMessageError");
                                        $('#<%= lblDatabaseInfoMsg.ClientID %>').text('<%= Localization.GetSafeJSString(LocalizeString("DatabaseError"))%>');
                                        $('#<%= lblDatabaseError.ClientID %>').html(valid.Item2);
                                    }
                                    $("#continueLink").removeClass('dnnDisabledAction');
                                });
                            } else {
                                installWizard.validateInput();
                                $('#<%= lblAccountInfoError.ClientID %>').text(result.Item2);
                                $('#<%= lblAccountInfoError.ClientID %>').css('display', 'block');
                                $("#continueLink").removeClass('dnnDisabledAction');
                            }
                        });
                    }
                }
                return false;
            });
            
        } (jQuery, window.Sys));
    </script>
       
    <!-- Progressbar -->
    <script type="text/javascript">
        $.getInstallProgress = function () {
            var xmlhttp;
            if (window.XMLHttpRequest) {
                xmlhttp = new XMLHttpRequest();
            } 
            xmlhttp.onreadystatechange = function () {
                if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
                    var statuslines = xmlhttp.responseText.split('\n');
                    $.updateProgressbar(statuslines[statuslines.length - 2]);
                } else {
                    installWizard.Status = "";
                }
            };
            xmlhttp.open("GET", "<%= StatusFilename %>" + "?" + Math.random(), true);
            xmlhttp.send();
        };

        $.updateProgressbar = function (status) {
            var result = jQuery.parseJSON(status);
            if (result !== null) {
	            if (result.progress < $("#progressbar").progressbar('value')) return;
                //Updating progress              
	            $("#progressbar").progressbar('value', result.progress);
	            installWizard.dnnProgressbar.update(result.progress);
                $("#percentage").text(result.progress + '% ' + result.details);
                var installationError = result.details.toUpperCase().indexOf('ERROR') > -1;
                if (installationError) {
                    // go through all the result and mark error state
                    for (var i = 0; i < 6; i++) {
                        var done = result["check" + i] === "Done";
                        if (!done) { break; }
                    } 
                }
                $.applyCssStyle(result.check0, $('#FileAndFolderPermissionCheck'));
                $.applyCssStyle(result.check1, $('#DatabaseInstallation'));
                $.applyCssStyle(result.check2, $('#ExtensionsInstallation'));
                $.applyCssStyle(result.check3, $('#WebsiteCreation'));
                $.applyCssStyle(result.check4, $('#SuperUserCreation'));
                $.applyCssStyle(result.check5, $('#LicenseActivation'));
                //If operation is complete
                if (result.progress >= 100 || result.details == '<%= Localization.GetSafeJSString(LocalizeString("InstallationDone"))%>') {
                    installWizard.finishInstall();
                    $('#<%= lblInstallationIntroInfo.ClientID %>').text('<%= Localization.GetSafeJSString(LocalizeString("InstallationComplete"))%>');
                }
                //If not
                else {
                    if (installationError) { // if error in installation                       
                        $.stopProgressbarOnError();
                        //Allow user to visit site even if only license step error occurs.
                        if (result["check4"] === "Done" && result.check5.indexOf("Error" > -1)) {
                            $.applyCssStyle("Error", $('#LicenseActivation'));
                            $('#visitSite').removeClass('dnnDisabledAction');
                        }
                    }
                }
            }
        };
        
        $.applyCssStyle = function (state, ele) {
            if (!state) state = '';
            switch (state.toLowerCase()) {
                case 'done':
                    ele.attr('class', 'step-done');
                    break;
                case 'running':
                    ele.attr('class', 'step-running');
                    break;
                case 'error':
                    ele.attr('class', 'step-error');
                    break;
                default:
                    ele.attr('class', 'step-notstarted');
                    break;
            }
        };

        //Start progress bar
        $.startProgressbar = function () {
            //Disabling button
            $('#seeLogs, #visitSite, #retry').addClass('dnnDisabledAction');
            //Making sure that progress indicate 0            
            $("#progressbar").progressbar().progressbar('value', 0);
            $("#percentage").text('0%');
            installWizard.startProgressBar();
            $("#progressbar").removeClass('stoppedProgress');
            $("#progressbar").addClass('inProgress');
        };

        //Stop update progress bar on errors
        $.stopProgressbarOnError = function () {
            $('#seeLogs, #retry').removeClass('dnnDisabledAction');
            if ($.updateProgressbarTimeId) {
                clearTimeout($.updateProgressbarTimeId);
                $.updateProgressbarTimeId = null;
            }
            $('#installation-steps > p.step-running').attr('class', 'step-error');
            installWizard.stopProgressBar();
            $("#progressbar").removeClass('inProgress');
            $("#progressbar").addClass('stoppedProgress');
        };

        $(document).ready(function () {
            //Progressbar and button initialization                   
            $("#progressbar").progressbar({ value: 0 });
            $('#visitSite, #seeLogs, #retry').addClass('dnnDisabledAction');

            $("#retry").click(function (e) {
                e.preventDefault();
                if (!$(this).hasClass('dnnDisabledAction')) {
                    $('#retry').addClass('dnnDisabledAction');
                    $('#installation-log-container').hide();
                    $.startProgressbar();
                    //Call PageMethod which triggers long running operation
                    PageMethods.RunInstall(function () {
                    }, function (err) {
                        $.stopProgressbarOnError();
                    });
                }
            });

            var installationLogStartLine = 0;
            var getInstallationLog = function () {
                PageMethods.GetInstallationLog(installationLogStartLine, function (result) {
                    if (result) {
                        if (installationLogStartLine === 0)
                            $('#installation-log').html(result);
                        else
                            $('#installation-log').append(result);
                        
                        installationLogStartLine += 500;
                        setTimeout(getInstallationLog, 100);
                    } else {
                        if (installationLogStartLine === 0)
                            $('#installation-log').html('<%= Localization.GetSafeJSString(LocalizeString("NoInstallationLog"))%>');
                    }
                    $('#installation-log-container').jScrollPane();
                }, function (err) {
                });
            };

            $('#seeLogs').click(function (e) {
                e.preventDefault();
                if (!$(this).hasClass('dnnDisabledAction')) {
                    $(this).addClass('dnnDisabledAction');
                    $('#installation-log-container').show();
                    getInstallationLog();
                }
            });
            
            $('#visitSite').click(function (e) {
                if ($(this).hasClass('dnnDisabledAction')) {
                    e.preventDefault();
                } else {
	                $(this).addClass('dnnDisabledAction');
                	if(installWizard.bannerTimer) {
                		clearInterval(installWizard.bannerTimer);
                	}
                }
            });
        });
    </script>    

</body>
</html>
