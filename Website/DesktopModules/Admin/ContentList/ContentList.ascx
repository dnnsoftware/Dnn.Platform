<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.ContentList.ContentList" CodeFile="ContentList.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnContentList dnnClear">
	<div>
		<asp:label ID="lblMessage" runat="server" />
	</div>
	<div>
		<dnn:DnnGrid id="dgResults" runat="server" AutoGenerateColumns="False" AllowPaging="true">
            <MasterTableView>
			    <Columns>
				    <dnn:DnnGridTemplateColumn>
					    <ItemTemplate>
						    <asp:Label id="lblNo" runat="server" Text='<%# (int)DataBinder.Eval(Container, "ItemIndex") + 1 %>' CssClass="SubHead" />
					    </ItemTemplate>
				    </dnn:DnnGridTemplateColumn>
				    <dnn:DnnGridTemplateColumn>
					    <ItemTemplate>
						    <asp:HyperLink id="lnkTitle" runat="server" NavigateUrl='<%# FormatURL((int)DataBinder.Eval(Container.DataItem,"TabId"),DataBinder.Eval(Container.DataItem,"ContentKey").ToString()) %>' Text='<%# DataBinder.Eval(Container.DataItem, "Title") %>' />
						    <br/>
						    <asp:Label id="lblSummary" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Description") + "<br>" %>' Visible="<%# ShowDescription() %>" />
						    <asp:HyperLink id="lnkLink" runat="server" CssClass="dnnSecondaryAction" NavigateUrl='<%# FormatURL((int)DataBinder.Eval(Container.DataItem,"TabId"),DataBinder.Eval(Container.DataItem,"ContentKey").ToString()) %>' Text='<%# FormatURL((int)DataBinder.Eval(Container.DataItem,"TabId"),DataBinder.Eval(Container.DataItem,"ContentKey").ToString()) %>' />&nbsp;-
						    <asp:Label id="lblPubDate" runat="server" Text='<%# FormatDate((DateTime)DataBinder.Eval(Container.DataItem, "PubDate")) %>' />
					    </ItemTemplate>
				    </dnn:DnnGridTemplateColumn>
			    </Columns>
            </MasterTableView>
		</dnn:DnnGrid>
	</div>
</div>