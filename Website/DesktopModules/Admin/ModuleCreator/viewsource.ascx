<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Modules.ViewSource" CodeFile="viewsource.ascx.cs" %>

<%@ Register Assembly="DotnetNuke" Namespace="DotNetNuke.UI.WebControls" TagPrefix="dnn" %>

<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<%-- Custom CSS Registration --%>
<dnn:DnnCssInclude runat="server" FilePath="~/DesktopModules/ModuleCreator/Components/CodeEditor/lib/codemirror.css" />

<%-- Custom JavaScript Registration --%>
<dnn:dnnjsinclude runat="server" filepath="~/DesktopModules/ModuleCreator/Components/CodeEditor/lib/codemirror.js" priority="1" />
<dnn:dnnjsinclude runat="server" filepath="~/DesktopModules/ModuleCreator/Components/CodeEditor/mode/clike/clike.js" priority="2" />

<script>
    jQuery(function ($) {
        var editor = CodeMirror.fromTextArea($("textarea[id$='txtSource']")[0], {
            lineNumbers: true,
            matchBrackets: true,
            mode: "text/x-csharp"
        });
    });
</script>

<div id="viewSourceForm" class="dnnForm dnnViewSource dnnClear">

    <ul class="dnnAdminTabNav dnnClear">
        <li><a href="#rbEdit">Edit Control</a></li>
        <li><a href="#rbAdd">Add Control</a></li>
    </ul>
    <div class="rbEdit dnnClear" id="rbEdit">
        <fieldset>

	    <div class="dnnFormItem">

                <dnn:Label id="plFile" runat="Server" />

                <asp:DropDownList ID="cboFile" runat="server" AutoPostBack="true" />

            </div>

            <div class="dnnFormItem">

                <dnn:label id="plSource" controlname="txtSource" runat="server" />
                <asp:Label ID="lblPath" runat="server" />
            </div>
            <div>
                <asp:TextBox ID="txtSource" runat="server" TextMode="MultiLine" Rows="30" Columns="140" />

            </div>

        </fieldset>

        <ul class="dnnActions dnnClear">

            <li><asp:LinkButton id="cmdUpdate" resourcekey="cmdUpdate" runat="server" cssclass="dnnPrimaryAction" /></li>

            <li><asp:LinkButton id="cmdConfigure" resourcekey="cmdConfigure" runat="server" cssclass="dnnSecondaryAction" causesvalidation="False" /></li>

            <li><asp:LinkButton id="cmdPackage" resourcekey="cmdPackage" runat="server" cssclass="dnnSecondaryAction" causesvalidation="False" /></li>

            <li><asp:HyperLink id="cmdCancel1" resourcekey="cmdCancel" runat="server" cssclass="dnnSecondaryAction" causesvalidation="False" /></li>

        </ul>

    </div>
    <div class="rbAdd dnnClear" id="rbAdd">
        <fieldset>

            <div class="dnnFormItem">

                <dnn:label id="plLanguage" controlname="optLanguage" runat="server" />

	        <asp:RadioButtonList ID="optLanguage" CssClass="dnnFormRadioButtons" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostback="True" />

            </div> 
            <div class="dnnFormItem">

                <dnn:label id="plTemplate" controlname="cboTemplate" runat="server" />

                <asp:DropdownList ID="cboTemplate" runat="server" AutoPostback="True" />
            </div> 
            <div class="dnnFormItem">

                <dnn:label id="plControl" controlname="txtControl" runat="server" />

                <asp:TextBox ID="txtControl" runat="server" />

            </div>

            <div class="dnnFormItem">

                <dnn:label id="plType" controlname="cboType" runat="server" />

                <div class="dnnLeft">
                    <asp:RadioButtonList ID="cboType" CssClass="dnnFormRadioButtons" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                        <asp:ListItem resourcekey="View" Value="View" Text="View" />
                        <asp:ListItem resourcekey="Edit" Value="Edit" Text="Edit" Selected="True" />
                    </asp:RadioButtonList>
                </div>
            </div>

        </fieldset>

        <ul class="dnnActions dnnClear">

            <li><asp:LinkButton id="cmdCreate" resourcekey="cmdCreate" runat="server" cssclass="dnnPrimaryAction" /></li>

            <li><asp:HyperLink id="cmdCancel2" resourcekey="cmdCancel" runat="server" cssclass="dnnSecondaryAction" causesvalidation="False" /></li>

        </ul>

        <div class="dnnFormItem">

            <asp:Label ID="lblDescription" runat="server" />
        </div> 
    </div>
</div>
<script type="text/javascript">

    jQuery(function ($) {

        var setupModule = function () {

            $('#viewSourceForm').dnnTabs();

        };

        setupModule();

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {

            // note that this will fire when _any_ UpdatePanel is triggered,

            // which may or may not cause an issue

            setupModule();

        });

    });

</script>            

