<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Settings.ascx.cs" Inherits="DotNetNuke.Modules.DigitalAssets.Settings" %>
<%@ Register TagPrefix="dnnweb" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>

<h2 id="dnnSitePanel-BasicSettings" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("BasicSettings")%></a></h2>
<fieldset>
    <div class="dnnFormItem">
        <dnnweb:Label ID="DefaultFolderTypeLabel" runat="server" ResourceKey="DefaultFolderType" Suffix=":" ControlName="DefaultFolderTypeComboBox" />
		<dnnweb:DnnComboBox id="DefaultFolderTypeComboBox" DataTextField="MappingName" DataValueField="FolderMappingID" runat="server" CssClass="dnnFixedSizeComboBox" />
    </div>
    
    <div class="dnnFormItem">
        <dnnweb:Label ID="ModeLabel" runat="server" ResourceKey="Mode" Suffix=":" ControlName="ModeComboBox" />
        <dnnweb:DnnComboBox ID="ModeComboBox" runat="server" OnClientSelectedIndexChanged="updateRootFolderItemVisibility" >
            <Items>
                <dnnweb:DnnComboBoxItem Value="Normal" ResourceKey="Normal"></dnnweb:DnnComboBoxItem>
                <dnnweb:DnnComboBoxItem Value="Group" ResourceKey="Group"></dnnweb:DnnComboBoxItem>
                <dnnweb:DnnComboBoxItem Value="User" ResourceKey="User"></dnnweb:DnnComboBoxItem>
            </Items>
        </dnnweb:DnnComboBox>
    </div>

    <div class="dnnFormItem" ID="ViewConditionItem" runat="server">
        <dnnweb:Label ID="ViewConditionLabel" runat="server" ResourceKey="ViewCondition" Suffix=":" />
        <div class="dnnDigitalAssetsSettingControlBox">
            <asp:RadioButtonList runat="server" ID="FilterOptionsRadioButtonsList" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                <asp:ListItem Value="NoSet" resourcekey="NoSet" Selected="True" />
                <asp:ListItem Value="FilterByFolder" resourcekey="FilterByFolder" />
            </asp:RadioButtonList>
            <div id="FilterByFolderOptions">
                <dnnweb:DnnFolderDropDownList ID="FilterByFolderDropDownList" runat="server" /><br/>
                <div>                        
                    <asp:RadioButtonList ID="SubfolderFilterRadioButtonList" runat="server" RepeatDirection="Vertical" RepeatLayout="Flow">
                        <asp:ListItem Value="ExcludeSubfolders" resourcekey="ExcludeSubfolders" Selected="True" />                      
                        <asp:ListItem Value="IncludeSubfoldersFilesOnly" resourcekey="IncludeSubfoldersFilesOnly"/>                      
                        <asp:ListItem Value="IncludeSubfoldersFolderStructure" resourcekey="IncludeSubfoldersFolderStructure"/>
                    </asp:RadioButtonList>
                </div>                
                <asp:CustomValidator runat="server" Display="Dynamic" resourcekey="FolderMustBeSelected.ErrorMessage" 
                    CssClass="dnnFormMessage dnnFormError" ID="FolderMustBeSelected" OnServerValidate="ValidateFolderIsSelected" ClientValidationFunction="settingsController.ValidateFolderIsSelected"/>
            </div>
        </div>
    </div>
</fieldset>

<script type="text/javascript">

    function updateRootFolderItemVisibility(sender) {
        if (sender.get_value() != "Normal") {
            $('#<%=ViewConditionItem.ClientID%>').hide();
        } else {            
            $('#<%=ViewConditionItem.ClientID%>').show();
        }
    }

    var settingsController;
    
    $(function () {
        
    <% if (ModeComboBox.SelectedValue != "Normal") 
       { %>    
            $('#<%=ViewConditionItem.ClientID%>').hide();            
    <% } %>
        
        settingsController = new dnn.DigitalAssetsFilterViewSettingsController({ serviceRoot: "DigitalAssetsPro" },
            {
                FolderDropDownList: $("#<%= FilterByFolderDropDownList.ClientID %>"),
                FilterOptionGroupID: '<%= FilterOptionsRadioButtonsList.ClientID %>',
                moduleId: "<%= ModuleId %>"
            });

        settingsController.initFilterOptionsRadioInput();
    });
</script>