<%@ Control Language="C#" AutoEventWireup="false" Explicit="true" Inherits="DotNetNuke.Modules.Admin.LogViewer.LogViewer" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" CodeFile="LogViewer.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
	function setUpDnnLogViewer() {
		$('.dnnLogItemHeader').unbind("click").click(function () {
			$(this).next('.dnnLogItemDetail').slideToggle();
		});
		$('.dnnLogItemHeader input').click(function (e) {
			e.stopPropagation();
		});
		$('#dnnLogViewer').dnnPanels();
		$('#<%= btnClear.ClientID %>').dnnConfirm({
		    text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ClearLog.Text")) %>',
			yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
			noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
			title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
		});
		$('#<%= btnEmail.ClientID %>,#<%= btnDelete.ClientID %>').click(function (e) {
			var checked = $('#dnnLogViewer input').is(':checked');
			if (!checked) {
				e.preventDefault();
				$.dnnAlert({
				    closeText: '<%= Localization.GetSafeJSString("Close.Text", Localization.SharedResourceFile)%>',
				    text: '<%= Localization.GetSafeJSString("SelectException", this.LocalResourceFile) %>'
				});
			}
			return checked;
		});
		
		$("div.wordwrap").each(function() {
			var content = $(this).html();
			content = content.replace(/[\w\.]+/g, function (word) {
				if (word.length <= 50 || word.indexOf(".") == -1) {
					return word;
				}
				return "<span class=\"block\" title=\"" + word + "\">" + word.substr(0, 10) + "..." + word.substr(word.length - 10, 10) + "</span>";
			});
			$(this).html(content);
		});

	}
	$(document).ready(function () {
		setUpDnnLogViewer();
		Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
			setUpDnnLogViewer();
		});
	});
} (jQuery, window.Sys));
</script>

<div class="dnnForm dnnLogViewer dnnClear" id="dnnLogViewer">
	<h2 class="dnnFormSectionHead" id="dnnPanel-ViewLog"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Viewer")%></a></h2>
	<fieldset>
		<div class="dnnClear">        
			<div class="dnnlvOptions">
				<dnn:Label ID="plPortalID" runat="server" ControlName="ddlPortalid" Suffix=":" />
                <dnn:DnnComboBox ID="ddlPortalid" runat="server" AutoPostBack="true" />
                <dnn:Label ID="plLogType" runat="server" ControlName="ddlLogType" Suffix=":" />
                <dnn:DnnComboBox ID="ddlLogType" runat="server" AutoPostBack="true" />
			</div>
            <div class="dnnlvPageSettings">
                <dnn:Label ID="plRecordsPage" runat="server" ResourceKey="Recordsperpage" Suffix=":" />
                    <dnn:DnnComboBox ID="ddlRecordsPerPage" runat="server" AutoPostBack="true">
                        <Items>
                            <dnn:DnnComboBoxItem Value="10" Text="10" />
                            <dnn:DnnComboBoxItem Value="25" Text="25" Selected="True" />
                            <dnn:DnnComboBoxItem Value="50" Text="50" />
                            <dnn:DnnComboBoxItem Value="100" Text="100" />
                            <dnn:DnnComboBoxItem Value="250" Text="250" />
                        </Items>
                    </dnn:DnnComboBox>
            </div>
            <div class="dnnClear"></div>
		</div>
		<div class="dnnlvContent dnnClear">
			<div class="dnnFormMessage">
				<asp:Label ID="lbClickRow" runat="server" resourcekey="ClickRow" />
				<asp:Label ID="txtShowing" runat="server" />
			</div>         
			<asp:Repeater EnableViewState="False" runat="server" ID="dlLog">
				<HeaderTemplate>
					<div class="dnnLogHeader dnnClear">
					    <div style="width:40px"></div>
						<div style="width:190px"><%= Localization.GetString("Date", this.LocalResourceFile) %></div>
						<div style="width:170px"><%= Localization.GetString("Type", this.LocalResourceFile) %></div>
						<div style="width:90px"><%= Localization.GetString("Username", this.LocalResourceFile) %></div>
						<div style="width:120px"><%= Localization.GetString("Portal", this.LocalResourceFile) %></div>
						<div style="width:280px"><%= Localization.GetString("Summary", this.LocalResourceFile) %></div>
					</div>
				</HeaderTemplate>
				<ItemTemplate>
					<div class='dnnLogItemHeader'>
					    <div style="width:40px" class="dnnLogItemHeaderIndicator"><span class="<%# GetMyLogType(DataBinder.Eval(Container.DataItem,"LogTypeKey").ToString()).LogTypeCSSClass %>"></span></div>
						<div style="width:190px" class="dnnLogItemHeaderDate">
						        <div><input type="checkbox" name="Exception" value='<%# ((DotNetNuke.Services.Log.EventLog.LogInfo)Container.DataItem).LogGUID %>|<%# ((DotNetNuke.Services.Log.EventLog.LogInfo)Container.DataItem).LogFileID %>' /></div>
                                <div><%# DataBinder.Eval(Container.DataItem,"LogCreateDate") %></div>
						</div>
						<div style="width:170px"><%# GetMyLogType(DataBinder.Eval(Container.DataItem,"LogTypeKey").ToString()).LogTypeFriendlyName %>&nbsp;</div>
						<div style="width:90px"><%# DataBinder.Eval(Container.DataItem,"LogUserName") %>&nbsp;</div>
						<div style="width:120px"><%# DataBinder.Eval(Container.DataItem,"LogPortalName") %>&nbsp;</div>
						<div style="width:280px" class="wordwrap"><%# ((DotNetNuke.Services.Log.EventLog.LogInfo)Container.DataItem).LogProperties.Summary %>&nbsp;</div>
					</div>
					<div class="dnnLogItemDetail" style="display:none;">
						<%# GetPropertiesText(Container.DataItem) %>
					</div>
				</ItemTemplate>
                <AlternatingItemTemplate>
                    <div class='dnnLogItemHeader dnnLogItemAltHeader'>
					    <div style="width:40px" class="dnnLogItemHeaderIndicator"><span class="<%# GetMyLogType(DataBinder.Eval(Container.DataItem,"LogTypeKey").ToString()).LogTypeCSSClass %>"></span></div>
						<div style="width:190px" class="dnnLogItemHeaderDate">
						        <div><input type="checkbox" name="Exception" value='<%# ((DotNetNuke.Services.Log.EventLog.LogInfo)Container.DataItem).LogGUID %>|<%# ((DotNetNuke.Services.Log.EventLog.LogInfo)Container.DataItem).LogFileID %>' /></div>
                                <div><%# DataBinder.Eval(Container.DataItem,"LogCreateDate") %></div>
						</div>
						<div style="width:170px"><%# GetMyLogType(DataBinder.Eval(Container.DataItem,"LogTypeKey").ToString()).LogTypeFriendlyName %>&nbsp;</div>
						<div style="width:90px"><%# DataBinder.Eval(Container.DataItem,"LogUserName") %>&nbsp;</div>
						<div style="width:120px"><%# DataBinder.Eval(Container.DataItem,"LogPortalName") %>&nbsp;</div>
						<div style="width:280px" class="wordwrap"><%# ((DotNetNuke.Services.Log.EventLog.LogInfo)Container.DataItem).LogProperties.Summary %>&nbsp;</div>
					</div>
					<div class="dnnLogItemDetail" style="display:none;">
						<%# GetPropertiesText(Container.DataItem) %>
					</div>

                </AlternatingItemTemplate>
			</asp:Repeater>
			<dnn:PagingControl ID="ctlPagingControlBottom" runat="server"></dnn:PagingControl>
		</div>
        <div class="dnnlvLegend">
			<h3><%=LocalizeString("Legend") %></h3>
			<div class="dnnLeft">
				<div class="lvlItem"><span class="Exception"></span><asp:Label ID="Label1" runat="server" resourcekey="ExceptionCode" /></div>
				<div class="lvlItem"><span class="ItemCreated"></span><asp:Label ID="Label2" runat="server" resourcekey="ItemCreatedCode" /></div>
				<div class="lvlItem"><span class="ItemUpdated"></span><asp:Label ID="Label3" runat="server" resourcekey="ItemUpdatedCode" /></div>
				<div class="lvlItem"><span class="ItemDeleted"></span><asp:Label ID="Label4" runat="server" resourcekey="ItemDeletedCode" /></div>
			</div>
			<div class="dnnLeft">
				<div class="lvlItem"><span class="AdminAlert"></span><asp:Label ID="Label8" runat="server" resourcekey="AdminAlertCode" /></div>
				<div class="lvlItem"><span class="HostAlert"></span><asp:Label ID="Label9" runat="server" resourcekey="HostAlertCode" /></div>
				<div class="lvlItem"><span class="SecurityException"></span><asp:Label ID="Label10" runat="server" resourcekey="SecurityException" /></div>
			</div>
			<div class="dnnLeft">
				<div class="lvlItem"><span class="OperationSuccess"></span><asp:Label ID="Label5" runat="server" resourcekey="SuccessCode" /></div>
				<div class="lvlItem"><span class="OperationFailure"></span><asp:Label ID="Label6" runat="server" resourcekey="FailureCode" /></div>
				<div class="lvlItem"><span class="GeneralAdminOperation"></span><asp:Label ID="Label7" runat="server" resourcekey="AdminOpCode" /></div>
			</div>
		</div>
	</fieldset>
	<h2 class="dnnFormSectionHead" id="dnnPanel-LogSendExceptions"><a href="" class="dnnSectionExpanded"><%=LocalizeString("SendExceptions")%></a></h2>
	<fieldset id="dnnSendExceptions">
		<div class="dnnFormItem">
			<dnn:Label ID="plEmailAddress" runat="server" ControlName="txtEmailAddress" Suffix=":" />
			<asp:TextBox ID="txtEmailAddress" runat="server" />
		</div>        
		<div class="dnnFormItem">
			<dnn:Label ID="plSubject" runat="server" ControlName="txtSubject" Suffix=":" />
			<asp:TextBox ID="txtSubject" runat="server" Rows="2" Columns="25" TextMode="MultiLine" />
		</div>        
		<div class="dnnFormItem">
			<dnn:Label ID="plMessage" runat="server" ResourceKey="SendMessage" ControlName="txtMessage" Suffix=":" />
			<asp:TextBox ID="txtMessage" runat="server" Rows="6" Columns="25" TextMode="MultiLine" />
		</div>        
		<ul class="dnnActions dnnClear">
			<li><asp:LinkButton ID="btnEmail" runat="server" CssClass="dnnPrimaryAction" resourcekey="btnEmail" /></li>
		</ul>    
	</fieldset>
	<ul class="dnnActions dnnClear">
		<li><asp:LinkButton CssClass="dnnPrimaryAction" ID="btnDelete" resourcekey="btnDelete" runat="server" /></li>
		<li><asp:LinkButton CssClass="dnnSecondaryAction dnnLogDelete" ID="btnClear" resourcekey="btnClear" runat="server" /></li>
		<li><dnn:ActionLink CssClass="dnnSecondaryAction" ID="editSettings" ControlKey="Edit" resourcekey="AddContent.Action" runat="server" /></li>
	</ul>
</div>