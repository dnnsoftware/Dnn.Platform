<%@ Control Language="VB" ClassName="_OWNER_._MODULE_._CONTROL_" Inherits="DotNetNuke.Entities.Modules.PortalModuleBase" %>

<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="DotNetNuke.Entities.Modules" %>

<script runat="server">


#Region "Copyright"

' 
' Copyright (c) _YEAR_
' by _OWNER_
' 

#End Region

#Region "Event Handlers"

Protected Overrides Sub OnInit(e As EventArgs)
    MyBase.OnInit(e)
End Sub

Protected Overrides Sub OnLoad(e As EventArgs)
    MyBase.OnLoad(e)

    If Not Page.IsPostBack Then
        txtField.Text = DirectCast(Settings("field"), String)
    End If
End Sub

Protected Sub cmdSave_Click(sender As Object, e As EventArgs) Handles cmdSave.Click
        ModuleController.Instance.UpdateModuleSetting(ModuleId, "field", txtField.Text)
    Skins.Skin.AddModuleMessage(Me, "Update Successful", Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess)
End Sub

Protected Sub cmdCancel_Click(sender As Object, e As EventArgs) Handles cmdCancel.Click

End Sub

#End Region

</script>


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

