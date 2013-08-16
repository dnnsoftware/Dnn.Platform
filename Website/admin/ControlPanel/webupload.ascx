<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.FileManager.WebUpload" CodeFile="WebUpload.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnFileUpload dnnClear" id="dnnFileUpload">
    <fieldset>
        <div id="rootRow" runat="server" visible="false" class="dnnFormItem">
            <div class="dnnLabel">
            <label><asp:Label ID="lblRootType" runat="server" CssClass="dnnFormLabel" /></label>
            </div>
            <asp:Label ID="lblRootFolder" runat="server"  />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="plBrowse" runat="server" ControlName="cmdBrowse" />
            <input id="cmdBrowse" type="file" size="50" name="cmdBrowse" runat="server" />
            <asp:LinkButton ID="cmdAdd" runat="server" CssClass="dnnPrimaryAction"  />
        </div>
        <div id="foldersRow" runat="server" visible="false" class="dnnFormItem">
            <dnn:Label ID="plFolder" runat="server" ControlName="ddlFolders" />
            <dnn:DnnFolderDropDownList ID="ddlFolders" runat="server" />
        </div>
        <div id="unzipRow" runat="server" visible="false" class="dnnFormItem">
            <dnn:Label ID="Label1" runat="server" ControlName="chkUnzip" resourcekey="Decompress" />
            <asp:CheckBox ID="chkUnzip" runat="server"  />
        </div>
        <div class="dnnFormItem">
            <asp:Label ID="lblMessage" runat="server" EnableViewState="False" />
        </div>
    </fieldset>
    <div class="dnnFormMessage dnnFormInfo">
        <asp:Label id="maxSizeWarningLabel" runat="server" />
    </div>
    <ul class="dnnActions dnnClear">
    	<li><asp:LinkButton id="cmdReturn1" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdReturn" /></li>
    </ul>
</div>
<div id="tblLogs" runat="server" visible="False">
	<div class="dnnFormItem">
        <asp:Label ID="lblLogTitle" runat="server" resourcekey="LogTitle" />
        <asp:PlaceHolder ID="phPaLogs" runat="server" />
    </div>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton id="cmdReturn2" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdReturn" /></li>
    </ul>
</div>