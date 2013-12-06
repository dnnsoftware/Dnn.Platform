<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Modules.ViewSource" CodeFile="viewsource.ascx.cs" %>

<%@ Register Assembly="DotnetNuke" Namespace="DotNetNuke.UI.WebControls" TagPrefix="dnn" %>

<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>

<%-- Custom CSS Registration --%>
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/lib/codemirror.css" />

<%-- Custom JavaScript Registration --%>
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/lib/codemirror.js" Priority="101" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/sql/sql.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/clike/clike.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/vb/vb.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/xml/xml.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/javascript/javascript.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/css/css.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/htmlmixed/htmlmixed.js" Priority="103" />

<div id="viewSourceForm" class="dnnForm dnnViewSource dnnClear">

    <ul class="dnnAdminTabNav dnnClear">
        <li><a href="#rbEdit"><%=Localization.GetString("EditFile.Text", LocalResourceFile)%></a></li>
        <li><a href="#rbAdd"><%=Localization.GetString("AddFile.Text", LocalResourceFile)%></a></li>
    </ul>
    <div class="rbEdit dnnClear" id="rbEdit">
        <fieldset>

            <div class="dnnFormItem">

                <dnn:Label ID="plFile" runat="Server" />

                <asp:DropDownList ID="cboFile" runat="server" AutoPostBack="true" />

            </div>

            <div class="dnnFormItem">

                <dnn:Label ID="plSource" ControlName="txtSource" runat="server" />
                <asp:Label ID="lblPath" runat="server" />
            </div>
            <div>
                <asp:TextBox ID="txtSource" runat="server" TextMode="MultiLine" Rows="30" Columns="140" />

            </div>

        </fieldset>

        <ul class="dnnActions dnnClear">

            <li>
                <asp:LinkButton ID="cmdUpdate" resourcekey="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" /></li>

            <li>
                <asp:LinkButton ID="cmdConfigure" resourcekey="cmdConfigure" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" /></li>

            <li>
                <asp:LinkButton ID="cmdPackage" resourcekey="cmdPackage" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" /></li>

            <li>
                <asp:HyperLink ID="cmdCancel1" resourcekey="cmdCancel" runat="server" CssClass="dnnSecondaryAction" causesvalidation="False" /></li>

        </ul>

    </div>
    <div class="rbAdd dnnClear" id="rbAdd">
        <fieldset>

            <div class="dnnFormItem">

                <dnn:Label ID="plLanguage" ControlName="optLanguage" runat="server" />

                <asp:RadioButtonList ID="optLanguage" CssClass="dnnFormRadioButtons" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" />

            </div>
            <div class="dnnFormItem">

                <dnn:Label ID="plTemplate" ControlName="cboTemplate" runat="server" />

                <asp:DropDownList ID="cboTemplate" runat="server" AutoPostBack="True" />
            </div>
            <div class="dnnFormItem">

                <dnn:Label ID="plControl" ControlName="txtControl" runat="server" />

                <asp:TextBox ID="txtControl" runat="server" />

            </div>

            <div class="dnnFormItem">

                <dnn:Label ID="plType" ControlName="cboType" runat="server" />

                <div class="dnnLeft">
                    <asp:RadioButtonList ID="cboType" CssClass="dnnFormRadioButtons" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                        <asp:ListItem resourcekey="View" Value="View" Text="View" />
                        <asp:ListItem resourcekey="Edit" Value="Edit" Text="Edit" Selected="True" />
                    </asp:RadioButtonList>
                </div>
            </div>

        </fieldset>

        <ul class="dnnActions dnnClear">

            <li>
                <asp:LinkButton ID="cmdCreate" resourcekey="cmdCreate" runat="server" CssClass="dnnPrimaryAction" /></li>

            <li>
                <asp:HyperLink ID="cmdCancel2" resourcekey="cmdCancel" runat="server" CssClass="dnnSecondaryAction" causesvalidation="False" /></li>

        </ul>

        <div class="dnnFormItem">

            <asp:Label ID="lblDescription" runat="server" />
        </div>
    </div>
</div>
<script type="text/javascript">

    jQuery(function ($) {
        var mimeType = dnn.getVar('mimeType') || "text/html";

        var setupModule = function () {
            $('#viewSourceForm').dnnTabs();
        };

        setupModule();

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {

            // note that this will fire when _any_ UpdatePanel is triggered,
            // which may or may not cause an issue
            setupModule();

        });
        
        CodeMirror.fromTextArea($("textarea[id$='txtSource']")[0], {
            lineNumbers: true,
            matchBrackets: true,
            lineWrapping: true,
            mode: mimeType
        });
    });

</script>
