<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Vendors.Banners" CodeFile="Banners.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div class="dnnForm dnnBanners dnnClear">
    <asp:datagrid id="grdBanners" runat="server" Width="100%" AutoGenerateColumns="false" EnableViewState="true" GridLines="None" CssClass="dnnBannersGrid">
        <headerstyle cssclass="dnnGridHeader" verticalalign="Top"/>
        <itemstyle cssclass="dnnGridItem" horizontalalign="Left" />
        <alternatingitemstyle cssclass="dnnGridAltItem" />
        <edititemstyle cssclass="dnnFormInput" />
        <selecteditemstyle cssclass="dnnFormError" />
        <footerstyle cssclass="dnnGridFooter" />
        <pagerstyle cssclass="dnnGridPager" />
        <Columns>
            <asp:TemplateColumn>
                <ItemStyle Width="20px"/>
                <ItemTemplate>
				    <asp:HyperLink NavigateUrl='<%# FormatURL("BannerId",DataBinder.Eval(Container.DataItem,"BannerId").ToString()) %>' runat="server" ID="Hyperlink1">					    
                        <dnn:DnnImage iconKey="edit" resourcekey="Edit" alternatetext="Edit" runat="server" id="Hyperlink1Image" />
				    </asp:HyperLink>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn DataField="BannerName" HeaderText="Banner"/>
            <asp:TemplateColumn HeaderText="Type">
                <ItemTemplate>
			        <asp:Label ID="lblType" Runat="server" Text='<%# DisplayType((int)DataBinder.Eval(Container.DataItem, "BannerTypeId")) %>' />
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn DataField="GroupName" HeaderText="Group"/>
            <asp:BoundColumn DataField="Impressions" HeaderText="Impressions"/>
            <asp:BoundColumn DataField="CPM" HeaderText="CPM" DataFormatString="{0:#,##0.00}"/>
            <asp:BoundColumn DataField="Views" HeaderText="Views"/>
            <asp:BoundColumn DataField="ClickThroughs" HeaderText="Clicks"/>
            <asp:TemplateColumn HeaderText="Start">
                <ItemTemplate>
		            <asp:Label ID="lblStartDate" Runat="server" Text='<%# DisplayDate((DateTime)DataBinder.Eval(Container.DataItem, "StartDate")) %>' />
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn HeaderText="End">
                <ItemTemplate>
			        <asp:Label ID="lblEndDate" Runat="server" Text='<%# DisplayDate((DateTime)DataBinder.Eval(Container.DataItem, "EndDate")) %>' />
                </ItemTemplate>
            </asp:TemplateColumn>
        </Columns>
    </asp:datagrid>
    <ul class="dnnActions dnnClear">
        <li><asp:hyperlink CssClass="dnnPrimaryAction" id="cmdAdd" resourcekey="cmdAdd" runat="server" /></li>
    </ul>
</div>