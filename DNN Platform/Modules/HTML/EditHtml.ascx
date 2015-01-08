<%@ Control Language="C#" Inherits="DotNetNuke.Modules.Html.EditHtml" CodeBehind="EditHtml.ascx.cs" AutoEventWireup="false" Explicit="True" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="texteditor" Src="~/controls/texteditor.ascx" %>
<%@ Register TagPrefix="dnnweb" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnncl" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<dnncl:DnnCssInclude ID="customJS" runat="server" FilePath="DesktopModules/HTML/edit.css" AddTag="false" />

<div class="dnnForm dnnEditHtml dnnClear" id="dnnEditHtml">
    <div class="ehCurrentContent dnnClear" id="ehCurrentContent">
        <div class="ehccContent dnnClear">
            <asp:PlaceHolder ID="phEdit" runat="server">
                <fieldset>
                    <div class="dnnFormItem">
                        <asp:PlaceHolder ID="phMasterContent" runat="server" Visible="false">
                            <div class="ehmContent dnnClear" id="ehmContent" runat="server">
                                <div class="html_preview">
                                    <asp:PlaceHolder ID="placeMasterContent" runat="server" />
                                </div>
                            </div>
                        </asp:PlaceHolder>
                        <dnn:texteditor id="txtContent" runat="server" height="400" width="100%" ChooseMode="false" ></dnn:texteditor>
                    </div>
                    <div class="dnnFormItem" id="divSubmittedContent" runat="server">
                        <div id="Div3" class="html_preview" runat="server">
                            <asp:Literal ID="litCurrentContentPreview" runat="server" />
                        </div>
                    </div>
                    <asp:PlaceHolder ID="phCurrentVersion" runat="server">
                        <div class="divCurrentVersion">
                            <div class="dnnFormItem" id="divCurrentVersion" runat="server">
                                <dnn:label id="plCurrentWorkVersion" runat="server" controlname="lblCurrentVersion" text="Version" suffix=":" />
                                <asp:Label ID="lblCurrentVersion" runat="server" />
                            </div>

                            <div class="dnnFormItem">
                                <dnn:label id="plCurrentWorkflowInUse" runat="server" controlname="lblCurrentWorkflowInUse" text="Workflow in Use" suffix=":" />
                                <asp:Label ID="lblCurrentWorkflowInUse" runat="server" />
                            </div>
                            <div class="dnnFormItem" id="divCurrentWorkflowState" runat="server">
                                <dnn:label id="plCurrentWorkflowState" runat="server" controlname="lblCurrentWorkflowState" text="Workflow State" suffix=":" />
                                <asp:Label ID="lblCurrentWorkflowState" runat="server" />
                            </div>
                            <div class="dnnFormItem" id="divPublish" runat="server">
                                <dnn:label id="plActionOnSave" runat="server" text="On Save" suffix="?" />
                                <asp:CheckBox ID="chkPublish" runat="server" resourcekey="chkPublish" AutoPostBack="true" />
                            </div>
                        </div>
                    </asp:PlaceHolder>

                </fieldset>
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="phPreview" runat="server" Visible="false">
                <fieldset>
                    <asp:PlaceHolder ID="phPreviewVersion" runat="server">
                        <div class="dnnFormItem" id="divPreviewVersion" runat="server">
                            <dnn:label id="plPreviewVersion" runat="server" controlname="lblPreviewVersion" suffix=":" />
                            <asp:Label ID="lblPreviewVersion" runat="server" />
                        </div>
                        <div class="dnnFormItem" id="divPreviewWorlflow" runat="server">
                            <dnn:label id="plPreviewWorkflowInUse" runat="server" controlname="lblPreviewWorkflowInUse" suffix=":" />
                            <asp:Label ID="lblPreviewWorkflowInUse" runat="server" />
                        </div>
                        <div class="dnnFormItem" id="divPreviewWorkflowState" runat="server">
                            <dnn:label id="plPreviewWorkflowState" runat="server" controlname="lblPreviewWorkflowState" suffix=":" />
                            <asp:Label ID="lblPreviewWorkflowState" runat="server" />
                        </div>
                    </asp:PlaceHolder>
                    <div id="Div1" class="html_preview" runat="server">
                        <asp:Literal ID="litPreview" runat="server" />
                    </div>
                </fieldset>
                <h2 id="dnnSitePanelEditHTMLHistory" class="dnnFormSectionHead" runat="server"><a href=""><%=LocalizeString("dshHistory")%></a></h2>
                <fieldset id="fsEditHtmlHistory" runat="server">
                    <dnnweb:DnnGrid ID="dgHistory" runat="server" AutoGenerateColumns="false">
                        <mastertableview>
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
					</mastertableview>
                    </dnnweb:DnnGrid>
                </fieldset>
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="phHistory" runat="server" Visible="false">
                <fieldset>
                    <div class="ehvContent">
                        <div class="dnnFormItem">
                            <dnn:label id="plMaxVersions" runat="server" controlname="lblMaxVersions" suffix=":" />
                            <asp:Label ID="lblMaxVersions" runat="server" />
                        </div>
                        <dnnweb:DnnGrid ID="dgVersions" runat="server" AutoGenerateColumns="false" AllowPaging="True" PageSize="5">
                            <pagerstyle mode="NextPrevAndNumeric"></pagerstyle>
                            <mastertableview>
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
						        <asp:Label ID="lblNoRecords1" runat="server" resourcekey="NoVersions" />
					        </NoRecordsTemplate>
				        </mastertableview>
                        </dnnweb:DnnGrid>
                    </div>
                </fieldset>
            </asp:PlaceHolder>
        </div>
    </div>
    <div class="ehActions">
        <ul class="dnnActions dnnClear">
            <li>
                <asp:LinkButton ID="cmdSave" runat="server" class="dnnPrimaryAction" resourcekey="cmdSave" /></li>
            <li>
                <asp:HyperLink ID="hlCancel" runat="server" class="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
            <li>
                <asp:LinkButton ID="cmdPreview" runat="server" class="dnnSecondaryAction" resourcekey="cmdPreview" /></li>
            <li>
                <asp:LinkButton ID="cmdEdit" runat="server" class="dnnSecondaryAction" resourcekey="cmdEdit" Visible="false" /></li>
            <li>
                <asp:LinkButton ID="cmdHistory" runat="server" class="dnnSecondaryAction" resourcekey="cmdHistory" /></li>
            <li>
                <asp:LinkButton ID="cmdMasterContent" runat="server" class="dnnSecondaryAction" Visible="false" /></li>
            <li>
                <asp:DropDownList ID="ddlRender" runat="server" AutoPostBack="true">
                    <asp:ListItem resourcekey="liRichText" Value="RICH"></asp:ListItem>
                    <asp:ListItem resourcekey="liBasicText" Value="BASIC"></asp:ListItem>
                </asp:DropDownList>
            </li>
        </ul>
    </div>
</div>
<asp:HiddenField ID="hfEditor" runat="server" />
<script language="javascript" type="text/javascript">
    /*globals jQuery, window, Sys */
    (function ($, Sys) {
        function setupDnnEditHtml() {
            $('#dnnEditHtml').dnnPanels();
            if ($(window).attr('name') == 'iPopUp') {
                var ckeditorid = $(".ehCurrentContent textarea.editor").attr('id');
                if (ckeditorid) {
                    CKEDITOR.on("instanceReady", function (event) {
                        editor = event.editor;
                        resizeDnnEditHtml();
                    });
                } else {
                    resizeDnnEditHtml();
                }
                $(window).resize(function () {
                    var timeout;
                    if (timeout) clearTimeout(timeout);
                    timeout = setTimeout(function () {
                        timeout = null;
                        resizeDnnEditHtml();
                    }, 50);
                });
            }
        }
        function resizeDnnEditHtml() {
            $('.ehCurrentContent').height($(window).height() - $('.ehActions').height() - $('.dnnEditHtml ').outerHeight(true) + $('.dnnEditHtml ').innerHeight() - $('.dnnEditHtml ').offset().top);
            // RadEditor
            var editor = $find($(".RadEditor").attr('id'));
            if (editor) {
                editor.setSize($('.ehCurrentContent').width(), $('.ehCurrentContent').height() - 30 - $('.divCurrentVersion').height() - $('.ehmContent').height());
                $('.ehCurrentContent').css('overflow', 'hidden');
            }
            // CK Editor
            var editorid = $(".ehCurrentContent textarea.editor").attr('id');
            if (editorid) {

                var ckeditor = CKEDITOR.instances[editorid];
                if (ckeditor && ckeditor.status == 'ready') {
                    ckeditor.resize($('.ehCurrentContent').width(), $('.ehCurrentContent').height() - $('.dnnTextPanelView').height() - $('.divCurrentVersion').height() - $('.ehCurrentContent .dnnTextPanel p').height() - $('.ehmContent').height());
                    $('.ehCurrentContent').css('overflow', 'hidden');
                    $('.ehCurrentContent .dnnTextPanel p').css('margin-bottom', '0');
                }
            }
            // Basic editor
            var basiceditor = $('.dnnTextPanel .dnnFormItem textarea');
            if (basiceditor) {
                $('.ehCurrentContent').css('overflow', 'hidden');
                $(basiceditor).css('max-width', $('.ehCurrentContent').width() - 18);
                $(basiceditor).css('height', $('.ehCurrentContent').height() - 36 - 37 - 30);
            }
        }
        $(document).ready(function () {
            setupDnnEditHtml();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setupDnnEditHtml();
            });
        });
    }(jQuery, window.Sys));
</script>
