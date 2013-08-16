<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Portals.Portals" CodeFile="Portals.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<div class="dnnForm dnnPortals dnnClear" id="dnnPortals">
    <ul class="vLetterSearch">
	    <asp:Repeater id="rptLetterSearch" Runat="server">
		    <itemtemplate>
			    <li><asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl='<%# FilterURL(Container.DataItem.ToString(),"1") %>' Text='<%# Container.DataItem.ToString() %>' /></li>
		    </ItemTemplate>
	    </asp:Repeater>
    </ul>
    <dnn:DnnGrid ID="grdPortals" runat="server" AutoGenerateColumns="false" CssClass="dnnPortalsGrid" AllowCustomPaging="True" AllowPaging="True" EnableViewState="True" OnNeedDataSource="GridNeedsDataSource">
        <MasterTableView>
		    <Columns>
			    <dnn:DnnGridImageCommandColumn CommandName="Edit" IconKey="Edit" EditMode="URL" KeyField="PortalID" UniqueName="EditColumn" />
			    <dnn:DnnGridImageCommandColumn commandname="Delete" IconKey="Delete" keyfield="PortalID" UniqueName="DeleteColumn" />
			    <dnn:DnnGridTemplateColumn HeaderText="PortalId">
				    <ItemTemplate>
					    <asp:Label ID="lblPortalId" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "PortalId") %>' />
				    </ItemTemplate>
			    </dnn:DnnGridTemplateColumn>
			    <dnn:DnnGridTemplateColumn HeaderText="Title">
				    <ItemTemplate>
					    <asp:Label ID="lblPortal" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "PortalName") %>' />
				    </ItemTemplate>
			    </dnn:DnnGridTemplateColumn>
			    <dnn:DnnGridTemplateColumn HeaderText="Portal Aliases">
				    <ItemTemplate>
					    <asp:Label ID="lblPortalAliases" runat="server" Text='<%# FormatPortalAliases(Convert.ToInt32(DataBinder.Eval(Container.DataItem, "PortalID").ToString())) %>' />
				    </ItemTemplate>
			    </dnn:DnnGridTemplateColumn>
			    <dnn:DnnGridBoundColumn DataField="Users" HeaderText="Users" />
			    <dnn:DnnGridBoundColumn DataField="Pages" HeaderText="Pages"/>
			    <dnn:DnnGridBoundColumn DataField="HostSpace" HeaderText="DiskSpace"/>
			    <dnn:DnnGridBoundColumn DataField="HostFee" HeaderText="HostingFee" DataFormatString="{0:0.00}" />
			    <dnn:DnnGridTemplateColumn HeaderText="Expires" >
				    <ItemTemplate>
					    <asp:Label runat="server" Text='<%#FormatExpiryDate((DateTime)DataBinder.Eval(Container.DataItem, "ExpiryDate")) %>' ID="Label1" />
				    </ItemTemplate>
			    </dnn:DnnGridTemplateColumn>
		    </Columns>
        </MasterTableView>
    </dnn:DnnGrid>
    <ul class="dnnActions dnnClear">
	    <li><dnn:ActionLink id="createSite" runat="server" ControlKey="SignUp" CssClass="dnnPrimaryAction" resourcekey="AddContent.Action"  /></li>
		<li><dnn:ActionLink id="exportSite" runat="server" ControlKey="Template" CssClass="dnnSecondaryAction" resourcekey="ExportTemplate.Action" /></li>
		<li><asp:LinkButton id="cmdDeleteExpired" runat="server" CssClass="dnnSecondaryAction" resourcekey="DeleteExpired.Action" /></li>
	</ul>

</div>