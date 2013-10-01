<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.SQL.SQL" CodeFile="SQL.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnSQLModule dnnClear" id="dnnSQLModule">
    <fieldset>
        <div class="dnnFormItem">
            <dnn:Label ID="lblSavedQuery" runat="server" ControlName="ddlSavedQuery" />
            <asp:DropDownList ID="ddlSavedQuery" runat="server" DataTextField="Name" DataValueField="QueryId" AutoPostBack="true"></asp:DropDownList>
            <asp:LinkButton ID="lnkDelete" resourcekey="lnkDelete" runat="server" CssClass="dnnSecondaryAction" Visible="false" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="plConnection" runat="server" ControlName="ddlConnection" />
            <asp:DropDownList ID="ddlConnection" runat="server"></asp:DropDownList>
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="scriptLabel" runat="server" ControlName="txtQuery" CssClass="dnnFormRequired" />
            <asp:RequiredFieldValidator ID="valText" runat="server" CssClass="dnnFormMessage dnnFormError" resourcekey="NoScript" ControlToValidate="txtQuery" ValidationGroup="Script"></asp:RequiredFieldValidator>
        </div>
        <div class="dnnClear">
            <asp:TextBox ID="txtQuery" runat="server" TextMode="MultiLine" Rows="10" Width="100%" ValidationGroup="Script" />
        </div>
        <div class="dnnFormItem">
            <dnn:Label ID="plSqlScript" runat="server" ControlName="uplSqlScript" />
            <asp:FileUpload ID="uplSqlScript" runat="server" />
            <asp:LinkButton ID="cmdUpload" resourcekey="cmdUpload" EnableViewState="False" runat="server" CssClass="dnnSecondaryAction" />
        </div>
        <div runat="server" id="errorRow" visible="false" enableviewstate="false">
            <div class="dnnFormItem">
                <dnn:Label ID="errorLabel" runat="server" ControlName="txtError" />
            </div>
            <div class="dnnClear">
                <asp:TextBox ID="txtError" runat="server" TextMode="MultiLine" Width="100%" Rows="10" EnableViewState="False" Wrap="False" ReadOnly="true" />
            </div>
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdExecute" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdExecute" ValidationGroup="Script" /></li>
        <li>
            <asp:LinkButton ID="lnkSaveQuery" runat="server" CssClass="dnnSecondaryAction" resourcekey="lnkSaveQuery" ValidationGroup="Script" /></li>
    </ul>
    <asp:Repeater ID="rptResults" runat="server" EnableViewState="False">
        <ItemTemplate>
            <div class="dnnFormItem dnnResults">
                <img class="imgCopy" runat="server" src="~/images/copy.gif" title='<%#LocalizeString("CopyToClipboard") %>' alt='<%#LocalizeString("CopyToClipboard") %>' />
                <asp:GridView ID="grdResults" runat="server" AutoGenerateColumns="true" CssClass="dnnGrid">
                    <HeaderStyle CssClass="dnnGridHeader" />
                    <RowStyle CssClass="dnnGridItem" />
                    <AlternatingRowStyle CssClass="dnnGridAltItem" />
                    <EmptyDataTemplate>
                        <asp:Label ID="lblNoData" runat="server" resourcekey="lblNoData"></asp:Label>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </ItemTemplate>
    </asp:Repeater>
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
        $('#<%=lnkSaveQuery.ClientID%>').bind("click", function () {
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
            resizable: false,
            dialogClass: 'dnnFormPopup dnnClear',
            width: 400,
            height: 250,
            title: '<%=LocalizeString("SaveDialogTitle")%>'
        }).parent().appendTo(jQuery('form:first'));;

        $('img.imgCopy').zclip({
            path: '<%=GetClipboardPath()%>',
            copy: function () { return $(this).siblings('div').html(); }
        });

    });
</script>
