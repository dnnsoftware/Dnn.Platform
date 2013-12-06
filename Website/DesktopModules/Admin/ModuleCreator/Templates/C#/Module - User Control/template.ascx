<%@ Control Language="C#" AutoEventWireup="false" Inherits="[OWNER].[MODULE].[CONTROL]" CodeFile="[CONTROL].ascx.cs" %>

<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<div class="dnnForm dnnEdit dnnClear" id="dnnEdit">

    <fieldset>

        <div class="dnnFormItem">

            <dnn:label id="plField" runat="server" text="Field" helptext="Enter a value" controlname="txtField" />

            <asp:textbox id="txtField" runat="server" maxlength="255" />

        </div>

   </fieldset>

    <ul class="dnnActions dnnClear">

        <li><asp:linkbutton id="cmdSave" text="Save" runat="server" cssclass="dnnPrimaryAction" /></li>

        <li><asp:linkbutton id="cmdCancel" text="Cancel" runat="server" cssclass="dnnSecondaryAction" /></li>

    </ul>

</div>

