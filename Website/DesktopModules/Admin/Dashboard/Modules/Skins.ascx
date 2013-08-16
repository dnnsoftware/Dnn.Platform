<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Dashboard.Controls.Skins" CodeFile="Skins.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnSkins dnnClear">
  <%--  <asp:DataGrid ID="grdSkins" runat="server" GridLines="None" AutoGenerateColumns="false" EnableViewState="False" CssClass="dnnGrid">
        <headerstyle cssclass="dnnGridHeader" verticalalign="Top" />
        <ItemStyle cssclass="dnnGridItem" horizontalalign="Left" />
        <AlternatingItemStyle cssclass="dnnGridAltItem" />
        <footerstyle cssclass="dnnGridFooter" />
        <pagerstyle cssclass="dnnGridPager" />
        <Columns>
            <asp:BoundColumn DataField="SkinName" HeaderText="Skin"  ItemStyle-Width="250px"/>
            <asp:BoundColumn DataField="InUse" HeaderText="InUse" ItemStyle-Width="100px"/>
        </Columns>
    </asp:DataGrid>--%>

    <dnn:DnnGrid ID="grdSkins" runat="server" AutoGenerateColumns="false">
        <MasterTableView>
            <Columns>
                <dnn:DnnGridBoundColumn DataField="SkinName" HeaderText="Skin" />
                <dnn:DnnGridBoundColumn DataField="InUse" HeaderText="InUse" />
            </Columns>
        </MasterTableView>
    </dnn:DnnGrid>
</div>