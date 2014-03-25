<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Modules.Import"
    CodeFile="Import.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm" id="importModuleForm">
    <h2 id="dnnPanel-ImportFile" class="dnnFormSectionHead">
        <a href="#" class="dnnSectionExpanded">
            <%=LocalizeString("ImportFile")%></a></h2>
    <fieldset class="dnnClear">
        <div class="dnnFormItem">
            <dnn:Label ID="plFolder" runat="server" ControlName="cboFolders" Suffix=":" />
            <dnn:DnnFolderDropDownList ID="cboFolders" runat="server" AutoPostBack="True" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="plFile" runat="server" ControlName="cboFiles" Suffix=":" />
              <dnn:DnnComboBox runat="server" ID="cboFiles"  AutoPostBack="true"/>
        </div>
 </fieldset>
    <h2 id="dnnPanel-ModuleContent" class="dnnFormSectionHead">
        <a href="#" class="">
            <%=LocalizeString("ModuleContent")%></a></h2>
    <fieldset>
        <div class="dnnFormItem">
            <asp:TextBox ID="txtContent" runat="server" Rows="12" TextMode="MultiLine" Wrap="False"
                Columns="100" />
        </div>
    </fieldset>
    <br />
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdImport" resourcekey="cmdImport" runat="server" CssClass="dnnPrimaryAction" /></li>
        <li>
            <asp:HyperLink ID="cmdCancel" resourcekey="cmdCancel" runat="server" CssClass="dnnSecondaryAction"
                causesvalidation="False" /></li>
    </ul>
</div>
<script type="text/javascript">
    jQuery(function ($) {
        var setupModule = function () {
            $('#importModuleForm').dnnPanels();
        };
        setupModule();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setupModule();
        });
    });
</script>
