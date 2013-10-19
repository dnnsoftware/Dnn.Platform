<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Settings.ascx.cs" Inherits="DotNetNuke.Modules.DigitalAssets.Settings" %>
<%@ Register TagPrefix="dnnweb" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>

<h2 id="dnnSitePanel-BasicSettings" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("BasicSettings")%></a></h2>
<fieldset>
    <div class="dnnFormItem">
        <dnnweb:Label ID="DefaultFolderTypeLabel" runat="server" ResourceKey="DefaultFolderType" Suffix=":" ControlName="DefaultFolderTypeComboBox" />
		<dnnweb:DnnComboBox id="DefaultFolderTypeComboBox" DataTextField="MappingName" DataValueField="FolderMappingID" runat="server" CssClass="dnnFixedSizeComboBox" />
    </div>
    
    <div class="dnnFormItem">
        <dnnweb:Label ID="GroupModeLabel" runat="server" ResourceKey="GroupMode" Suffix=":" ControlName="GroupModeComboBox" />
        <dnnweb:DnnComboBox ID="GroupModeComboBox" runat="server" OnClientSelectedIndexChanged="updateRootFolderItemVisibility" >
            <Items>
                <dnnweb:DnnComboBoxItem Value="False" ResourceKey="Normal"></dnnweb:DnnComboBoxItem>
                <dnnweb:DnnComboBoxItem Value="True" ResourceKey="Group"></dnnweb:DnnComboBoxItem>
            </Items>
        </dnnweb:DnnComboBox>
    </div>

    <div class="dnnFormItem" ID="RootFolderItem" runat="server">
        <dnnweb:Label ID="RootFolderLabel" runat="server" ResourceKey="RootFolder" Suffix=":" ControlName="RootFolderDropDownList" />
        <dnnweb:DnnFolderDropDownList ID="RootFolderDropDownList" runat="server" /><br/>
    </div>

</fieldset>

<script type="text/javascript">

    function updateRootFolderItemVisibility(sender) {
        if (sender.get_value() == "True") {
            $('#<%=RootFolderItem.ClientID%>').hide();
        } else {            
            $('#<%=RootFolderItem.ClientID%>').show();
        }
    }

    <% if (GroupModeComboBox.SelectedValue == "True") 
       { %>
    
        $(function() {        
            $('#<%=RootFolderItem.ClientID%>').hide();        
        });
    
    <% } %>
        
</script>