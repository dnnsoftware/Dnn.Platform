<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Host.IPFilters" CodeFile="IPFilters.ascx.cs" %>
<div id="dvRedirectionsGrid" runat="server" class="dnnGrid dnnRedirectionGrid">
    <div class="dnnTableHeader">
        <asp:HyperLink ID="cmdAddFilter" runat="server" resourcekey="cmdAdd" CssClass="dnnSecondaryAction dnnRight" />
        <div class="dnnClear"></div>
    </div>
    <asp:DataGrid ID="grdFilters" AutoGenerateColumns="false" Width="100%" CellPadding="2" GridLines="None" CssClass="dnnGrid" runat="server">
        <HeaderStyle CssClass="dnnGridHeader" VerticalAlign="Top" HorizontalAlign="Center" />
        <ItemStyle CssClass="dnnGridItem" HorizontalAlign="Center" Height="30" />
        <AlternatingItemStyle CssClass="dnnGridAltItem" />
        <EditItemStyle CssClass="dnnFormEditItem" />
        <SelectedItemStyle CssClass="dnnFormError" />
        <FooterStyle CssClass="dnnGridFooter" />
        <PagerStyle CssClass="dnnGridPager" />
        <Columns>
            <asp:TemplateColumn HeaderText="" HeaderStyle-Width="50px" ItemStyle-HorizontalAlign="Center">
                <ItemTemplate>
                    <asp:Image ID="Image2" runat="server" ImageUrl='<%#ConvertType(DataBinder.Eval(Container.DataItem, "RuleType")) %>' AlternateText='<%#ConvertTypeAlt(DataBinder.Eval(Container.DataItem, "RuleType")) %>' />
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn HeaderText="IPFilter" HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder">
                <ItemTemplate>
                    <%#ConvertCIDR(Eval("IPAddress").ToString(), Eval("Subnetmask").ToString()) %>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn HeaderText="Actions" HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder">
                <ItemTemplate>
                    <a href="<%#GetEditUrl(Eval("IPFilterID").ToString()) %>">
                        <img src="<%= ResolveUrl("~/icons/sigma/edit_16X16_standard.png") %>" alt="" />
                    </a>
                    <asp:LinkButton ID="btnDel" runat="server" CssClass="delete" CausesValidation="false" CommandArgument='<%#Eval("IPFilterID") %>' CommandName="Delete" OnClick="DeleteFilter">
                        <asp:Image ID="image1" runat="server" ImageUrl="~/icons/sigma/delete_16X16_standard.png" />
                    </asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateColumn>
        </Columns>
    </asp:DataGrid>
</div>
<script type="text/javascript">
    
    (function ($, sys) {
        
        var prm = sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(endRequestHandler);

        function endRequestHandler(sender, args) {
            initializeDeleteIPFilterConfirm();
        }

        function initializeDeleteIPFilterConfirm() {
            var yesText = '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
                noText = '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
                titleText = '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>';

            $("#<%= grdFilters.ClientID %> a.delete").dnnConfirm({
                text: '<%= Localization.GetSafeJSString("DeleteItem", LocalResourceFile) %>',
                yesText: yesText,
                noText: noText,
                title: titleText
            });
        }

        $(document).ready(function() {
            initializeDeleteIPFilterConfirm();
        });
    }(jQuery, Sys));
</script>
