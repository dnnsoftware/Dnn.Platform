<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Portals.Template" CodeFile="Template.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<div class="dnnForm dnnExportPortal dnnClear" id="dnnExportPortal">
    <h2 id="H1" class="dnnFormSectionHead">
        <a href="">
            <%=LocalizeString("BasicSettings")%></a></h2>
    <fieldset>
        <div class="dnnFormItem">
            <dnn:label id="plPortals" controlname="cboPortals" runat="server" />
            <dnn:dnncombobox id="cboPortals" runat="server" autopostback="true" causesvalidation="False" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plTemplateName" controlname="txtTemplateName" runat="server" cssclass="dnnFormRequired" />
            <asp:TextBox ID="txtTemplateName" runat="server" EnableViewState="False" />
            <asp:RequiredFieldValidator ID="valFileName" runat="server" ControlToValidate="txtTemplateName" CssClass="dnnFormMessage dnnFormError" Display="Dynamic" resourcekey="valFileName.ErrorMessage" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plDescription" controlname="txtDescription" runat="server" cssclass="dnnFormRequired" />
            <asp:TextBox ID="txtDescription" runat="server" EnableViewState="False" TextMode="MultiLine" Rows="10" />
            <asp:RequiredFieldValidator ID="valDescription" runat="server" ControlToValidate="txtDescription" Display="Dynamic" CssClass="dnnFormMessage dnnFormError" resourcekey="valDescription.ErrorMessage" />
        </div>
    </fieldset>
    <h2 id="dnnSettings" class="dnnFormSectionHead">
        <a href="#" class="">
            <%=LocalizeString("Settings")%></a></h2>
    <fieldset>
        <div class="dnnFormItem">
            <dnn:label id="plContent" runat="server" controlname="chkContent" />
            <asp:CheckBox ID="chkContent" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblFiles" runat="server" controlname="chkFiles" />
            <asp:CheckBox ID="chkFiles" runat="server" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblRoles" runat="server" controlname="chkRoles" />
            <asp:CheckBox ID="chkRoles" runat="server" Checked="true" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblProfile" runat="server" controlname="chkProfile" />
            <asp:CheckBox ID="chkProfile" runat="server" Checked="true" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblModules" runat="server" controlname="chkModules" />
            <asp:CheckBox ID="chkModules" runat="server" Checked="true" />
        </div>
        <div class="dnnFormItem" id="rowMultiLanguage" runat="server">
            <dnn:label id="lblMultilanguage" runat="server" controlname="chkMultilanguage" />
            <asp:CheckBox ID="chkMultilanguage" runat="server" AutoPostBack="true" OnCheckedChanged="chkMultilanguage_OnCheckedChanged" />
        </div>
        <div class="dnnFormItem" id="rowLanguages" runat="server">
            <dnn:label id="lblLanguages" runat="server" controlname="chkLanguages" />
            <div id="MultiselectLanguages" runat="server" class="dnnLeft">
                <asp:CheckBoxList ID="chkLanguages" runat="server" CssClass="dnnFormRadioButtons"
                    RepeatColumns="2">
                </asp:CheckBoxList>
                <div class="dnnClear"></div>
                <p>
                    <em>
                        <asp:Label ID="lblNote" runat="server"></asp:Label></em>
                </p>
            </div>
            <div id="SingleSelectLanguages" runat="server" class="dnnLeft" visible="false">
                <dnn:DnnLanguageComboBox ID="languageComboBox" runat="server" LanguagesListType="Enabled" cssClass="dnnLanguageCombo" ShowModeButtons="true" IncludeNoneSpecified="False" AutoPostBack="True" OnItemChanged="languageComboBox_OnItemChanged"/>
                 <p>
                    <em>
                        <asp:Label ID="lblNoteSingleLanguage" runat="server"></asp:Label></em>
                </p>
               <div class="dnnClear"></div>
            </div>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="lblPages" runat="server" />
            <div class="dnnLeft">
                <dnn:dnntreeview id="ctlPages" cssclass="dnnTreePages" runat="server" checkchildnodes="true"
                    checkboxes="true" tristatecheckboxes="True">
                    </dnn:dnntreeview>
            </div>
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdExport" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdExport" /></li>
        <li>
            <asp:LinkButton ID="cmdCancel" runat="server" CssClass="dnnSecondaryAction" resourcekey="cmdCancel" CausesValidation="False" /></li>
    </ul>
</div>
<script type="text/javascript">
    (function ($, Sys) {
        function setupExportPortal() {
            $('#dnnExportPortal').dnnPanels();
        }

        $(document).ready(function () {
            setupExportPortal();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setupExportPortal();
            });
        });

    }(jQuery, window.Sys));

</script>
