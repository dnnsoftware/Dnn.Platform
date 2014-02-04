<%@ Control language="C#" Inherits="DotNetNuke.Modules.XmlMerge.XmlMerge" CodeFile="XmlMerge.ascx.cs" AutoEventWireup="false" Explicit="True" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<%-- Custom CSS Registration --%>
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/lib/codemirror.css" />

<%-- Custom JavaScript Registration --%>
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/lib/codemirror.js" Priority="1" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/xml/xml.js" Priority="2" />


<div class="dnnForm dnnConfigManager dnnClear" id="dnnConfigManager">
    <ul class="dnnAdminTabNav">
        <li><a href="#dnnConfigFiles"><%=LocalizeString("Configuration")%></a></li>
        <li><a href="#dnnConfigMerge"><%=LocalizeString("Merge")%></a></li>
    </ul>

    <div id="dnnConfigFiles" class="dnnClear">
        <fieldset>
            <div class="dnnFormItem dnnSeparatorPanel">
                <dnn:Label ID="plConfig" runat="server" ControlName="ddlConfig" />
                <dnn:DnnComboBox runat="server" ID="ddlConfig" AutoPostBack="True" />
            </div>
            <div class="dnnFormItem">
                <%--<dnn:Label ID="fileLabel" runat="server" ControlName="txtConfiguration" />--%>
                <asp:TextBox ID="txtConfiguration" runat="server" TextMode="MultiLine" Rows="20" Columns="75" EnableViewState="True" Enabled="false" />
            </div>
            <ul class="dnnActions dnnClear">
                <li>
                    <asp:LinkButton ID="cmdSave" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdSave" /></li>
            </ul>
        </fieldset>
    </div>
    <div id="dnnConfigMerge" class="dnnClear">
        <fieldset>
            <div class="dnnFormItem dnnSeparatorPanel">
                <dnn:Label ID="plScript" runat="server" ControlName="uplScript" Suffix="" />
                <asp:FileUpload ID="uplScript" runat="server" />
                <asp:LinkButton ID="cmdUpload" resourcekey="cmdUpload" EnableViewState="False" CssClass="dnnSecondaryAction" runat="server" />
            </div>
            <div class="dnnFormItem">
                <%--<dnn:Label ID="scriptLabel" runat="server" ControlName="txtScript" />--%>
                <asp:TextBox ID="txtScript" runat="server" TextMode="MultiLine" Rows="20" Columns="75" EnableViewState="False" />
            </div>
            <ul class="dnnActions dnnClear">
                <li>
                    <asp:LinkButton ID="cmdExecute" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdExecute" /></li>
            </ul>
        </fieldset>
    </div>
</div>
<asp:Label ID="lblMessage" runat="server" CssClass="NormalRed" EnableViewState="False" />
<script type="text/javascript">
	jQuery(function($) {
		$('#<%=cmdSave.ClientID%>, #<%= cmdExecute.ClientID %>').dnnConfirm({
			text: '<%= ConfirmText %>',
			yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
			noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
			title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
		});

	    $('#dnnConfigManager').dnnTabs();

	    var configeditor = CodeMirror.fromTextArea($("textarea[id$='txtConfiguration']")[0], {
	        lineNumbers: true,
	        matchBrackets: true,
	        lineWrapping: true,
	        indentWithTabs: true,
	        mode: 'application/xml'
	    });
	    var mergeeditor = CodeMirror.fromTextArea($("textarea[id$='txtScript']")[0], {
	        lineNumbers: true,
	        matchBrackets: true,
	        lineWrapping: true,
	        indentWithTabs: true,
	        mode: 'application/xml'
	    });

	});
</script>