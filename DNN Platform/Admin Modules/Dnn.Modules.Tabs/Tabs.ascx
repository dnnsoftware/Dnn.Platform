<%@ Control Language="C#" AutoEventWireup="false" Inherits="Dnn.Modules.Tabs.View" Codebehind="Tabs.ascx.cs" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web.Deprecated" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Security.Permissions.Controls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" TagName="URL" Src="~/controls/DnnUrlControl.ascx" %>
<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
	var needReload = false;

	var fieldsChanged = false;
	var fieldsChangedHandler = function(e) {
		fieldsChanged = true;
	}

	var originalPostBack = window['__doPostBack'];
	var executePostBackAndClean = function (sender, args){
		originalPostBack(sender, args);
		postBackElement = postBackArguments = null;
	};

	var postBackElement = null;
	var postBackArguments = null;

	window['__doPostBack'] = function (sender, args) {
		if (sender.indexOf('cmdUpdate') === -1) {
			postBackElement = sender;
			postBackArguments = args;
		}
		if (fieldsChanged){
			originalPostBack(sender, args);
		} else {
			executePostBackAndClean(sender, args);
		}
	}

	var isPendingAction = function (element) {
		var $el = $(element);
		return $el.parents('#<%=pnlDetails.ClientID %>').length === 0;
	}

	var delayPostBack = function () {
		if (postBackElement && postBackArguments) {
			executePostBackAndClean(postBackElement, postBackArguments);
		}
	}

	var msgQueue = [];
	var processMsgQueue = function () {
		while (msgQueue.length > 0) {
			setTimeout(msgQueue[0], 0);
			msgQueue.splice(0, 1);
		}
	};

	var parseSelectedPageFromHash = function() {
		if (location.hash != "") {
			$.each(location.hash.toUpperCase().replace("#", "").split("&"), function (index, value) {
				if (value == "P" || value == "H") {
					$("input[type=radio][name$=rblMode][value=" + value + "]").trigger("click");
				}
				else if (/^\d+$/.test(value)) {
					/*try to find node in tree, 
					if can't find then push it into message queue to wait tree re-load and check again.*/
					setTimeout(function () {
						var selectNode = function () {
							var tree = $find("<%=ctlPages.ClientID %>");
							var node = tree.findNodeByValue(value);
							if (node == null) {
								return false;
							}
							else {
								node.get_parent().expand();
								node.select();
								return true;
							}
						};

						if (!selectNode()) {
							msgQueue.push(selectNode);
						}
					}, 0);
				}
			});
		}
	}

	var showConfirmSave = function () {
		var pmr = Sys.WebForms.PageRequestManager.getInstance();
		var ajaxPanelId = $('#' + pmr._updatePanelClientIDs[0]).find('> div').attr('id');
		var ajaxPanel = $find(ajaxPanelId);

		setTimeout(function() {
			$('#' + ajaxPanel.get_loadingPanelID() + ajaxPanelId).remove();
		}, 0);

		var options = {
			callbackTrue: function () {
				fieldsChanged = false;
				location.href = $('#<%=cmdUpdate.ClientID%>').attr('href');
			},
			callbackFalse: function () {
				fieldsChanged = false;
				if (needReload) {
					parseSelectedPageFromHash();
				} else {
					delayPostBack();
				}
			},
			text: '<%=Localization.GetSafeJSString("ConfirmSave", LocalResourceFile)%>',
			yesText: '<%=Localization.GetSafeJSString("Yes", LocalResourceFile)%>',
			noText: '<%=Localization.GetSafeJSString("No", LocalResourceFile)%>',
			title: '<%=Localization.GetSafeJSString("Confirm", LocalResourceFile)%>'
		};

		//show prompt
		$.dnnConfirm(options);
	}

	function setUpTabsModule() {
		$('#dnnTabsModule').dnnPanels()
			.find('.dnnFormExpandContent a').dnnExpandAll({
			    expandText: '<%=Localization.GetSafeJSString("ExpandAll", Localization.SharedResourceFile)%>',
			    collapseText: '<%=Localization.GetSafeJSString("CollapseAll", Localization.SharedResourceFile)%>',
				targetArea: '#dnnTabsModule'
			});

		$("#btnPageSearch", $('#dnnTabsModule')).click(function(e) {
			searchPages($("#searchKeyword", $('#dnnTabsModule')).val());
		});
		$("#searchKeyword", $('#dnnTabsModule')).keydown(function(e) {
			if (e.which == 13) {
				$("#btnPageSearch", $('#dnnTabsModule')).click();
				e.preventDefault();
			}
		});
		
		$("#<%=cmdUpdate.ClientID%>").click(function() {
			needReload = true;
			fieldsChanged = false;
		});
	    
        $('input[id$=cmdDeleteModule]').dnnConfirm({
            text: '<%= LocalizeSafeJsString("DeleteItem") %>',
            yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
            noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
        	title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>',
        	isButton: true
        });

		$('#<%=pnlDetails.ClientID%>').find('input:not([readonly]), textarea, select')
            .change(fieldsChangedHandler)
            .keyup(fieldsChangedHandler);

		$('#<%=pnlDetails.ClientID%>').find('.dnnPermissionsGrid .dnnGridItem td img, .dnnPermissionsGrid .dnnGridAltItem td img')
			.click(fieldsChangedHandler);

		setTimeout(function() {
			var tree = $find("<%=ctlPages.ClientID %>");
			tree.add_nodeClicking(function (sender, args) {
				var nodeValue = args.get_node().get_value();
				location.hash = "#" + $("input[type=radio][name$=rblMode]:checked").val() + "&" + nodeValue;

				if (fieldsChanged) {
					args.set_cancel(true);
					needReload = true;
					showConfirmSave();
				}
			});
		}, 0);
		
	}

	var searchPages = function(keyword) {
		var tree = $find("<%=ctlPages.ClientID %>");
		var nodes = tree.get_allNodes();
		for (var i = 0; i < nodes.length; i++) {
			var node = nodes[i];
			if (node.get_value() == "-1") {
				continue;
			}
			if (keyword == "") {
				node.set_visible(true);
				node.collapse();
			} else if (node.get_text().toLowerCase().indexOf(keyword.toLowerCase()) > -1) {
				node.set_visible(true);
				var parent = node.get_parent();
				while (parent.get_value() != "-1") {
					parent.set_visible(true);
					parent.expand();
					parent = parent.get_parent();
				}
			} else {
				node.set_visible(false);
			}
		}
	};

	$(document).ready(function () {
		setUpTabsModule();

		parseSelectedPageFromHash();
		$(window).on('beforeunload', function() {
			if (fieldsChanged) {
				return '<%=Localization.GetSafeJSString("ConfirmSave", LocalResourceFile)%>';
			}
		});

		Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
			if (!needReload && postBackElement && postBackArguments) {
				$('div.dnnFormMessage').remove();
				delayPostBack();
			} else if (needReload) {
				if ($('div.dnnFormMessage.dnnFormValidationSummary').length === 0) {
					location.reload();
				}
			} else {
				setUpTabsModule();
				processMsgQueue();
			}
		});

		Sys.WebForms.PageRequestManager.getInstance().add_initializeRequest(function (sender, args) {
			if (fieldsChanged && isPendingAction(args.get_postBackElement())) {
				args.set_cancel(true);
				showConfirmSave();
			}
		});
	});

	function toggleSection(id, isToggled) {
		$("div[id$='" + id + "']").toggle(isToggled);
	}
} (jQuery, window.Sys));
</script>
<dnnweb:DnnScriptBlock ID="RadScriptBlock1" runat="server">
	<script type="text/javascript">
		function onContextClicking(sender, eventArgs) {
			var id = '<%=ctlContext.ClientID%>';
			var item = eventArgs.get_menuItem();
			var cmd = item.get_value();
		    var attributes = item.get_attributes();
			if (cmd == 'delete' && !attributes.getAttribute("confirm")) {
			    item.get_menu().hide();
			    $("<a href='#' />").dnnConfirm({
			        text: '<%=GetConfirmString()%>',
			        callbackTrue: function () {
			            attributes.setAttribute("confirm", true);
			            item.click();
			        }
			    }).click();
			    eventArgs.set_cancel(true);
			}
			/*get current node to set hash*/
			var nodeValue = eventArgs.get_node().get_value();
			location.hash = "#" + $("input[type=radio][name$=rblMode]:checked").val() + "&" + nodeValue;
		}
		function onContextShowing(sender, eventArgs) {
			var node = eventArgs.get_node();
			var menu = eventArgs.get_menu();
			if (node) {
				var a = node.get_attributes();

				menu.findItemByValue('view').set_visible(a.getAttribute("CanView") == 'True');
				menu.findItemByValue('edit').set_visible(a.getAttribute("CanEdit") == 'True');
				menu.findItemByValue('add').set_visible(a.getAttribute("CanAdd") == 'True');
				menu.findItemByValue('hide').set_visible(a.getAttribute("CanHide") == 'True');
				menu.findItemByValue('show').set_visible(a.getAttribute("CanMakeVisible") == 'True');
				menu.findItemByValue('disable').set_visible(a.getAttribute("CanDisable") == 'True');
				menu.findItemByValue('enable').set_visible(a.getAttribute("CanEnable") == 'True');
				menu.findItemByValue('delete').set_visible(a.getAttribute("CanDelete") == 'True');
				menu.findItemByValue('makehome').set_visible(a.getAttribute("CanMakeHome") == 'True');
			}
		}
	</script>
</dnnweb:DnnScriptBlock>
<div class="dnnForm dnnTabsModule dnnClear" id="dnnTabsModule">
	<div class="dnnTreeArea">
		<asp:Panel ID="pnlHost" runat="server">
			<div class="dnnFormItem">                
				<asp:Label ID="lblHostOnly" runat="server" resourcekey="lblHostOnly" AssociatedControlID="rblMode" />
                <div class="dnnHSRadioButtons" style="margin-top :5px; float: none; display: block; margin-bottom: 0;" >
				    <asp:RadioButtonList ID="rblMode" runat="server" AutoPostBack="true" RepeatLayout="Flow" RepeatDirection="Vertical">
					    <asp:ListItem Value="P" Selected="True" />
					    <asp:ListItem Value="H" />
				    </asp:RadioButtonList>
                </div>
			</div>		
		</asp:Panel>
		<div class="dnnFormItem">
			<input id="searchKeyword" type="text" /> <a id="btnPageSearch" class="dnnSecondaryAction"><%=LocalizeString("Search") %></a>
		</div>
		<div class="dnnTreeExpand">
			<asp:LinkButton ID="cmdExpandTree" runat="server" CommandName="Expand" />
		</div>
		<dnnweb:DnnTreeView ID="ctlPages" cssclass="dnnTreePages" runat="server" AllowNodeEditing="true"
		 OnClientContextMenuShowing="onContextShowing" OnClientContextMenuItemClicking="onContextClicking"
		  EnableDragAndDropBetweenNodes="true">
			<ContextMenus>                
				<dnnweb:DnnTreeViewContextMenu ID="ctlContext" runat="server">
					<Items>                                            
						<dnnweb:DnnMenuItem Text="View" Value="view" />
						<dnnweb:DnnMenuItem Text="Edit" Value="edit" />
						<dnnweb:DnnMenuItem Text="Delete" Value="delete" />
						<dnnweb:DnnMenuItem Text="Add" Value="add" />
						<dnnweb:DnnMenuItem Text="Hide Page in Menu" Value="hide" />
						<dnnweb:DnnMenuItem Text="Show Page in Menu" Value="show" />
						<dnnweb:DnnMenuItem Text="Enable Page" Value="enable" />
						<dnnweb:DnnMenuItem Text="Disable Page" Value="disable" />
						<dnnweb:DnnMenuItem Text="Make Homepage" Value="makehome" />
					</Items>
				</dnnweb:DnnTreeViewContextMenu>               
			</ContextMenus>
		</dnnweb:DnnTreeView>
		<div class="dnnTreeLegend">
			<h3><asp:Label ID="lblLegend" runat="server" resourcekey="lblLegend" /></h3>
			<div class="dtlItem">
				<img runat="server" src="images/Icon_Home.png" alt="" />
				<asp:Literal ID="lblHome" runat="server" />
			</div>
			<div class="dtlItem">
				<img runat="server" src="images/Icon_Everyone.png" alt="" />
				<asp:Literal ID="lblEveryone" runat="server" />
			</div>
			<div class="dtlItem">
				<img runat="server" src="images/Icon_User.png" alt="" />
				<asp:Literal ID="lblRegistered" runat="server" />
			</div>
			<div class="dtlItem">
				<img runat="server" src="images/Icon_UserSecure.png" alt="" />
				<asp:Literal ID="lblSecure" runat="server" />
			</div>
			<div class="dtlItem">
				<img runat="server" src="images/Icon_UserAdmin.png" alt="" />
				<asp:Literal ID="lblAdminOnly" runat="server" />
			</div>
			<div class="dtlItem">
				<img runat="server" src="images/Icon_Hidden.png" alt="" />
				<asp:Literal ID="lblHidden" runat="server" />
			</div>                
			<div class="dtlItem">
				<img runat="server" src="images/Icon_Disabled.png" alt="" />
				<asp:Literal ID="lblDisabled" runat="server" />
			</div>
			<div class="dtlItem">
				<img runat="server" src="images/Icon_Redirect.png" alt="" />
				<asp:Literal ID="lblRedirect" runat="server" />
			</div>
		</div>
	</div>        
	<div class="tmTabContainer" runat="server" visible="false" id="pnlDetails">
		<div class="dnnFormExpandContent"><a href=""><%=Localization.GetString("ExpandAll", Localization.SharedResourceFile)%></a></div>
		<%--<div class="dnnFormItem dnnFormHelp dnnClear"><p class="dnnFormRequired"><span><%=LocalizeString("RequiredFields")%></span></p></div>--%>
		<div class="ssasContent dnnClear">
			<h2 id="Panel-Common" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Common.Tabname")%></a></h2>
			<fieldset>
				<div class="dnnFormItem">
					<dnn:Label ID="lblName" runat="server" Suffix=":" CssClass="dnnFormRequired" ControlName="txtName"  />
					<asp:TextBox ID="txtName" runat="server" MaxLength="200" ValidationGroup="Page" />
					<asp:RequiredFieldValidator ID="valName" runat="server" EnableClientScript="True" Display="Dynamic" resourcekey="valName.ErrorMessage" ControlToValidate="txtName" CssClass="dnnFormMessage dnnFormError" ValidationGroup="Page"/>
				</div>
				<div class="dnnFormItem">
					<dnn:Label ID="lblTitle" runat="server" suffix=":" ControlName="txtTitle" />
					<asp:TextBox ID="txtTitle" runat="server" />
				</div>
				<div class="dnnFormItem">
					<dnn:Label ID="lblVisible" runat="server" suffix="?" ControlName="chkVisible" />
					<asp:CheckBox ID="chkVisible" runat="server" />
				</div>
				<div class="dnnFormItem">
					<dnn:Label ID="lblDisabledPage" runat="server" suffix="?" ControlName="chkDisabled" />
					<asp:CheckBox ID="chkDisabled" runat="server" />
				</div>
				<div class="dnnFormItem">
					<dnn:Label ID="lblPageSSL" runat="server" suffix="?" ControlName="chkSecure" />
					<asp:CheckBox ID="chkSecure" runat="server" />
				</div> 
				<div class="dnnFormItem">
					<dnn:Label ID="lblAllowIndex" runat="server" ControlName="chkAllowIndex" />
					<asp:CheckBox ID="chkAllowIndex" runat="server" />
				</div>  
			</fieldset>
		</div>    														 
		<div id="PermissionsSection" class="ssasContent dnnClear" runat="server">
			<h2 id="Panel-Permissions" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Permissions.Tabname")%></a></h2>
			<fieldset>
				<dnn:TabPermissionsGrid ID="dgPermissions" runat="server" />
			</fieldset>
		</div>
		<div class="ssasContent dnnClear">
			<h2 id="Panel-Modules" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Modules.Tabname")%></a></h2>
			<fieldset>
				<dnnweb:DnnGrid ID="grdModules" runat="server" AllowPaging="false" AllowSorting="false">
					<MasterTableView AutoGenerateColumns="false">
						<Columns>
							<dnnweb:DnnGridTemplateColumn HeaderText="ModuleTitle">
								<ItemTemplate>
									<%#DataBinder.Eval(Container.DataItem, "ModuleTitle")%>
								</ItemTemplate>
							</dnnweb:DnnGridTemplateColumn>
							<dnnweb:DnnGridTemplateColumn HeaderText="Module">
								<ItemTemplate>
									<%#DataBinder.Eval(Container.DataItem, "FriendlyName")%>
								</ItemTemplate>
							</dnnweb:DnnGridTemplateColumn>
							<dnnweb:DnnGridTemplateColumn HeaderText="Options">
								<ItemTemplate>
									<dnnweb:DnnImageButton ID="cmdDeleteModule" runat="server" CommandArgument='<%#DataBinder.Eval(Container.DataItem, "ModuleId")%>' OnClick="CmdDeleteModuleClick" IconKey="Delete" resourcekey="cmdDelete" />
									<a href="<%#ModuleEditUrl((int)DataBinder.Eval(Container.DataItem, "ModuleId"))%>">
										<dnnweb:DnnImage ID="imgEdit" runat="server" IconKey="Edit" resourcekey="Edit" />
									</a>
								</ItemTemplate>
							</dnnweb:DnnGridTemplateColumn>
						</Columns>
						<NoRecordsTemplate>
							<div class="dnnFormMessage dnnFormWarning">
								<asp:Label ID="lblNoRecords" runat="server" resourcekey="lblNoRecords" />
							</div>
						</NoRecordsTemplate>
					</MasterTableView>
				</dnnweb:DnnGrid>
			</fieldset>
		</div>
		<div class="ssasContent dnnClear">
			<h2 id="Panel-SEO" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("SEO.Tabname")%></a></h2>
			<fieldset>
                <div class="dnnFormItem">
					<dnn:Label ID="lblSitemapPriority" runat="server" suffix=":" CssClass="dnnFormRequired" ControlName="txtSitemapPriority" />
                    <asp:TextBox ID="txtSitemapPriority" runat="server" MaxLength="10" />
                    <asp:RequiredFieldValidator ID="val2" runat="server" ControlToValidate="txtSitemapPriority"
                        Display="Dynamic" CssClass="dnnFormMessage dnnFormError" resourcekey="valPriority" />
                </div>

				<div class="dnnFormItem">
					<dnn:Label ID="lblDescription" runat="server" suffix=":" ControlName="txtDescription" />
					<asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Height="40px" />
				</div>
				<div class="dnnFormItem">
					<dnn:Label ID="lblKeywords" runat="server" suffix=":" ControlName="txtKeywords" />
					<asp:TextBox ID="txtKeywords" runat="server" TextMode="MultiLine" Height="40px" />
				</div>
				<div class="dnnFormItem">
					<dnn:Label ID="lblTags" runat="server" suffix=":" ControlName="termsSelector" AssociateFormat="{0}_Input" />
					<dnnweb:TermsSelector ID="termsSelector" runat="server" IncludeTags="False" />
				</div>
			</fieldset>
		</div>
		<div class="ssasContent dnnClear">
			<h2 id="Panel-Meta" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Metatags.Tabname")%></a></h2>
			<fieldset>
				<div class="dnnFormItem">
					<dnn:Label ID="lblMetaRefresh" runat="server" suffix=":" ControlName="txtRefresh" />
					<asp:TextBox ID="txtRefresh" runat="server" ValidationGroup="Page" />
                    <asp:RegularExpressionValidator ID="valRefresh" runat="server" ControlToValidate="txtRefresh" ValidationGroup="Page"
                        resourcekey="valRefresh.ErrorMessage" ValidationExpression="^\d+$" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" />
				</div>
				<div class="dnnFormItem">
					<dnn:Label ID="lblMetaHead" runat="server" suffix=":" ControlName="txtMeta" />
					<asp:TextBox ID="txtMeta" runat="server" TextMode="MultiLine" Height="40px" />
				</div>
			</fieldset>
		</div>
		<div class="ssasContent dnnClear">
			<h2 id="Panel-Appearance" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Appearance.Tabname")%></a></h2>
			<fieldset>
				<div class="dnnFormItem">
					<dnn:Label ID="lblSkin" runat="server" suffix=":" ControlName="drpSkin" AssociateFormat="{0}_Input" />
                    <dnnweb:DnnSkinComboBox ID="drpSkin" runat="server" />
				</div>
				<div class="dnnFormItem">
					<dnn:Label ID="lblContainer" runat="server" suffix=":" ControlName="drpContainer" AssociateFormat="{0}_Input" />
                    <dnnweb:DnnSkinComboBox ID="drpContainer" runat="server"  />
					
				</div>
                <div class="dnnFormItem">
                    <div class="dnnLabel"></div>
                    <asp:LinkButton ID="cmdCopySkin" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCopySkin" />
                </div>
				<div class="dnnFormItem">
					<dnn:Label ID="lblIconLarge" runat="server" suffix=":" ControlName="ctlIconLarge" />
					<dnn:URL ID="ctlIconLarge" runat="server" ShowLog="False" ShowTrack="false" />
				</div>
				<div class="dnnFormItem">
					<dnn:Label ID="lblIconSmall" runat="server" suffix=":" ControlName="ctlIcon" />
					<dnn:URL ID="ctlIcon" runat="server" ShowLog="False" />
				</div>
			</fieldset>
		</div>
		<div class="ssasContent dnnClear">
			<h2 id="Panel-Link" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Link.Tabname")%></a></h2>
			<fieldset>
				<div class="dnnFormItem">
					<dnn:Label ID="lblUrl" runat="server" suffix=":" ControlName="ctlURL" />
					<dnn:URL ID="ctlURL" runat="server" ShowLog="False" ShowNone="True" ShowTrack="False" ShowNewWindow="True" />
				</div>
				<div class="dnnFormItem">
					<dnn:Label ID="lblPermanentRedirect" runat="server" suffix=":" ControlName="chkPermanentRedirect" />
					<asp:CheckBox ID="chkPermanentRedirect" runat="server" />
				</div>
			</fieldset>
		</div>
		<ul class="dnnActions dnnClear">
			<li><asp:LinkButton ID="cmdUpdate" runat="server" resourcekey="cmdUpdate" CssClass="dnnPrimaryAction" ValidationGroup="Page" CausesValidation="True" /></li>
			<li><asp:HyperLink ID="cmdMore" runat="server" resourcekey="cmdMore" CssClass="dnnSecondaryAction" /></li>
		</ul>     
	</div>
	<div runat="server" visible="false" id="pnlBulk" class="tmTabContainer BulkContainer">
		<div class="dnnFormMessage dnnFormInfo"><asp:Literal ID="lblBulkIntro" runat="server" /></div>
		<div class="dnnFormItem">
            <dnn:Label ID="bulkPagesLabel" runat="server" ControlName="txtBulk" />
            <asp:TextBox ID="txtBulk" runat="server" TextMode="MultiLine" />
        </div>
		<ul class="dnnActions dnnClear">
			<li><asp:LinkButton ID="btnBulkCreate" runat="server" resourcekey="btnBulkCreate" CssClass="dnnPrimaryAction" /></li>
		</ul>
	</div>
</div>
<script language="javascript" type="text/javascript">
    /*globals jQuery, window, Sys */
    (function ($, Sys) {
        $(document).ready(function () {

            function setupPrioritySpinner() {
                var updatePrioritySpinner = function (clientId) {
                    var txtCtrl = $('#' + clientId);
                    var defaultVal = txtCtrl.val();
                    txtCtrl.dnnSpinner({
                        type: 'list',
                        defaultVal: defaultVal,
                        typedata: { list: '1,0.9,0.8,0.7,0.6,0.5,0.4,0.3,0.2,0.1,0' }
                    });
                };

                updatePrioritySpinner('<%= txtSitemapPriority.ClientID %>');
            }

            setupPrioritySpinner();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setupPrioritySpinner();
            });
        });
    }(jQuery, window.Sys));
</script>
