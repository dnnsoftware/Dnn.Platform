<%@ Control Language="C#" AutoEventWireup="false" Explicit="true" Inherits="DotNetNuke.UI.UserControls.URLTrackingControl" %>
<div class="dnnForm dnnUrlTrackingControl dnnClear">
	<div class="dnnFormItem">
		<asp:label id="Label1" resourcekey="Url" runat="server" enableviewstate="False" />
		<asp:label id="lblURL" Runat="server" />
		<asp:label id="lblLogURL" Runat="server" Visible="False" />
	</div>
	<div class="dnnFormItem">
		<asp:label id="Label3" resourcekey="Created" runat="server" enableviewstate="False" />
		<asp:label id="lblCreatedDate" Runat="server" />
	</div>
	<asp:Panel id="pnlTrack" runat="server" visible="False">
		<div class="dnnFormItem">
			<asp:label id="Label2" resourcekey="trackingUrl" runat="server" enableviewstate="False" />
			<asp:label id="lblTrackingURL" Runat="server" />
		</div>
		<div class="dnnFormItem">
			<asp:label id="Label4" resourcekey="Clicks" runat="server" enableviewstate="False" />
			<asp:label id="lblClicks" Runat="server" />
		</div>
		<div class="dnnFormItem">
			<asp:label id="Label5" resourcekey="LastClick" runat="server" enableviewstate="False" />
			<asp:label id="lblLastClick" Runat="server" />
		</div>
	</asp:Panel>
	<asp:Panel id="pnlLog" runat="server" visible="False">
		<div class="dnnFormItem">
			<asp:label id="Label6" runat="server" resourcekey="Startdate" enableviewstate="False" />
			<asp:TextBox id="txtStartDate" runat="server" />
			<asp:HyperLink id="cmdStartCalendar" resourcekey="Calendar" Runat="server" CssClass="dnnSecondaryAction" enableviewstate="False" />
		</div>
		<div class="dnnFormItem">
			<asp:label id="Label7" runat="server" resourcekey="EndDate" enableviewstate="False" />
			<asp:TextBox id="txtEndDate" runat="server" />
			<asp:HyperLink id="cmdEndCalendar" resourcekey="Calendar" Runat="server" CssClass="dnnSecondaryAction" enableviewstate="False" />
		</div>
		<ul class="dnnActions dnnClear">
			<li><asp:LinkButton id="cmdDisplay" runat="server" resourcekey="cmdDisplay" cssclass="dnnPrimaryAction" enableviewstate="False" /></li>
		</ul>
		<asp:datagrid id="grdLog" runat="server" EnableViewState="false" AutoGenerateColumns="false" CssClass="dnnGrid">
			<headerstyle cssclass="dnnGridHeader" verticalalign="Top" />
			<itemstyle cssclass="dnnGridItem" horizontalalign="Left" />
			<alternatingitemstyle cssclass="dnnGridAltItem" />
			<edititemstyle cssclass="dnnFormInput" />
			<selecteditemstyle cssclass="dnnFormError" />
			<footerstyle cssclass="dnnGridFooter" />
			<pagerstyle cssclass="dnnGridPager" />
			<Columns>
				<asp:BoundColumn HeaderText="Date" DataField="ClickDate" />
				<asp:BoundColumn HeaderText="User" DataField="FullName" />
			</Columns>
		</asp:datagrid>
	</asp:Panel>
</div>