<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Languages.LanguagePackWriter" CodeFile="LanguagePackWriter.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<div class="dnnForm dnnLanguagePackWriter dnnClear" id="dnnLanguagePackWriter">
    <fieldset>
        <div class="dnnFormItem">
            <dnn:label id="lbLocale" text="Resource Locale" controlname="cboLanguage" runat="server" />
            <%--<asp:dropdownlist id="cboLanguage" runat="server" />--%>
            <dnn:DnnComboBox id="cboLanguage" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblType" text="Resource Locale" controlname="cboLanguage" runat="server"/>
            <asp:radiobuttonlist id="rbPackType" runat="server" AutoPostBack="true" Repeatdirection="Horizontal" CssClass="lpwRadioButtons dnnFormRadioButtons" RepeatLayout="Flow">
				<asp:ListItem resourcekey="Core.LangPackType" Value="Core" Selected="true">Core</asp:ListItem>
				<asp:ListItem resourcekey="Module.LangPackType" Value="Module">Module</asp:ListItem>
				<asp:ListItem resourcekey="Provider.LangPackType" Value="Provider">Provider</asp:ListItem>
				<asp:ListItem resourcekey="AuthSystem.LangPackType" Value="AuthSystem">Authentication System</asp:ListItem>
				<asp:ListItem resourcekey="Full.LangPackType" Value="Full">Full</asp:ListItem>
			</asp:radiobuttonlist>
        </div>
        <div  id="rowitems" runat="server" class="dnnFormItem" style="padding-top: 15px; ">
 		    <asp:label id="lblItems" runat="server" CssClass="dnnFormLabel"></asp:label>
			<asp:checkboxlist id="lstItems" runat="server" RepeatColumns="1" RepeatDirection="Horizontal" CssClass="lpwCheckBoxes dnnLeft" />
        </div>
        <div id="rowFileName" runat="server" class="dnnFormItem" style="padding-top: 15px; ">
            <dnn:label id="lblName" text="Resource Locale" controlname="cboLanguage" runat="server"></dnn:label>
			<asp:Label id="Label2" runat="server">ResourcePack.</asp:Label><asp:textbox id="txtFileName" runat="server" CssClass="txtFileName" >Core</asp:textbox>
			<asp:Label id="lblFilenameFix" runat="server">.&lt;version&gt;.&lt;locale&gt;.zip</asp:Label>
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
    	<li><asp:LinkButton id="cmdCreate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdCreate"/></li>
    	<li><asp:LinkButton id="cmdCancel" runat="server" CssClass="dnnSecondaryAction" ResourceKey="cmdCancel" CausesValidation="false" /></li>
    </ul>
</div>
<asp:panel id="pnlLogs" runat="server" Visible="False">
	<dnn:sectionhead id="dshBasic" runat="server" text="Language Pack Log" resourcekey="LogTitle" includerule="true" section="divLog"></dnn:sectionhead>
	<div id="divLog" runat="server">
		<asp:HyperLink id="hypLink" runat="server"></asp:HyperLink>
        <asp:Label id="lblMessage" runat="server"></asp:Label>
	</div>
</asp:panel>