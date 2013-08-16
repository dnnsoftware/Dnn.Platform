<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Host.FriendlyUrls" CodeFile="FriendlyUrls.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div style="width: 950px; overflow: auto;" class="dnnScroll">
<asp:DataGrid ID="grdRules" AutoGenerateColumns="false" width="100%" GridLines="None" 
    CssClass="dnnGrid" Runat="server">
    <headerstyle CssClass="dnnGridHeader" />
    <itemstyle CssClass="dnnGridItem" />
    <alternatingitemstyle CssClass="dnnGridAltItem" />
    <edititemstyle />
    <selecteditemstyle />
    <footerstyle />
    <Columns>
        <dnn:imagecommandcolumn commandname="Edit" IconKey="Edit" HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder" />
		<dnn:imagecommandcolumn commandname="Delete" IconKey="Delete" />
        <asp:TemplateColumn HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder">
            <ItemStyle></ItemStyle>
            <EditItemTemplate>
	            <dnn:DnnImageButton Runat="server" ID="lnkSave" resourcekey="saveRule" OnCommand="SaveRule" IconKey="Save" />
            </EditItemTemplate>
        </asp:TemplateColumn>
        <asp:TemplateColumn HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder">
            <ItemStyle></ItemStyle>
            <EditItemTemplate>
	            <dnn:DnnImageButton Runat="server" ID="lnkCancelEdit" resourcekey="cmdCancel" OnCommand="CancelEdit" IconKey="Cancel" />
            </EditItemTemplate>
        </asp:TemplateColumn>
		<asp:TemplateColumn HeaderText="Match">
		    <HeaderStyle  Width="47%" HorizontalAlign="Left" />
		    <ItemStyle Width="47%" HorizontalAlign="Left" />
		    <ItemTemplate>
                <asp:label runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "LookFor") %>' ID="lblMatch" />
		    </ItemTemplate>
		    <EditItemTemplate>
                <asp:textbox runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "LookFor") %>' ID="txtMatch" />
		    </EditItemTemplate>
		</asp:TemplateColumn>
		<asp:TemplateColumn HeaderText="ReplaceWith" >
		    <HeaderStyle  Width="47%" HorizontalAlign="Left" />
		    <ItemStyle  Width="47%" HorizontalAlign="Left" />
		    <ItemTemplate>
                <asp:label runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "SendTo") %>' ID="lblReplace" />
		    </ItemTemplate>
		    <EditItemTemplate>
                <asp:textbox runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "SendTo") %>' ID="txtReplace" />
		    </EditItemTemplate>
		</asp:TemplateColumn>
      
	
    </Columns>
</asp:DataGrid>
</div>
<ul class="dnnActions rfAddRule dnnClear"><li><asp:LinkButton ID="cmdAddRule" runat="server" resourcekey="cmdAdd" CssClass="dnnPrimaryAction" /></li></ul>