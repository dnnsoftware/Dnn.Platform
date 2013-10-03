<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.SQL.SQL" CodeFile="SQL.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnSQLModule dnnClear" id="dnnSQLModule">
    <fieldset>
        <div class="dnnFormItem">
            <dnn:Label ID="lblSavedQuery" runat="server" ControlName="ddlSavedQuery" />
            <asp:DropDownList ID="ddlSavedQuery" runat="server" DataTextField="Name" DataValueField="QueryId" AutoPostBack="true"></asp:DropDownList>
            <asp:ImageButton ID="btDelete" resourcekey="btDelete" runat="server" Visible="false" ImageUrl="~/icons/sigma/Delete_16X16_Standard.png" />
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
                <asp:ImageButton ID="btSave" resourcekey="btSaveQuery" runat="server" ImageUrl="~/icons/sigma/Save_16X16_Standard.png" ValidationGroup="Script" />
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
                    <asp:GridView ID="gvResults" runat="server" AutoGenerateColumns="true">
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
        $('#<%=btSave.ClientID%>').bind("click", function () {
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
            width: 400,
            height: 250,
            title: '<%=LocalizeString("SaveDialogTitle")%>'
        }).parent().appendTo(jQuery('form:first'));;

        $('.dnnResults table').each(function (index, element) {
            var query = $(this).attr("title");
            var oTable = $(this).dataTable(
            {
                "sScrollX": "100%",
                "sPaginationType": "full_numbers",
                "aLengthMenu": [
                        [10, 25, 50, 100, -1],
                        [10, 25, 50, 100, "<%=LocalizeString("AllRows")%>"]
                ],
                "iDisplayLength": -1,
                "sDom": 'T<"clear">lfrtip',
                "oTableTools": {
                    "sSwfPath": "/dnn_platform/desktopmodules/admin/sql/plugins/datatables/swf/copy_csv_xls_pdf.swf",
                    "aButtons": [
                        {
                            "sExtends": "copy",
                            "sButtonText": "<img src='/dnn_platform/icons/sigma/CheckList_16X16_Gray.png'/>"
                        },
                        {
                            "sExtends": "csv",
                            "sTitle": query,
                            "sButtonText": "<img src='/dnn_platform/icons/sigma/FileDownload_16x16_Black.png'/>"
                        },
                        {
                            "sExtends": "xls",
                            "sTitle": query,
                            "sButtonText": "<img src='/dnn_platform/icons/sigma/ExtXlsx_16X16_Standard.png'/>"
                        },
                        {
                            "sExtends": "pdf",
                            "sTitle": query,
                            "sPdfOrientation": "landscape",
                            "sButtonText": "<img src='/dnn_platform/icons/sigma/ExtPdf_16X16_Standard.png'/>"
                        }
                    ]
                }
            });
            //new FixedHeader(oTable);
            new FixedColumns(oTable);
        });

        $('#<%=pnlResults.ClientID%>').dnnTabs();

    });
</script>
