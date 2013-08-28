<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Modules.CreateModule" CodeFile="CreateModule.ascx.cs" %>

<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>

<div class="dnnForm dnnEditDefinition dnnClear" id="dnnEditDefinition">

    <fieldset>

        <div class="dnnFormItem">

            <dnn:label id="plOwner" runat="server" controlname="txtOwner"/>

            <asp:TextBox ID="txtOwner" Runat="server"  MaxLength="255" />

        </div>

        <div class="dnnFormItem">

            <dnn:label id="plModule" controlname="txtModule" runat="server" />

            <asp:TextBox ID="txtModule" runat="server" />

        </div>

        <div class="dnnFormItem">

            <dnn:label id="plDescription" controlname="txtDescription" runat="server" />

            <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="4" />

        </div>

        <div class="dnnFormItem">

            <dnn:label id="plLanguage" controlname="optLanguage" runat="server" />

	    <asp:RadioButtonList ID="optLanguage" CssClass="dnnFormRadioButtons" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostback="True" />

        </div> 
        <div class="dnnFormItem">

            <dnn:label id="plTemplate" controlname="cboTemplate" runat="server" />

            <asp:DropdownList ID="cboTemplate" runat="server" AutoPostback="True" />
        </div> 
        <div class="dnnFormItem">

            <dnn:label id="plControl" controlname="txtControl" runat="server" />

            <asp:TextBox ID="txtControl" runat="server" />

        </div>

   </fieldset>

    <ul class="dnnActions dnnClear">

        <li><asp:linkbutton id="cmdCreate" resourcekey="cmdCreate" runat="server" cssclass="dnnPrimaryAction"/></li>

        <li><asp:hyperlink id="cmdDocumentation" resourcekey="cmdDocumentation" runat="server" cssclass="dnnSecondaryAction" target="_new"/></li>

    </ul>
    <div class="dnnFormItem">

        <asp:Label ID="lblDescription" runat="server" />
    </div> 
</div>

