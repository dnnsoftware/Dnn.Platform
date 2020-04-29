<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Settings.ascx.cs" Inherits="Dnn.Modules.ResourceManager.Settings" %>
<%@ Register TagName="label" TagPrefix="dnn" Src="~/controls/labelcontrol.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>

<h2 id="dnnSitePanel-BasicSettings" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("BasicSettings")%></a></h2>
<fieldset>
    <div class="dnnFormItem">
        <dnn:Label ID="lblMode" runat="server" /> 
 
        <asp:DropDownList ID="ddlMode" runat="server" />
    </div>
</fieldset>
<fieldset>
    <div class="dnnFormItem">
        <dnn:label ID="lblHomeFolder" runat="server" />
        <dnn:DnnFolderDropDownList ID="ddlFolder" runat="server" />
    </div>
</fieldset>




