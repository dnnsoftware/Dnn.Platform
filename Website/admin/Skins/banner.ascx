<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Controls.Banner" CodeFile="Banner.ascx.cs" %>
<asp:DataList id="lstBanners" runat="server" Summary="Banner Design Table">
	<ItemTemplate>
		<asp:Label ID="lblItem" Runat="server" Text='<%# FormatItem((int)DataBinder.Eval(Container.DataItem,"VendorId"), (int)DataBinder.Eval(Container.DataItem,"BannerId"), (int)DataBinder.Eval(Container.DataItem,"BannerTypeId"), (string)DataBinder.Eval(Container.DataItem,"BannerName"), (string)DataBinder.Eval(Container.DataItem,"ImageFileUrl"), (string)DataBinder.Eval(Container.DataItem,"Description"), (string)DataBinder.Eval(Container.DataItem,"Url"), (int)DataBinder.Eval(Container.DataItem,"Width"), (int)DataBinder.Eval(Container.DataItem,"Height")) %>'></asp:Label>
	</ItemTemplate>
</asp:DataList>
