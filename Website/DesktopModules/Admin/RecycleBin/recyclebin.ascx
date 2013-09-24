<%@ Control Language="C#" AutoEventWireup="false" Inherits="DesktopModules.Admin.RecycleBin.RecycleBin" CodeFile="RecycleBin.ascx.cs" %>
<div class="dnnForm dnnRecycleBin dnnClear" id="dnnRecycleBin">
    <ul class="dnnAdminTabNav dnnClear">
		<li><a href="#rbTabs"><%=LocalizeString("Tabs")%></a></li>
		<li><a href="#rbModules"><%=LocalizeString("Modules")%></a></li>
	</ul>
    <div id="divMode" runat="server">
        <asp:RadioButtonList ID="modeButtonList" runat="server" RepeatDirection="Horizontal" AutoPostBack="true" CssClass="dnnFormRadioButtons">
            <asp:ListItem Value="ALL" Selected="True" resourcekey="allLocales" />
            <asp:ListItem Value="SINGLE" resourcekey="singleLocale" />
        </asp:RadioButtonList>
    </div>
    <div class="rbTabs dnnClear" id="rbTabs">
        <div class="rbtContent dnnClear">
            <fieldset>
                <div class="dnnFormItem">
                    <asp:ListBox ID="tabsListBox" runat="server" Width="450px" Rows="14" 
                            DataTextField="IndentedTabName" DataValueField="TabId" 
                            SelectionMode="Multiple" />
                </div>
                <div runat="server" id="divTabButtons">
                    <ul class="dnnActions">
                        <li><asp:LinkButton ID="cmdRestoreTab" runat="server" resourcekey="cmdRestoreTab" CssClass="dnnSecondaryAction cmdRestoreTab" /></li>
                        <li><asp:LinkButton ID="cmdDeleteTab" runat="server" resourcekey="cmdDeleteTab" CssClass="dnnSecondaryAction cmdDeleteTab" /></li>
                    </ul>     
                </div>
            </fieldset>
        </div>
    </div>
    <div class="rbModules dnnClear" id="rbModules">
        <div class="rbmContent dnnClear">
            <fieldset>
                <div class="dnnFormItem"><asp:ListBox ID="modulesListBox" runat="server" Width="450px" Rows="14" SelectionMode="Multiple" /></div>
                <div id="divModuleButtons" runat="server">
                    <ul class="dnnActions">
                        <li><asp:LinkButton ID="cmdRestoreModule" runat="server" resourcekey="cmdRestoreModule" CssClass="dnnSecondaryAction" /></li>
                        <li><asp:LinkButton ID="cmdDeleteModule" runat="server" resourcekey="cmdDeleteModule" CssClass="dnnSecondaryAction cmdDeleteModule" /></li>
                    </ul>     
                </div>
            </fieldset>
        </div>
    </div>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton ID="cmdEmpty" resourcekey="cmdEmpty" CssClass="dnnPrimaryAction dnnEmptyBin" runat="server" /></li>
    </ul>
</div>

<script type="text/javascript">
	(function ($, Sys) {
		var setUpRecycleBin = function() {
			$('#dnnRecycleBin').dnnTabs();

			var yesText = '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>';
			var noText = '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>';
			var titleText = '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>';
			$('#<%= cmdEmpty.ClientID %>').dnnConfirm({
				text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("DeleteAll")) %>',
				yesText: yesText,
				noText: noText,
				title: titleText
			});
			$('#<%= cmdDeleteTab.ClientID %>').dnnConfirm({
				text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("DeleteTab")) %>',
				yesText: yesText,
				noText: noText,
				title: titleText
			});
			$('#<%= cmdDeleteModule.ClientID %>').dnnConfirm({
				text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("DeleteModule")) %>',
				yesText: yesText,
				noText: noText,
				title: titleText
			});

			var restoreTab = $('#<%= cmdRestoreTab.ClientID %>');
			var tabListbox = $('#<%=tabsListBox.ClientID%>')[0];
			restoreTab.click(function (e) {
				if (tabListbox.value.length == 0) {
					$.dnnAlert({
						text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("NoTabSelected")) %>'
					});
					e.preventDefault();
					e.stopImmediatePropagation();
				}
			}).dnnConfirm({
				text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("RestoreTab")) %>',
				yesText: yesText,
				noText: noText,
				title: titleText,
				callbackTrue: function () {
					selectChildPages(tabListbox);
					window.location.href = restoreTab.attr("href");
				},
				callbackFalse: function() {
					window.location.href = restoreTab.attr("href");
				}
			});
		};

		var selectChildPages = function(tabListbox, parentId) {
			for (var i = 0; i < tabListbox.options.length; i++) {
				var option = tabListbox.options[i];
				if (typeof parentId != "undefined") {
					if (option.getAttribute("ParentId") == parentId) {
						option.selected = true;
						selectChildPages(tabListbox, option.value);
					}
				} else if (option.selected) {
					selectChildPages(tabListbox, option.value);
				}
			}
		};

		$(document).ready(function () {
			setUpRecycleBin();
			Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
				setUpRecycleBin();
			});

		});
	} (jQuery, window.Sys));
	
</script>