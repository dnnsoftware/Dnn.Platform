<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.PurchasedExtensions" CodeFile="PurchasedExtensions.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<dnn:DnnAjaxLoadingPanel ID="loadingPanel" runat="server" Skin="Default">
</dnn:DnnAjaxLoadingPanel>
<dnn:DnnAjaxPanel ID="ajaxPanel" runat="server" LoadingPanelID="loadingPanel" RestoreOriginalRenderDelegate="false">
    <div class="dnnForm dnnPurchasedExtensions dnnClear" id="dnnPurchasedExtensions">
        <h2 class="dnnFormSectionHead"><asp:Label ID="lblTitle" runat="server"><% =LocalizeString("PurchasedTitle")%></asp:Label></h2>
        <div class="dnnFormMessage dnnFormInfo"><asp:Label ID="lblHelp" runat="server"><% =LocalizeString("PurchasedHelp")%></asp:Label></div>
        <div id="loginWarning" runat="server" class="dnnFormMessage dnnFormWarning" visible="false">
            <asp:Label ID="Label1" runat="server"><% =LocalizeString("SnowcoveredLogin")%></asp:Label>
        </div>
        <div id="error" runat="server" class="dnnFormMessage dnnFormWarning" visible="false">
            <asp:Label ID="Label2" runat="server"><% =LocalizeString("WebserviceFailure")%></asp:Label>
        </div>
        <asp:GridView ID="grdSnow" runat="server" CellPadding="0" CellSpacing="0" GridLines="None" AutoGenerateColumns="false"
            EnableViewState="False"  Width="100%" CssClass="dnnGrid">
            <HeaderStyle CssClass="dnnGridHeader" HorizontalAlign="Left"/>
            <RowStyle CssClass="dnnGridItem" HorizontalAlign="Left" />
            <AlternatingRowStyle CssClass="dnnGridAltItem" />
            <FooterStyle CssClass="dnnGridFooter" />
            <PagerStyle CssClass="dnnGridPager" />
            <Columns>
                <asp:BoundField DataField="Package" HtmlEncode="true" HeaderText="Package Name" ItemStyle-Font-Bold="true" />
                <asp:BoundField DataField="Filename" HtmlEncode="true" HeaderText="Filename" />
                <asp:BoundField DataField="Download" HtmlEncode="false" HeaderText="Download" />
                <asp:BoundField DataField="Deploy" HtmlEncode="false" HeaderText="Deploy" />
            </Columns>
            <EmptyDataTemplate>
                <div class="dnnFormMessage dnnFormWarning">
                    <%= LocalizeString("NoData") %></div>
            </EmptyDataTemplate>
        </asp:GridView>
        <ul class="dnnActions dnnClear">
            <li><asp:HyperLink ID="setupCredentials" runat="server" CssClass="dnnPrimaryAction" resourcekey="setupCredentials" /></li>
            <li><asp:LinkButton ID="fetchExtensions" runat="server" CssClass="dnnPrimaryAction" resourcekey="fetchExtensions" Visible="false" /></li>
            <li><asp:HyperLink ID="updateCredentials" runat="server" CssClass="dnnSecondary" resourcekey="updateCredentials" /></li>
        </ul>
    </div>
</dnn:DnnAjaxPanel>
