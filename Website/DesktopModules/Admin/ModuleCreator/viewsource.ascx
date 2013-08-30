<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Modules.ViewSource" CodeFile="viewsource.ascx.cs" %>

<%@ Register Assembly="DotnetNuke" Namespace="DotNetNuke.UI.WebControls" TagPrefix="dnn" %>

<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>

<%-- Custom CSS Registration --%>
<dnn:DnnCssInclude runat="server" FilePath="~/DesktopModules/Admin/ModuleCreator/Components/CodeEditor/lib/codemirror.css" />

<%-- Custom JavaScript Registration --%>
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/Admin/ModuleCreator/Components/CodeEditor/lib/codemirror.js" Priority="1" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/Admin/ModuleCreator/Components/CodeEditor/mode/clike/clike.js" Priority="2" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/Admin/ModuleCreator/js/ModuleCreator.js" Priority="2" />

<script>
    jQuery(function ($) {
        CodeMirror.fromTextArea($("textarea[id$='txtSource']")[0], {
            lineNumbers: true,
            matchBrackets: true,
            mode: "text/x-csharp"
        });

        //var snippetEditor = CodeMirror.fromTextArea($("textarea[id$='SnippetView']")[0], {
        //    lineNumbers: true,
        //    matchBrackets: true,
        //    mode: "text/x-csharp"
        //});

    });
</script>

<div id="viewSourceForm" class="dnnForm dnnViewSource dnnClear">

    <ul class="dnnAdminTabNav dnnClear">
        <li><a href="#rbEdit"><%=Localization.GetString("Edit.Text", LocalResourceFile)%></a></li>
        <li><a href="#rbSnippet"><%=Localization.GetString("Snippet.Text", LocalResourceFile)%></a></li>
        <li><a href="#rbAdd"><%=Localization.GetString("Add.Text", LocalResourceFile)%></a></li>
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
    <div class="dnnClear" id="rbSnippet">
        <div class="snippetTree">
            <dnnweb:DnnTreeView ID="SnippetTree" runat="server" Skin="Vista" CssClass="dnnModuledigitalAssetsTreeView"
                OnClientNodeClicking="treeViewOnNodeClicking">
            </dnnweb:DnnTreeView>
        </div>
        <div>
            <asp:TextBox runat="server" ID="SnippetView" TextMode="MultiLine" Width="80%" Rows="24"></asp:TextBox>
        </div>
        <ul class="dnnActions dnnClear">
            <li>
                <asp:Button ID="cmdSaveSnippet" resourcekey="cmdSave" runat="server" CssClass="dnnPrimaryAction" /></li>
            <li>
                <asp:LinkButton ID="cmdSaveAsSnippet" resourcekey="cmdSaveAs" runat="server" CssClass="dnnSecondaryAction" /></li>
            <li>
                <asp:LinkButton ID="cmdDeleteSnippet" resourcekey="cmdDelete" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" /></li>
        </ul>
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
        var yesText = '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>';
        var noText = '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>';
        $('#<%= cmdDeleteSnippet.ClientID %>').dnnConfirm({
            text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("DeleteSnippet")) %>',
            yesText: yesText,
            noText: noText,
            title: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("DeleteSnippet")) %>',
        			    isButton: true
        });

        $("#<%=cmdSaveSnippet.ClientID%>").click(saveSnippet);
        $("#<%=cmdSaveAsSnippet.ClientID%>").click(saveAsSnippet);
        $("#<%=cmdDeleteSnippet.ClientID%>").click(deleteSnippet);

        function deleteSnippet(e, isTrigger) {
            e.preventDefault();
            if ($(this).hasClass("dnnDisabled")) return;
            if (isTrigger) {
                var tree = $find('<% =SnippetTree.ClientID %>');
                var node = tree.get_selectedNode();
                var nodeName = 'Snippets\\' + node.get_text();
                var currentObject = node;
                currentObject = currentObject.get_parent();
                while (currentObject != null) {
                    if (currentObject.get_parent() != null) {
                        nodeName = currentObject.get_text() + "\\" + nodeName;
                    }
                    currentObject = currentObject.get_parent();
                }
                $.ajax({
                    type: "POST",
                    beforeSend: sf.setModuleHeaders,
                    data: { Name: nodeName, Content:'' },
                    url: sf.getServiceRoot("Admin/ModuleCreator") + "ModuleCreator/DeleteSnippet"
                }).done(function () {
                }).fail(function (xhr, result, status) {
                    alert("Uh-oh, something broke: " + status);
                });
                tree.trackChanges();
                node.get_parent().get_nodes().remove(node);
                tree.commitChanges();
                var snippetView = document.getElementById('<% =SnippetView.ClientID %>');
                snippetView.innerHTML = '';
            }
        }

        function saveAsSnippet(e) {
            e.preventDefault();
            if ($(this).hasClass("dnnDisabled")) return;
            var fileName = prompt("New snippet name:", "Event");
            save(fileName);
            var tree = $find('<% =SnippetTree.ClientID %>');
            var node = tree.get_selectedNode();
            var parent = node.get_parent();
            var newNode = new Telerik.Web.UI.RadTreeNode();
            newNode.set_text(fileName);
            var snippetView = document.getElementById('<% =SnippetView.ClientID %>');
            newNode.set_value(snippetView.value);
            tree.trackChanges();
            parent.get_nodes().add(newNode);
            newNode.select();
            tree.commitChanges();
        }

        function saveSnippet(e) {
            e.preventDefault();
            if ($(this).hasClass("dnnDisabled")) return;
            var tree = $find('<% =SnippetTree.ClientID %>');
            var node = tree.get_selectedNode();
            save(node.get_text());
        }

        function save(fileName) {
            var tree = $find('<% =SnippetTree.ClientID %>');
            var node = tree.get_selectedNode();
            var currentObject = node;
            var nodeName = 'Snippets\\' + fileName;
            currentObject = currentObject.get_parent();
            while (currentObject != null) {
                if (currentObject.get_parent() != null) {
                    nodeName = currentObject.get_text() + "\\" + nodeName;
                }
                currentObject = currentObject.get_parent();
            }
            var sf = $.ServicesFramework(<% =ModuleId%>);
            var snippetView = document.getElementById('<% =SnippetView.ClientID %>');
            $.ajax({
                type: "POST",
                beforeSend: sf.setModuleHeaders,
                data: { Name: nodeName, Content: snippetView.value },
                url: sf.getServiceRoot("Admin/ModuleCreator") + "ModuleCreator/SaveSnippet"
            }).done(function () {
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });
        }
    });
    function loadLanguages(data) {

        var tree = $find('<% =SnippetTree.ClientID %>');

        for (var key in data) {
            if (data.hasOwnProperty(key)) {
                var node = new Telerik.Web.UI.RadTreeNode();
                node.set_text(data[key].Name);
                tree.trackChanges();
                tree.get_nodes().add(node);
                tree.commitChanges();
            }
        }
    }

    var sf = $.ServicesFramework(<% =ModuleId%>);
    $.ajax({
        type: "POST",
        beforeSend: sf.setModuleHeaders,
        data: '',
        url: sf.getServiceRoot("Admin/ModuleCreator") + "ModuleCreator/GetLanguages"
    }).done(function (data) {
        loadLanguages(data);
    }).fail(function (xhr, result, status) {
        alert("Uh-oh, something broke: " + status);
    });

    function treeViewOnNodeClicking(sender, args) {
        var node = args.get_node();
        
        $("#<%=cmdSaveSnippet.ClientID%>").addClass("dnnDisabled");
        $("#<%=cmdSaveAsSnippet.ClientID%>").addClass("dnnDisabled");
        $("#<%=cmdDeleteSnippet.ClientID%>").addClass("dnnDisabled"); 
        if (node.get_level() == 0) {
            var sf = $.ServicesFramework(<% =ModuleId%>);
            $.ajax({
                type: "POST",
                beforeSend: sf.setModuleHeaders,
                data: { Name: node.get_text() },
                url: sf.getServiceRoot("Admin/ModuleCreator") + "ModuleCreator/GetTemplates"
            }).done(function (data) {
                loadTemplates(node, data);
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });
        }
        if (node.get_level() == 1) {

            var nodeName = node.get_text();
            var currentObject = node.get_parent();
            while (currentObject != null) {
                if (currentObject.get_parent() != null) {
                    nodeName = currentObject.get_text() + "/" + nodeName;
                }
                currentObject = currentObject.get_parent();
            }
            var sf = $.ServicesFramework(<% =ModuleId%>);
            $.ajax({
                type: "POST",
                beforeSend: sf.setModuleHeaders,
                data: { Name: nodeName },
                url: sf.getServiceRoot("Admin/ModuleCreator") + "ModuleCreator/GetSnippets"
            }).done(function (data) {
                loadSnippets(node, data);
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });
        }
        if (node.get_level() == 2) {
            var snippet = node.get_value();
            var snippetView = document.getElementById('<% =SnippetView.ClientID %>');
            snippetView.innerHTML = snippet;
            $("#<%=cmdSaveSnippet.ClientID%>").removeClass("dnnDisabled");
            $("#<%=cmdSaveAsSnippet.ClientID%>").removeClass("dnnDisabled");
            $("#<%=cmdDeleteSnippet.ClientID%>").removeClass("dnnDisabled");
        }
    }
    function loadTemplates(parentNode, data) {

        var tree = $find('<% =SnippetTree.ClientID %>');

        for (var key in data) {
            if (data.hasOwnProperty(key)) {
                var node = new Telerik.Web.UI.RadTreeNode();
                node.set_text(data[key].Name);
                tree.trackChanges();
                parentNode.get_nodes().add(node);
                tree.commitChanges();
            }
        }
        parentNode.expand();
    }

    function loadSnippets(parentNode, data) {

        var tree = $find('<% =SnippetTree.ClientID %>');

        for (var key in data) {
            if (data.hasOwnProperty(key)) {
                var node = new Telerik.Web.UI.RadTreeNode();
                node.set_text(data[key].Name);
                node.set_value(data[key].Content);
                tree.trackChanges();
                parentNode.get_nodes().add(node);
                tree.commitChanges();
            }
        }
        parentNode.expand();
    }

</script>

