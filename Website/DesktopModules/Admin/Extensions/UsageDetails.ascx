<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.UsageDetails" CodeFile="UsageDetails.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls"%>                
<h2 class="dnnFormSectionHead"><asp:Literal ID="lblTitle" runat="server" /></h2>
<style type="text/css">
.dnnGrid{
    width: 100%;
    border: 1px solid #c9c9c9;
}
.dnnGridHeader th {
    border-bottom: 1px solid #c9c9c9;
    border-right: 1px solid #c9c9c9;
    background: #f0f2f1;
    background: -moz-linear-gradient(top, #fff 0%, #f0f2f1 100%); /* FF3.6+ */
    background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,#fff), color-stop(100%,#f0f2f1)); /* Chrome,Safari4+ */
    background: -webkit-linear-gradient(top, #fff 0%,#f0f2f1 100%); /* Chrome10+,Safari5.1+ */
    background: linear-gradient(top, #fff 0%,#f0f2f1 100%); /* W3C */
    padding: 6px 0 6px 12px;
}
.dnnGrid td {
    border-right: 1px solid #c9c9c9;
    padding: 6px;
}

.dnnGrid td:hover {
    background-color: #e8f1fa;
}
</style>

<asp:UpdatePanel ID="PnlUsageDetails" runat="server" ChildrenAsTriggers="true">
    <ContentTemplate>
        <asp:Panel id="tblFilterUsage" runat="server" CssClass="dnnFormItem">
            <dnn:Label ID="lblFilterUsageList" runat="server" ControlName="FilterUsageList" />
            <dnn:DnnComboBox ID="FilterUsageList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterUsageList_SelectedIndexChanged" />
        </asp:Panel>
     
        <asp:Label ID="UsageListMsg" runat="server" />
        <asp:GridView ID="UsageList" runat="server" AutoGenerateColumns="false" PageSize="100" AllowPaging="true" GridLines="None" CellPadding="0" EnableViewState="False" CssClass="dnnGrid">
            <HeaderStyle Wrap="False" CssClass="dnnGridHeader"/>
            <PagerSettings Mode="NextPreviousFirstLast" />         
            <Columns>
                <asp:TemplateField HeaderText="Page">
                    <ItemTemplate>
                        <%#GetFormattedLink(Container.DataItem)%>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </ContentTemplate>
</asp:UpdatePanel>