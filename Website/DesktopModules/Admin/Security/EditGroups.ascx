<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Security.EditGroups" CodeFile="EditGroups.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnEditRoleGroups dnnClear" id="dnnEditRoleGroups">
    <fieldset>
        <%--<div class="dnnFormItem dnnFormHelp dnnClear"><p class="dnnFormRequired"><span><%=Localization.GetString("RequiredFields", Localization.SharedResourceFile)%></span></p></div>--%>
        <div class="dnnFormItem">
            <dnn:label id="plRoleGroupName" runat="server" controlname="txtRoleGroupName" cssclass="dnnFormRequired" />
			<asp:textbox id="txtRoleGroupName" runat="server" maxlength="50" />
			<asp:requiredfieldvalidator id="valRoleGroupName" cssclass="dnnFormMessage dnnFormError" runat="server" resourcekey="valRoleGroupName" controltovalidate="txtRoleGroupName"  display="Dynamic"/>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plDescription" runat="server" controlname="txtDescription"/>
            <asp:textbox id="txtDescription" cssclass="NormalTextBox" runat="server" maxlength="1000" textmode="MultiLine" rows="10" />
        </div>
    </fieldset >   
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton ID="cmdUpdate" resourcekey="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" /></li>
        <li><asp:LinkButton ID="cmdDelete" resourcekey="cmdDelete" runat="server" CssClass="dnnSecondaryAction dnnDeleteRole" CausesValidation="False" /></li>
        <li><asp:LinkButton ID="cmdCancel" resourcekey="cmdCancel" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" /></li>
    </ul>
</div>