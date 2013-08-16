<%@ Control language="C#" Inherits="DotNetNuke.Modules.Html.EditHtml" CodeBehind="EditHtml.ascx.cs" AutoEventWireup="false" Explicit="True" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="texteditor" Src="~/controls/texteditor.ascx" %>
<%@ Register TagPrefix="dnnweb" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div class="dnnForm dnnEditHtml dnnClear" id="dnnEditHtml">
	<div class="ehCurrentContent dnnClear" id="ehCurrentContent">
		<div class="ehccContent dnnClear">
			<fieldset>
				<div class="dnnFormItem">
		        <div class="ehmContent dnnClear" id="ehmContent" runat="server">
			        <div class="html_preview"><asp:placeholder id="placeMasterContent" runat="server" /></div>
		        </div>
					<dnn:texteditor id="txtContent" runat="server" height="400" width="100%"></dnn:texteditor>
				</div>
				<div class="dnnFormItem" id="divSubmittedContent" runat="server">
					<div id="Div3" class="html_preview" runat="server"><asp:Literal ID="litCurrentContentPreview" runat="server" /></div>     
				</div>
				<div class="dnnFormItem" id="divCurrentVersion" runat="server">
					<dnn:label id="plCurrentWorkVersion" runat="server" controlname="lblCurrentVersion" text="Version" suffix=":"/>
					<asp:Label ID="lblCurrentVersion" runat="server" />
				</div>
				<div class="dnnFormItem">
					<dnn:label id="plCurrentWorkflowInUse" runat="server" controlname="lblCurrentWorkflowInUse" text="Workflow in Use" suffix=":"/>
					<asp:Label ID="lblCurrentWorkflowInUse" runat="server" />
				</div>
				<div class="dnnFormItem" id="divCurrentWorkflowState" runat="server">
					<dnn:label id="plCurrentWorkflowState" runat="server" controlname="lblCurrentWorkflowState" text="Workflow State" suffix=":"/>
					<asp:Label ID="lblCurrentWorkflowState" runat="server" />
				</div>
				<div class="dnnFormItem" id="divPublish" runat="server">
					<dnn:label id="plActionOnSave" runat="server" text="On Save" suffix="?" />
					<asp:checkbox id="chkPublish" runat="server" resourcekey="chkPublish" AutoPostBack="true" />
				</div>
				<ul class="dnnActions dnnClear">
					<li><asp:LinkButton id="cmdSave" runat="server" class="dnnPrimaryAction" resourcekey="cmdSave" /></li>
					<li><asp:LinkButton id="cmdPreview" runat="server" class="dnnSecondaryAction" resourcekey="cmdPreview" /></li>
					<li><asp:HyperLink id="hlCancel" runat="server" class="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
				</ul>
			</fieldset>
			<h2 id="dnnPanel-EditHtmlPreview" class="dnnFormSectionHead"><a href=""><%=LocalizeString("dshPreview")%></a></h2>
			<fieldset>
				<div class="dnnFormItem" id="divPreviewVersion" runat="server">
					<dnn:label id="plPreviewVersion" runat="server" controlname="lblPreviewVersion" suffix=":" />
					<asp:label id="lblPreviewVersion" runat="server" />
				</div>
				<div class="dnnFormItem" id="divPreviewWorlflow" runat="server">
					<dnn:label id="plPreviewWorkflowInUse" runat="server" controlname="lblPreviewWorkflowInUse" suffix=":" />
					<asp:label id="lblPreviewWorkflowInUse" runat="server" />
				</div>
				<div class="dnnFormItem" id="divPreviewWorkflowState" runat="server">
					<dnn:label id="plPreviewWorkflowState" runat="server" controlname="lblPreviewWorkflowState" suffix=":" />
					<asp:Label ID="lblPreviewWorkflowState" runat="server" />
				</div>
				<div id="Div1" class="html_preview" runat="server"><asp:Literal ID="litPreview" runat="server" /></div>
			</fieldset>
			<h2 id="dnnSitePanelEditHTMLHistory" class="dnnFormSectionHead" runat="server"><a href=""><%=LocalizeString("dshHistory")%></a></h2>
			<fieldset id="fsEditHtmlHistory" runat="server">
				<dnnweb:dnngrid ID="dgHistory" runat="server" AutoGenerateColumns="false">
					<MasterTableView>
						<Columns>
								<dnnweb:DnnGridBoundColumn HeaderText="Date" DataField="CreatedOnDate" />
								<dnnweb:DnnGridBoundColumn HeaderText="User" DataField="DisplayName"/>
								<dnnweb:DnnGridBoundColumn HeaderText="State" DataField="StateName"/>
								<dnnweb:DnnGridBoundColumn HeaderText="Approved" DataField="Approved" />
								<dnnweb:DnnGridBoundColumn HeaderText="Comment" DataField="Comment"/>
						</Columns>
						<NoRecordsTemplate>
							<asp:Label ID="lblNoRecords" runat="server" resourcekey="NoHistory" />
						</NoRecordsTemplate>
					</MasterTableView>
				</dnnweb:dnngrid>
			</fieldset>
            <h2 id="dnnVersions" class="dnnFormSectionHead" runat="server"><a href=""><%=LocalizeString("dshVersions")%></a></h2>
            <fieldset>
		        <div class="ehvContent">
			        <div class="dnnFormItem">
				        <dnn:label id="plMaxVersions" runat="server" controlname="lblMaxVersions" suffix=":" />
				        <asp:Label ID="lblMaxVersions" runat="server" />
			        </div>
			        <dnnweb:dnngrid ID="dgVersions" runat="server" AutoGenerateColumns="false" AllowPaging="True" PageSize="5" >
				        <PagerStyle Mode="NextPrevAndNumeric"></PagerStyle>
				         <MasterTableView>
					        <Columns>
						        <dnnweb:DnnGridBoundColumn HeaderText="Version" DataField="Version" />
						        <dnnweb:DnnGridBoundColumn HeaderText="Date" DataField="LastModifiedOnDate"  />
						        <dnnweb:DnnGridBoundColumn HeaderText="User" DataField="DisplayName" />
						        <dnnweb:DnnGridBoundColumn HeaderText="State" DataField="StateName" />
						        <dnnweb:DnnGridTemplateColumn>
							        <HeaderTemplate>
								        <table cellpadding="0" cellspacing="0" class="DnnGridNestedTable">
									        <tr>
										        <td><dnnweb:DnnImage ID="imgDelete" runat="server" IconKey="ActionDelete" resourcekey="VersionsRemove" /></td>
										        <td><dnnweb:DnnImage ID="imgPreview" runat="server" IconKey="View"  resourcekey="VersionsPreview" /></td>
										        <td><dnnweb:DnnImage ID="imgRollback" runat="server" IconKey="Restore"  resourcekey="VersionsRollback" /></td>
									        </tr>
								        </table>
							        </HeaderTemplate>
							        <ItemTemplate>
								        <table cellpadding="0" cellspacing="0" class="DnnGridNestedTable">
									        <tr style="vertical-align: top;">
										        <td><dnnweb:DnnImageButton ID="btnRemove" runat="server" CommandName="Remove" IconKey="ActionDelete" Text="Delete" resourcekey="VersionsRemove" /></td>
										        <td><dnnweb:DnnImageButton ID="btnPreview" runat="server" CommandName="Preview"  IconKey="View" Text="Preview" resourcekey="VersionsPreview" /></td>
										        <td><dnnweb:DnnImageButton ID="btnRollback" runat="server" CommandName="RollBack" IconKey="Restore" Text="Rollback" resourcekey="VersionsRollback" /></td>
									        </tr>
								        </table>
							        </ItemTemplate>
						        </dnnweb:DnnGridTemplateColumn>
					        </Columns>
					        <NoRecordsTemplate>
						        <asp:Label ID="lblNoRecords" runat="server" resourcekey="NoVersions" />
					        </NoRecordsTemplate>
				        </MasterTableView>
			        </dnnweb:dnngrid>
		        </div>
            </fieldset>
		</div>
	</div>
</div>
<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
	function setupDnnEditHtml() {
		$('#dnnEditHtml').dnnPanels();
	}
	$(document).ready(function () {
		setupDnnEditHtml();
		Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
			setupDnnEditHtml();
		});
	});
} (jQuery, window.Sys));
</script>