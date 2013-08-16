<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.SkinEditor" CodeFile="SkinEditor.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<h2 class="dnnFormSectionHead"><a href="" class="dnnLabelExpanded"><asp:Label ID="lblTitle" runat="server"/></a></h2>
<fieldset>
    <div class="dnnFormMessage dnnFormInfo"><asp:Label ID="lblHelp" runat="server" /></div>
    <dnn:DnnFormEditor id="skinForm" runat="Server" FormMode="Short" Visible="false">
	    <Items><dnn:DnnFormTextBoxItem ID="name" runat="server" DataField = "SkinName" /></Items>
    </dnn:DnnFormEditor>
    <dnn:DnnFormEditor id="skinFormReadOnly" runat="Server" FormMode="Short" Visible="false">
        <Items><dnn:DnnFormLiteralItem ID="nameReadOnly" runat="server" DataField = "SkinName" /></Items>
    </dnn:DnnFormEditor>
</fieldset>