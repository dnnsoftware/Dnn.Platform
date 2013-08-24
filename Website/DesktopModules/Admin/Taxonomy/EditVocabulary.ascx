<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="EditVocabulary.ascx.cs" Inherits="DotNetNuke.Modules.Taxonomy.Views.EditVocabulary" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="EditVocabularyControl" Src="Controls/EditVocabularyControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="EditTermControl" Src="Controls/EditTermControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client"%>
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/Admin/Taxonomy/scripts/TaxonomyEditor.js" />
<div class="dnnForm dnnEditVocab dnnClear">
    <asp:Panel ID="pnlVocabTerms" runat="server" class="dnnForm">
        <dnn:EditVocabularyControl ID="editVocabularyControl" runat="server" IsAddMode="false" />
        <div class="dnnFormItem">
            <div class="dnnLabel">
                <label>
                    <dnn:DnnFieldLabel id="termsLabel" runat="server" Text="Terms.Text" ToolTip="Terms.ToolTip" />
                </label>
            </div>            
            <dnn:TermsList id="termsList" runat="server" Width="300px" />
        </div>
        <ul class="dnnActions dnnClear">
            <li><asp:LinkButton ID="saveVocabulary" runat="server" resourcekey="SaveVocabulary" CssClass="dnnPrimaryAction" /></li>
            <li><asp:LinkButton ID="addTermButton" runat="server" resourceKey="AddTerm" CssClass="dnnSecondaryAction" /></li>
            <li><asp:HyperLink ID="cancelEdit" runat="server" resourceKey="cmdCancel" CssClass="dnnSecondaryAction" /></li>
            <li><asp:LinkButton ID="deleteVocabulary" runat="server" resourceKey="DeleteVocabulary" CausesValidation="false" CssClass="dnnSecondaryAction dnnDeleteItem" /></li>
        </ul>
    </asp:Panel>
    <asp:Panel ID="pnlTermEditor" runat="server" Visible="false">
        <h2><asp:Label ID="termLabel" runat="server" /></h2>
        <fieldset>
            <dnn:EditTermControl ID="editTermControl" runat="server" />
            <ul class="dnnActions dnnClear">
                <li><asp:LinkButton ID="saveTermButton" runat="server" CssClass="dnnPrimaryAction" resourcekey="SaveTerm" /></li>
                <li><asp:LinkButton ID="cancelTermButton" runat="server" resourceKey="cmdCancel" CssClass="dnnSecondaryAction" CausesValidation="false" /></li>
                <li><asp:LinkButton ID="deleteTermButton" runat="server" resourceKey="DeleteTerm" CausesValidation="false" CssClass="dnnSecondaryAction dnnDeleteItem" /></li>
            </ul>
        </fieldset>
    </asp:Panel>
</div>

<script language="javascript" type="text/javascript">
    /*globals jQuery, window, Sys */
	(function ($, Sys) {
		function setUpDnnEditVocab() {
			$('.dnnDeleteItem').dnnConfirm({
				text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("DeleteItem")) %>',
				yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
				noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
				title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
			});
			
			var serviceFramework = $.ServicesFramework(<%=ModuleContext.ModuleId %>);
			$(".termNameBox").termComplete({
				serviceFramework: serviceFramework,
				getParent: function () {
					var parentCombo = $find($("div[id$=parentTermCombo]").prop("id"));
					if (parentCombo == null) {
						return -1;
					}
					return parentCombo.get_value();
				},
				saveButton: $("a[id$=saveTermButton]")
			});

			window.parentTermChanged = function() {
				$(".termNameBox").trigger("termVal");
			};
		}

		$(document).ready(function () {
			setUpDnnEditVocab();  
			Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
				setUpDnnEditVocab();
			});
		});

	} (jQuery, window.Sys));
</script>