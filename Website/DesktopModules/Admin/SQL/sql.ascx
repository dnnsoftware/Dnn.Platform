<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.SQL.SQL" CodeFile="SQL.ascx.cs" %>
<%@ Import Namespace="DotNetNuke.Entities.Icons" %>

<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<%-- Custom CSS Registration --%>
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/lib/codemirror.css" />
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/theme/dnn-sql.css" />

<%-- Custom JavaScript Registration --%>
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/lib/codemirror.js" Priority="1" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/sql/sql.js" Priority="2" />
<div class="dnnForm dnnSQLModule dnnClear" id="dnnSQLModule">
    <div class="sqlQuery">
        <fieldset>
            <div class="dnnFormItem">
                <dnn:Label ID="lblSavedQuery" runat="server" ControlName="ddlSavedQuery" />
                <asp:DropDownList ID="ddlSavedQuery" runat="server" DataTextField="Name" DataValueField="QueryId" AutoPostBack="true"></asp:DropDownList>
                <button id="btDelete" class="singleButton" resourceKey="btDelete" runat="server" >
                    <span class="saveButton" style='background-image:url(<%= IconController.IconURL("Delete", "16x16", "Gray")%>)'><%= Localization.GetString("btDelete", LocalResourceFile) %></span>
                </button>
                <div class="dnnRight">
                    <asp:FileUpload ID="uplSqlScript" runat="server" />
                    <asp:LinkButton ID="cmdUpload" resourcekey="cmdUpload" EnableViewState="False" runat="server" CssClass="dnnSecondaryAction" />
                </div>
            </div>
            <hr />
            <div class="dnnFormItem">
                <dnn:Label ID="plConnection" runat="server" ControlName="ddlConnection" />
                <asp:DropDownList ID="ddlConnection" runat="server"></asp:DropDownList>
                <div class="dnnRight">
                    <button id="btSave" class="singleButton" title='<%= Localization.GetString("btSaveQuery", LocalResourceFile) %>' ><span class="saveButton" style='background-image:url(<%= IconController.IconURL("Save", "16x16", "Gray")%>)'><%= Localization.GetString("btSaveQuery", LocalResourceFile) %></span></button>
                </div>
            </div>
            <div class="dnnFormItem">
                <asp:RequiredFieldValidator ID="valText" runat="server" CssClass="dnnFormMessage dnnFormError" resourcekey="NoScript" ControlToValidate="txtQuery" ValidationGroup="Script"></asp:RequiredFieldValidator>
            </div>
            <div>
                <asp:TextBox ID="txtQuery" runat="server" TextMode="MultiLine" Rows="10" Width="100%" ValidationGroup="Script" />
            </div>
        </fieldset>
        <ul class="dnnActions dnnClear">
            <li>
                <asp:LinkButton ID="cmdExecute" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdExecute" ValidationGroup="Script" /></li>
        </ul>
    </div>
    <asp:Panel ID="pnlError" runat="server" Visible="false">
        <div class="dnnFormItem">
            <dnn:Label ID="errorLabel" runat="server" ControlName="txtError" />
        </div>
        <div class="dnnClear">
            <asp:TextBox ID="txtError" runat="server" TextMode="MultiLine" Width="100%" Rows="10" EnableViewState="False" Wrap="False" ReadOnly="true" />
        </div>
    </asp:Panel>
    <asp:Panel ID="pnlResults" runat="server" Visible="false">
        <ul class="dnnAdminTabNav">
            <asp:PlaceHolder ID="plTabs" runat="server"></asp:PlaceHolder>
        </ul>
        <asp:Repeater ID="rptResults" runat="server" EnableViewState="False">
            <ItemTemplate>
                <div class="dnnResults" id='result_<%#Container.ItemIndex +1 %>'>
                    <asp:Label ID="lblRows" runat="server" CssClass="NormalBold"></asp:Label>
                    <asp:GridView ID="gvResults" runat="server" AutoGenerateColumns="true" CssClass="dnnTableDisplay" HeaderStyle-CssClass="dnnGridHeader">
                    </asp:GridView>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </asp:Panel>

    <div id="dialog-save" class="dnnDialog">
        <div class="dnnFormItem">
            <dnn:Label ID="lblName" runat="server" ControlName="txtName" CssClass="dnnFormRequired" />
            <asp:TextBox ID="txtName" runat="server" Width="200px" ValidationGroup="Save" />
            <asp:RequiredFieldValidator ID="valName" runat="server" CssClass="dnnFormMessage dnnFormError" resourcekey="NoName" ControlToValidate="txtName" ValidationGroup="Save"></asp:RequiredFieldValidator>
            <asp:LinkButton ID="lnkSave" runat="server" CssClass="dnnPrimaryAction" resourcekey="lnkSave" ValidationGroup="Save" />
            <a href="#" id="btcancel" class="dnnSecondaryAction"><%=LocalizeString("cmdCancel") %></a>
        </div>
    </div>
</div>

<script>
    $(function () {
        $('#btSave').bind("click", function () {
            if ($('#<%=txtQuery.ClientID%>').val() == "")
                return false;

            var active = $('#<%=ddlSavedQuery.ClientID%> option:selected').text();
            if ($('#<%=ddlSavedQuery.ClientID%>').val() == "")
                active = "";
            $('#<%=txtName.ClientID%>').val(active);
            $("#dialog-save").dialog('open');
            return false;
        });
        $('#btcancel').bind("click", function () {
            $("#dialog-save").dialog('close');
            return false;
        });

        $("#dialog-save").dialog({
            modal: true,
            autoOpen: false,
            resizable: true,
            dialogClass: 'dnnFormPopup dnnClear',
            width: 420,
            height: 280,
            title: '<%=LocalizeString("SaveDialogTitle")%>'
        }).parent().appendTo(jQuery('form:first'));;

        $('.dnnResults table').each(function (index, element) {
            var query = $(this).attr("title");
            var oTable = $(this).dataTable(
            {
                "aaSorting": [],
                "sScrollX": "100%",
                "sPaginationType": "full_numbers",
                "aLengthMenu": [
                        [10, 25, 50, 100, -1],
                        [10, 25, 50, 100, "<%=LocalizeString("AllRows")%>"]
                ],
                "iDisplayLength": -1,
                "sDom": '<"dnnClear dnnSeparatorPanel"T>lfrt<"dnnClear dnnTableFooter"ip>',
                "oTableTools": {
                    "sSwfPath": '<%=ResolveUrl("~/desktopmodules/admin/sql/plugins/datatables/swf/copy_csv_xls_pdf.swf") %>',
                    "aButtons": [
                        {
                            "sExtends": "copy",
                            "sToolTip": "<%=LocalizeString("CopyButtonAlt")%>",
                            "sButtonText": "<img src='<%=ResolveUrl("~/icons/sigma/CheckList_16X16_Gray.png") %>'/>",
                            "fnComplete": function (nButton, oConfig, oFlash, sFlash) {
                                $.dnnAlert({
                                    title: "<%=LocalizeString("CopyTitle")%>",
                                    text: "<%=LocalizeString("CopyText")%>"
                                });
                            }
                        },
                        {
                            "sExtends": "csv",
                            "sToolTip": "<%=LocalizeString("CSVButtonAlt")%>",
                            "sTitle": query,
                            "sButtonText": "<img src='<%=ResolveUrl("~/icons/sigma/FileDownload_16x16_Gray.png") %>'/>"
                        },
                        {
                            "sExtends": "xls",
                            "sToolTip": "<%=LocalizeString("XLSButtonAlt")%>",
                            "sTitle": query,
                            "sButtonText": "<img src='<%=ResolveUrl("~/icons/sigma/ExtXlsx_16X16_Gray.png") %>'/>"
                        },
                        {
                            "sExtends": "pdf",
                            "sToolTip": "<%=LocalizeString("PDFButtonAlt")%>",
                            "sTitle": query,
                            "sPdfOrientation": "landscape",
                            "sButtonText": "<img src='<%=ResolveUrl("~/icons/sigma/ExtPdf_16X16_Gray.png") %>'/>"
                        }<%--,
                        {
                            "sExtends": "text",
                            "sToolTip": "<%=LocalizeString("PopupButtonAlt")%>",
                            "sButtonText": "<img src='/dnn_platform/icons/sigma/UploadFiles_16x16_Gray.png'/>",
                            "fnClick": function (nButton, oConfig, oFlash) {
                                alert('open popup');
                            }
                        }--%>
                    ]
                },
                oLanguage: {
                    "sInfo": "<%=Localization.GetSafeJSString(LocalizeString("sInfo"))%>",
                    "sInfoFiltered": "<%=Localization.GetSafeJSString(LocalizeString("sInfoFiltered"))%>",
                    "sInfoEmpty": "<%=Localization.GetSafeJSString(LocalizeString("sInfoEmpty"))%>",
                    "sLengthMenu": "<%=Localization.GetSafeJSString(LocalizeString("sLengthMenu"))%>",
                    "sLoadingRecords": "<%=Localization.GetSafeJSString(LocalizeString("sLoadingRecords"))%>",
                    "sProcessing": "<%=Localization.GetSafeJSString(LocalizeString("sProcessing"))%>",
                    "sSearch": "<%=Localization.GetSafeJSString(LocalizeString("sSearch"))%>",
                    "oPaginate": {
                        "sFirst": "<%=Localization.GetSafeJSString(LocalizeString("sFirst"))%>",
                        "sLast": "<%=Localization.GetSafeJSString(LocalizeString("sLast"))%>",
                        "sNext": "<%=Localization.GetSafeJSString(LocalizeString("sNext"))%>",
                        "sPrevious": "<%=Localization.GetSafeJSString(LocalizeString("sPrevious"))%>"
                    }
                }
            });
            //new FixedHeader(oTable); 
            //new FixedColumns(oTable);
        });

        var resultsPane = $('#<%=pnlResults.ClientID%>');
        resultsPane.dnnTabs();
        var originActivateEvent = resultsPane.tabs("option", "activate");
        resultsPane.tabs("option", "activate", function (ui, event) {
            originActivateEvent.call(this, ui, event);
		    var tools = TableTools.fnGetMasters();
		    for (var i = 0; i < tools.length; i++) {
			    tools[i].fnResizeButtons();
		    }
	    });

        var editor = CodeMirror.fromTextArea($("textarea[id$='txtQuery']")[0], {
            lineNumbers: true,
            matchBrackets: true,
            lineWrapping: true,
            indentWithTabs: true,
            theme: 'dnn-sql light',
            mode: 'text/x-sql'            
        });

        editor.on("blur", function (cm) {
            cm.save();
            return true;
        });

    });
</script>
