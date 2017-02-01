<%@ Control Language="C#" Inherits="DotNetNuke.Providers.FiftyOneClientCapabilityProvider.Administration, DotNetNuke.Providers.FiftyOneClientCapabilityProvider" AutoEventWireup="true" CodeBehind="Administration.ascx.cs" %>
<%@ Register Assembly="FiftyOne.Foundation" Namespace="FiftyOne.Foundation.UI.Web" TagPrefix="fiftyOne" %>
<%@ Register Assembly="FiftyOne.Foundation" Namespace="FiftyOne.Foundation.Mobile.Detection" TagPrefix="fiftyOne" %>
<%@ Register Assembly="DotNetNuke.Providers.FiftyOneClientCapabilityProvider" Namespace="DotNetNuke.Providers.FiftyOneClientCapabilityProvider" tagPrefix="dnn" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls.Internal" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelcontrol.ascx" %>

<script runat="server">
   
    void Page_Load(Object sender, EventArgs e) 
    {
        var enabled = FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.Enabled && WebProvider.ActiveProvider != null;
        LabelDataSetName.Text = enabled ? WebProvider.ActiveProvider.DataSet.Name : LocalizeString("Unavailable.Text");
        LabelDataSetPublished.Text = enabled ? WebProvider.ActiveProvider.DataSet.Published.ToString("d") : LocalizeString("Unavailable.Text");
        LabelDataSetNextUpdate.Text = enabled ? WebProvider.ActiveProvider.DataSet.NextUpdate.ToString("d") : LocalizeString("Unavailable.Text");
        LabelDataSetPropertyCount.Text = enabled ? WebProvider.ActiveProvider.DataSet.Properties.Count.ToString() : LocalizeString("Unavailable.Text");
        LabelDataSetVersion.Text = enabled ? WebProvider.ActiveProvider.DataSet.Version.ToString() : LocalizeString("Unavailable.Text");
        LabelDataSetHardwareProfiles.Text = enabled ? WebProvider.ActiveProvider.DataSet.Hardware.Profiles.Count().ToString("n0") : LocalizeString("Unavailable.Text");
        LabelDataSetDeviceCombinations.Text = enabled ? WebProvider.ActiveProvider.DataSet.DeviceCombinations.ToString("n0") : LocalizeString("Unavailable.Text");
        CheckBoxAutoUpdate.Enabled = this.IsPremium;
        ButtonSettingsRefresh.Enabled = enabled;
        LabelFactory.Text = enabled ? WebProvider.ActiveProvider.DataSet.Mode.ToString() : LocalizeString("Unavailable.Text");

        LiteralGetStartedActivateIntro.Text = LocalizeString("GetStartedIntro.Html");
        LiteralGetStartedActivateDetail.Text = LocalizeString("GetStartedDetail.Html");
        LiteralGetStartedIntro.Text = LocalizeString("GetStartedIntro.Html");
        LiteralGetStartedDetail.Text = LocalizeString("GetStartedDetail.Html");
		ButtonGetStartedActivate.Text = LocalizeString("GetStartedButton.Text");
		ButtonGetStartedActivate.Visible = FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.Enabled == false;

        DeviceBrowserUnavailableLiteral.Text = LocalizeString("DeviceBrowserUnavailable.Html");
        DeviceBrowserUnavailableButton.NavigateUrl = LocalizeString("DeviceBrowserUnavailableButton.Url");

        TopDevices.DeviceUrl = TabController.CurrentPage.FullUrl;
    }
    
</script>

<div class="DnnModule DnnModule-Device-Detection">
    <div id="fiftyOneDegrees" class="dnnForm ui-tabs ui-widget ui-widget-content ui-corner-all">
               
        <%--List of available tabs for configuration--%>
        <ul class="dnnAdminTabNav">
            <li><a href="#panels-settings"><%= LocalizeString("Settings.Header")%></a></li>
            <li><a href="#panels-explorer"><%= LocalizeString("Explorer.Header")%></a></li>
            <% if (this.IsPremium) { %>
            <li><a href="#panels-topdevices"><%= LocalizeString("NewDevices.Header")%></a></li>
            <% } %>
            <li><a href="#panels-properties"><%= LocalizeString("Properties.Header")%></a></li>
            <% if (this.IsPremium == false) { %>
            <% if (IsPostBack == false) { %>
            <li aria-selected="true"><a href="#panels-activate"><%= LocalizeString("Activate.Header")%></a></li>
            <% } else { %>
            <li><a href="#panels-activate"><%= LocalizeString("Activate.Header")%></a></li>
            <% } %>
            <% } else { %>
            <li><a href="#panels-get-started"><%= LocalizeString("GetStarted.Header")%></a></li>
            <% } %>
        </ul>

        <%--Setting change messages--%>
        <p id="UploadError" runat="server" class="dnnFormMessage dnnFormValidationSummary"><%=GetLicenseFormatString("PremiumUploadError.Text")%></p>
	    <p id="UploadSuccess" runat="server" class="dnnFormMessage dnnFormSuccess"><%=GetLicenseFormatString("PremiumUploadSuccess.Text")%></p>
        <p id="SettingsChangedSuccess" runat="server" class="dnnFormMessage dnnFormSuccess"><%=GetLicenseFormatString("SettingsChangeSuccess.Text")%></p>
        <p id="SettingsChangedError" runat="server" class="dnnFormMessage dnnFormSuccess"><%=GetLicenseFormatString("SettingsChangeError.Text")%></p>

        <%--Main settings available with every version of the data set--%>
        <div id="panels-settings" class="dnnClear">
        <div class="dnnFormExpandContent"><a href="#">Expand All</a></div>
            
        <h2 class="dnnFormSectionHead"><a href="#"><%= LocalizeString("SettingsConfiguration.Header")%></a></h2>
        <fieldset>
            <div class="dnnFormItem">
                <dnn:label runat="server" ID="LabelImageOptimiser" ControlName="CheckBoxImageOptimiser" resourcekey="SettingsImageOptimiser" />
                <asp:CheckBox ID="CheckBoxImageOptimiser" runat="server" />
            </div>
            <div class="dnnFormItem">
                <dnn:label runat="server" ID="LabelEnabled" ControlName="CheckBoxEnabled" resourcekey="SettingsEnabled" />
                <asp:CheckBox ID="CheckBoxEnabled" runat="server" />
            </div>
            <div class="dnnFormItem">
                <dnn:label runat="server" ID="LabelShareUsage" ControlName="CheckBoxShareUsage" resourcekey="SettingsShareUsage" />
                <asp:CheckBox ID="CheckBoxShareUsage" runat="server" />
            </div>
            <div class="dnnFormItem">
                <dnn:label runat="server" ID="LabelAutoUpdate" ControlName="CheckBoxAutoUpdates" resourcekey="SettingsAutoUpdates" />
                <asp:CheckBox ID="CheckBoxAutoUpdate" runat="server" />
            </div>
            <div class="dnnFormItem">
                <dnn:label runat="server" ID="LabelFileMode" ControlName="CheckBoxFileMode" resourcekey="SettingsFileMode" />
                <asp:CheckBox ID="CheckBoxFileMode" runat="server" />
            </div>
        </fieldset>

        <h2 class="dnnFormSectionHead"><a href="#"><%= LocalizeString("SettingsStats.Header")%></a></h2>
        <fieldset>
            <div class="dnnFormItem">
                <dnn:label runat="server" ControlName="LabelDataSetName" resourcekey="SettingsDataSetName" />
                <asp:Label runat="server" ID="LabelDataSetName" />
            </div>
            <div class="dnnFormItem">
                <dnn:label runat="server" ControlName="LabelDataSetPublished" resourcekey="SettingsDataSetPublished" />
                <asp:Label runat="server" ID="LabelDataSetPublished" />
            </div>
            <div class="dnnFormItem">
                <dnn:label runat="server" ControlName="LabelDataSetNextUpdate" resourcekey="SettingsDataSetNextUpdate" />
                <asp:Label runat="server" ID="LabelDataSetNextUpdate" />
            </div>
            <div class="dnnFormItem">
                <dnn:label runat="server" ControlName="LabelDataSetPropertyCount" resourcekey="SettingsDataSetPropertyCount" />
                <asp:Label runat="server" ID="LabelDataSetPropertyCount" />
            </div>
            <div class="dnnFormItem">
                <dnn:label runat="server" ControlName="LabelDataSetHardwareProfiles" resourcekey="SettingsDataSetHardwareProfiles" />
                <asp:Label runat="server" ID="LabelDataSetHardwareProfiles" />
            </div>
            <div class="dnnFormItem">
                <dnn:label runat="server" ControlName="LabelDataSetDeviceCombinations" resourcekey="SettingsDataSetDeviceCombinations" />
                <asp:Label runat="server" ID="LabelDataSetDeviceCombinations" />
            </div>
            <div class="dnnFormItem">
                <dnn:label runat="server" ControlName="LabelDataSetVersion" resourcekey="SettingsDataSetVersion" />
                <asp:Label runat="server" ID="LabelDataSetVersion" />
            </div>
            <div class="dnnFormItem">
                <dnn:label runat="server" ControlName="LabelFactory" resourcekey="SettingsFactory" />
                <asp:Label runat="server" ID="LabelFactory" />
            </div>
            <div class="dnnFormItem">
                <dnn:label runat="server" ControlName="refresh" resourcekey="SettingsRefresh" />
                <asp:LinkButton ID="ButtonSettingsRefresh" runat="server" ControlKey="Refresh" CssClass="dnnSecondaryAction" resourcekey="Refresh.Action" />
            </div>
        </fieldset>        

        <h2 class="dnnFormSectionHead"><a href="#"><%= LocalizeString("SettingsManualUpload.Header")%></a></h2>
        <fieldset>
            <div class="dnnFormItem">
                <dnn:label runat="server" ControlName="Upload" resourcekey="SettingsUpload" />
                <fiftyOne:Upload runat="server" ID="Upload" FooterEnabled="False" LogoEnabled="False" ButtonCssClass="dnnSecondaryAction" CssClass="dnnUploadActions" />
            </div>
        </fieldset>
        <ul class="dnnActions dnnClear">
	        <li><asp:LinkButton id="ButtonSettingsUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="Update.Action" OnClientClick="form.submit();" /></li>
		    <li><asp:LinkButton id="ButtonSettingsCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="Cancel.Action" OnClientClick="window.history.back();" /></li>
	    </ul>
        </div>

        <div id="panels-explorer" class="dnnClear">
		<% if (this.IsPremium) { %>
			<fiftyOne:DeviceExplorer runat="server" ID="DeviceBrowser" FooterEnabled="False" LogoEnabled="False" CssClass="deviceExplorerVendors" SearchButtonCssClass="dnnSecondaryAction" BackButtonCssClass="dnnSecondaryAction" />
		<% } else { %>
            <asp:Literal runat="server" ID="DeviceBrowserUnavailableLiteral" />
            <ul class="dnnActions dnnClear">
                <li><asp:HyperLink id="DeviceBrowserUnavailableButton" runat="server" CssClass="dnnPrimaryAction" resourcekey="DeviceBrowserUnavailableButton.Action" /></li>
            </ul>
        <% } %>
        </div>
        
		<% if (this.IsPremium) { %>
        <div id="panels-topdevices" class="dnnClear">
			<fiftyOne:TopDevices runat="server" ID="TopDevices" FooterEnabled="False" LogoEnabled="False" CssClass="topDevices" />
        </div>
        <% } %>

        <div id="panels-properties" class="dnnClear">
			<fiftyOne:PropertyDictionary runat="server" ID="Properties" FooterEnabled="False" LogoEnabled="False" CssClass="propertyDictionary" />
        </div>

        <% if (this.IsPremium == false) { %>
        <div id="panels-activate" class="dnnClear">
            <asp:Image runat="server" ImageUrl="Images/Optimization.png?w=300" title="Mobile Optimisation" style="float: right; margin: 0 0 1em 1em; width:300px;"/>
            <asp:Literal runat="server" ID="LiteralGetStartedActivateIntro" />
            <asp:LinkButton runat="server" ID="ButtonGetStartedActivate" CssClass="dnnSecondaryAction" />
            <asp:Literal runat="server" ID="LiteralGetStartedActivateDetail" />
			<br/>
            <fiftyOne:Detection runat="server" ID="Activate" LogoEnabled="False" CssClass="upgradePremium"
							ErrorCssClass="dnnFormMessage dnnFormValidationSummary" 
							SuccessCssClass="dnnFormMessage dnnFormSuccess" FooterEnabled="False"
							ButtonCssClass="dnnSecondaryAction" ShowShareUsage="False"
							CheckBoxCssClass="dnnInvisible" TextBoxCssClass="licenceKey"
                            ShowUpload="False" InstructionsEnabled="True" />
        </div>
        <% } else { %>
        <div id="panels-get-started" class="dnnClear">
            <asp:Image runat="server" ImageUrl="Images/Optimization.png?w=300" title="Mobile Optimisation" style="float: right; margin: 0 0 1em 1em; width:300px;"/>
            <asp:Literal runat="server" ID="LiteralGetStartedIntro" />
            <asp:Literal runat="server" ID="LiteralGetStartedDetail" />
        </div>
        <% } %>

    </div>
</div>

<script type="text/javascript">
    jQuery(function ($) {
        $('#fiftyOneDegrees').dnnTabs();
    });
</script>   

<script type="text/javascript">
    jQuery(function ($) {
        var setupModule = function () {
            $('#panels-settings').dnnPanels();
            $('#panels-settings .dnnFormExpandContent a').dnnExpandAll({
                targetArea: '#panels-settings'
            });
        };
        setupModule();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            // note that this will fire when _any_ UpdatePanel is triggered,
            // which may or may not cause an issue
            setupModule();
        });
    });
</script>