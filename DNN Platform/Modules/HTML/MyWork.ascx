<%@ Control language="C#" Inherits="DotNetNuke.Modules.Html.MyWork" CodeBehind="MyWork.ascx.cs" AutoEventWireup="false" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls.Internal" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnncl" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<dnncl:DnnCssInclude ID="customJS" runat="server" FilePath="DesktopModules/HTML/edit.css" AddTag="false" />

<div class="dnnForm dnnMyWork dnnClear">
	<dnnweb:DnnGrid ID="dgTabs" runat="server" AutoGenerateColumns="False" AllowPaging="false" EnableViewState="true" CssClass="dnnGrid">
			<Columns>
				<dnnweb:DnnGridTemplateColumn HeaderText="Page">
					<ItemTemplate>
						<%#FormatURL(Container.DataItem)%>
					</ItemTemplate>
				</dnnweb:DnnGridTemplateColumn>
			</Columns>
			<EmptyDataTemplate>
				<div class="dnnFormMessage dnnFormWarning">
					<asp:Label ID="lblNoRecords" resourcekey="lblNoRecords" runat="server" />
				</div>
			</EmptyDataTemplate>
	</dnnweb:DnnGrid>
	<ul class="dnnActions dnnClear">
		<li><asp:HyperLink id="hlCancel" runat="server" class="dnnPrimaryAction" resourcekey="cmdCancel" /></li>
	</ul>
</div>