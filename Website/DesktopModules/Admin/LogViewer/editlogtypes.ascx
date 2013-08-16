<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.LogViewer.EditLogTypes" CodeFile="EditLogTypes.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<div class="dnnForm dnnEditLogTypes" id="dnnEditLogTypes">
    <asp:panel id="pnlLogTypeConfigInfo" runat="server">
	    <dnn:DnnGrid id="dgLogTypeConfigInfo" runat="server" autogeneratecolumns="false">
	        <MasterTableView DataKeyNames="ID">
		        <Columns>
		            <dnn:DnnGridImageCommandColumn CommandName="Edit" IconKey="Edit" UniqueName="EditColumn" />
			        <dnn:DnnGridBoundColumn headertext="LogType" datafield="LogTypeFriendlyName" />
			        <dnn:DnnGridBoundColumn headertext="Portal" datafield="LogTypePortalID" />
			        <dnn:DnnGridBoundColumn headertext="Active" datafield="LoggingIsActive" />
			        <dnn:DnnGridBoundColumn headertext="FileName" datafield="LogFilename" />
		        </Columns>
            </MasterTableView>
	    </dnn:DnnGrid>
	    <ul class="dnnActions dnnClear"><li><asp:HyperLink class="dnnPrimaryAction" id="hlReturn" runat="server" resourcekey="cmdReturn" causesvalidation="False" /></li></ul>
    </asp:panel>
    <asp:panel id="pnlEditLogTypeConfigInfo" runat="server">
        <div class="eltContent dnnClear" id="eltContent">
            <h2 id="Panel-Settings" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Settings")%></a></h2>
            <fieldset>
                <legend></legend>
                <div class="dnnFormItem">
                    <dnn:label id="plIsActive" runat="server" controlname="chkIsActive" suffix=":" />
                    <asp:checkbox id="chkIsActive" runat="server" autopostback="True" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plLogTypeKey" runat="server" controlname="ddlLogTypeKey" suffix=":" />
                    <asp:dropdownlist id="ddlLogTypeKey" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plLogTypePortalID" runat="server" controlname="ddlLogTypePortalID" suffix=":" />
                    <asp:dropdownlist id="ddlLogTypePortalID" runat="server" /> 
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plKeepMostRecent" runat="server" controlname="ddlKeepMostRecent" suffix=":" />
                    <asp:dropdownlist id="ddlKeepMostRecent" runat="server" />
                </div>
            </fieldset>
            <h2 id="Panel-EmailSettings" class="dnnFormSectionHead"><a href=""><%=LocalizeString("EmailSettings")%></a></h2>
            <fieldset>
                <legend></legend>
                <div class="dnnFormItem">
                    <dnn:label id="plEmailNotificationStatus" runat="server" controlname="chkEmailNotificationStatus" suffix=":" />
                    <asp:checkbox id="chkEmailNotificationStatus" runat="server" autopostback="True" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plThreshold" runat="server" controlname="ddlThreshold" suffix=":" />
                    <asp:dropdownlist id="ddlThreshold" runat="server" />
                    <asp:Label id="lblIn" runat="server" resourcekey="In" />
                    <asp:dropdownlist id="ddlThresholdNotificationTime" runat="server">
					    <asp:listitem value="1" />
					    <asp:listitem value="2" />
					    <asp:listitem value="3" />
					    <asp:listitem value="4" />
					    <asp:listitem value="5" />
					    <asp:listitem value="6" />
					    <asp:listitem value="7" />
					    <asp:listitem value="8" />
					    <asp:listitem value="9" />
					    <asp:listitem value="10" />
					    <asp:listitem value="20" />
					    <asp:listitem value="30" />
					    <asp:listitem value="60" />
					    <asp:listitem value="90" />
					    <asp:listitem value="120" />
				    </asp:dropdownlist>
                    <asp:dropdownlist id="ddlThresholdNotificationTimeType" runat="server">
					    <asp:listitem value="1" resourcekey="Seconds">Seconds</asp:listitem>
					    <asp:listitem value="2" resourcekey="Minutes">Minutes</asp:listitem>
					    <asp:listitem value="3" resourcekey="Hours">Hours</asp:listitem>
					    <asp:listitem value="4" resourcekey="Days">Days</asp:listitem>
				    </asp:dropdownlist>
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plMailFromAddress" runat="server" controlname="txtMailFromAddress" suffix=":" />
                    <asp:textbox id="txtMailFromAddress" runat="server" />
                </div>
                <div class="dnnFormItem">
                    <dnn:label id="plMailToAddress" runat="server" controlname="txtMailToAddress" suffix=":" />
                    <asp:textbox id="txtMailToAddress" runat="server" />
                </div>
            </fieldset>
        </div>
	    <ul class="dnnActions dnnClear">
		    <li><asp:linkbutton class="dnnPrimaryAction" id="cmdUpdate" runat="server" resourcekey="cmdUpdate" /></li>
		    <li><asp:linkbutton class="dnnSecondaryAction" id="cmdCancel" runat="server" resourcekey="cmdCancel" causesvalidation="False" /></li>
		    <li><asp:linkbutton class="dnnSecondaryAction dnnLogTypeDelete" id="cmdDelete" runat="server" resourcekey="cmdDelete" causesvalidation="False" /></li>
	    </ul>
		<dnn:DnnScriptBlock ID="scriptBlock1" runat="server">
			<script type="text/javascript">
                /*globals jQuery */
                (function ($) {
				$('#eltContent').dnnPanels();
				var yesText = '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>';
				var noText = '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>';
				var titleText = '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>';
				$('#<%= cmdDelete.ClientID %>').dnnConfirm({
				    text: '<%= Localization.GetSafeJSString("DeleteItem.Text", Localization.SharedResourceFile) %>',
					yesText: yesText,
					noText: noText,
					title: titleText
				});
                } (jQuery));
			</script>
		</dnn:DnnScriptBlock>
    </asp:panel>
</div>