<%@ Control language="C#" Inherits="DotNetNuke.Providers.RadEditorProvider.ProviderConfig" AutoEventWireup="false"  Codebehind="ProviderConfig.ascx.cs" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="Telerik" Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/LabelControl.ascx" %>
<asp:Panel class="dnnProviderConfig dnnClear" id="dnnProviderConfig" runat="server">
	<div class="dnnTreeArea">
		<asp:Panel id="pnlSelectProvider" runat="server" class="dnnProviderSelect">
			<h3><asp:Label ID="lblSelectedProvider" runat="server" resourcekey="lblSelectedProvider" /></h3>
			<asp:Label ID="editorState" runat="server" />
			<%--<asp:DropDownList ID="editorList" runat="server" />--%>
		    <dnn:DnnComboBox ID="editorList" runat="server" />
			<asp:LinkButton ID="btnEnable" runat="server" CssClass="dnnPrimaryAction" resourcekey="btnEnable" />
		</asp:Panel>
		<div class="dnnTreePages">
			<Telerik:RadTreeView Runat="server" Id="treeTools" AllowNodeEditing="true" MultipleSelect="false" SingleExpandPath="true" /> 
		</div>  
	</div>     
	<asp:Panel ID="pnlTabContent" runat="server" class="dnnpcTabs">
	    <asp:Panel ID="MessagePanel" class="dnnFormMessage dnnFormValidationSummary" runat="server">
	        <%=LocalizeString("NotCurrentProvider") %>
	    </asp:Panel>                                                                                       
		<asp:Panel ID="pnlEditor" runat="server" CssClass="dnnForm">                   
			<ul class="dnnAdminTabNav dnnClear">
				<li><a href="#dnnEditorConfig"><%=LocalizeString("EditConfigTab")%></a></li>
				<li><a href="#dnnToolbarConfig"><%=LocalizeString("ToolbarConfigTab")%></a></li>
			</ul>
			<div id="dnnEditorConfig">
				<div class="dnnFormExpandContent"><a href=""><%=Localization.GetString("ExpandAll", Localization.SharedResourceFile)%></a></div>
				<asp:PlaceHolder ID="plhConfig" runat="server" />
			</div>
			<div id="dnnToolbarConfig">
				<div class="dnnFormItem">
					<asp:TextBox ID="txtTools" runat="server" TextMode="MultiLine" />
				</div>
			</div>
			<ul class="dnnActions dnnClear">
				<li><asp:LinkButton ID="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" ValidationGroup="Page" resourcekey="cmdUpdate" /></li>
				<li><asp:LinkButton ID="cmdCopy" runat="server" CssClass="dnnSecondaryAction" ValidationGroup="Page" resourcekey="cmdCopy" /></li>
				<li><asp:LinkButton ID="cmdCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" CausesValidation="false" /></li>
				<li><asp:LinkButton ID="cmdDelete" runat="server" CssClass="dnnSecondaryAction dnnDeleteTab" ValidationGroup="Page" resourcekey="cmdDelete" /></li>
			</ul>
		</asp:Panel>                         
		<asp:Panel ID="pnlForm" runat="server" CssClass="dnnForm">
			<fieldset>
				<div class="dnnFormItem">
					<dnn:Label ID="lblMode" runat="server" resourcekey="lblMode" suffix=":" />
					<asp:RadioButtonList ID="rblMode" RepeatDirection="Vertical" runat="server" CssClass="dnnFormRadioButtons">
						<asp:ListItem Value="" Text="Everyone" />
						<asp:ListItem Value="Registered" Text="Registered Users" />                                                                                                                            
						<asp:ListItem Value="Admin" Text="Administrators" />
						<asp:ListItem Value="Host" Text="Host Users" />                                                                                                                                             
					</asp:RadioButtonList>
				</div>
				<div class="dnnFormItem">
					<dnn:Label ID="lblPortal" runat="server" resourcekey="lblPortal" suffix=":" />
					<asp:CheckBox ID="chkPortal" runat="server" AutoPostBack="true" />
				</div>
				<div id="divTabs" runat="server" class="dnnFormItem">
					<dnn:Label ID="lblTabs" runat="server" resourcekey="lblTabs" />
					<div class="dnnLeft"><Telerik:RadTreeView Runat="server" Id="treePages" AllowNodeEditing="false" MultipleSelect="false" SingleExpandPath="false" /></div>
				</div>
			</fieldset>
			<ul class="dnnActions dnnClear">
				<li><asp:LinkButton ID="cmdCreate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdCreate" /></li>
			</ul>                                     
		</asp:Panel> 
	</asp:Panel>                     
</asp:Panel>
<script language="javascript" type="text/javascript">
	/*globals jQuery, window, Sys */
	(function ($, Sys) {
	    function setupDnnProviderConfig() {
	        $('#<%=pnlEditor.ClientID%>').dnnTabs().dnnPanels();
	        $('#dnnEditorConfig .dnnFormExpandContent a').dnnExpandAll({ expandText: '<%=Localization.GetSafeJSString("ExpandAll", Localization.SharedResourceFile)%>', collapseText: '<%=Localization.GetSafeJSString("CollapseAll", Localization.SharedResourceFile)%>', targetArea: '#dnnEditorConfig' });
	        $('.dnnDeleteTab').dnnConfirm({
	            text: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("DeleteItem"))%>',
	            yesText: '<%=Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile)%>',
	            noText: '<%=Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile)%>',
	            title: '<%=Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile)%>'
	        });
	    }

	    function setupSpinner() {
	        $('.SpinnerStepOne').each(function () {
	            var ctrl = $(this);
	            var defaultVal = parseInt(ctrl.val());
	            ctrl.dnnSpinner({
	                type: 'range',
	                defaultVal: defaultVal,
	            	typedata: {min: 0, max: 9999}
	            });
	        });

	        $('.SpinnerStep1024').each(function () {
	            var ctrl = $(this);
	            var defaultVal = parseInt(ctrl.val());
	            ctrl.dnnSpinner({
	                type: 'range',
	                defaultVal: defaultVal,
	                typedata: { min: 1024, interval: 1024, max: 2147482624 }
	            });
	        });
	    }

	    $(document).ready(function () {
	        setupDnnProviderConfig();
	        setupSpinner();
	        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
	            setupDnnProviderConfig();
	            setupSpinner();
	        });
	    });

	} (jQuery, window.Sys));
</script>