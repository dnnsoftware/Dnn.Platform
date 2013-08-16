<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Vendors.Affiliates" CodeFile="Affiliates.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div class="dnnForm dnnAffiliates dnnClear">
    <asp:DataGrid ID="grdAffiliates" runat="server" Width="100%" AutoGenerateColumns="false" EnableViewState="true" GridLines="None" CssClass="dnnAffiliatesGrid">
        <headerstyle cssclass="dnnGridHeader" verticalalign="Top"/>
	    <itemstyle cssclass="dnnGridItem" horizontalalign="Left" />
	    <alternatingitemstyle cssclass="dnnGridAltItem" />
	    <edititemstyle cssclass="dnnFormInput" />
	    <selecteditemstyle cssclass="dnnFormError" />
	    <footerstyle cssclass="dnnGridFooter" />
	    <pagerstyle cssclass="dnnGridPager" />
        <Columns>
            <asp:TemplateColumn>
                <ItemStyle Width="20px"></ItemStyle>
                <ItemTemplate>
                    <asp:HyperLink NavigateUrl='<%# FormatURL("AffilId",DataBinder.Eval(Container.DataItem,"AffiliateId").ToString()) %>'
                        runat="server" ID="Hyperlink1">
                        <dnn:DnnImage IconKey="Edit" resourcekey="Edit" AlternateText="Edit" runat="server" ID="Hyperlink1Image" />
                    </asp:HyperLink>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn HeaderText="Start">
                <ItemTemplate>
                    <asp:Label ID="lblStartDate" runat="server" Text='<%# DisplayDate((DateTime)DataBinder.Eval(Container.DataItem, "StartDate")) %>' />
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn HeaderText="End">
                <ItemTemplate>
                    <asp:Label ID="lblEndDate" runat="server" Text='<%# DisplayDate((DateTime)DataBinder.Eval(Container.DataItem, "EndDate")) %>' />
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn DataField="CPC" HeaderText="CPC" DataFormatString="{0:#,##0.0####}" />
            <asp:BoundColumn DataField="Clicks" HeaderText="Clicks"/>
            <asp:BoundColumn DataField="CPCTotal" HeaderText="Total" DataFormatString="{0:#,##0.0####}" />
            <asp:BoundColumn DataField="CPA" HeaderText="CPA" DataFormatString="{0:#,##0.0####}" />
            <asp:BoundColumn DataField="Acquisitions" HeaderText="Acquisitions"/>
            <asp:BoundColumn DataField="CPATotal" HeaderText="Total" DataFormatString="{0:#,##0.0####}" />
        </Columns>
    </asp:DataGrid>
    <ul class="dnnActions dnnClear">
        <li><asp:hyperlink CssClass="dnnPrimaryAction" id="cmdAdd" resourcekey="cmdAdd" runat="server" /></li>
    </ul>
</div>