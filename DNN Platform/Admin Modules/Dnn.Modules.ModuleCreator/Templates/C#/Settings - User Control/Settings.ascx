<%@ Control Language="C#" AutoEventWireup="false" Inherits="_OWNER_._MODULE_.Settings" CodeFile="Settings.ascx.cs" %>

<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<div class="dnnForm dnnSettings dnnClear" id="dnnSettings">

    <fieldset>

        <div class="dnnFormItem">

            <dnn:label id="plField" runat="server" text="Field" helptext="Enter a value" controlname="txtField" />

            <asp:textbox id="txtField" runat="server" maxlength="255" />

        </div>

   </fieldset>

</div>

