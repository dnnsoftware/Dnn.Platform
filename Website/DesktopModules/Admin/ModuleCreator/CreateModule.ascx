<%@ Control Language="C#" AutoEventWireup="false" Inherits="DesktopModules.Admin.ModuleCreator.CreateModule" CodeFile="CreateModule.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>

<div class="dnnForm dnnEditDefinition dnnClear" id="dnnEditDefinition">
    <asp:PlaceHolder runat="server" ID="createForm">
        <fieldset>
            <div class="dnnFormItem">
                <dnn:Label ID="plOwner" runat="server" ControlName="txtOwner" />
                <asp:TextBox ID="txtOwner" runat="server" MaxLength="255" />
            </div>
            <div class="dnnFormItem">
                <dnn:Label ID="plModule" ControlName="txtModule" runat="server" />
                <asp:TextBox ID="txtModule" runat="server" />
            </div>
            <div class="dnnFormItem">
                <dnn:Label ID="plDescription" ControlName="txtDescription" runat="server" />
                <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="4" />
            </div>
            <div class="dnnFormItem">
                <dnn:Label ID="plLanguage" ControlName="optLanguage" runat="server" />
                <asp:RadioButtonList ID="optLanguage" CssClass="dnnFormRadioButtons" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" />
            </div>
           <div class="dnnFormItem">
                <dnn:Label ID="plTemplate" ControlName="cboTemplate" runat="server" />
                <asp:DropDownList ID="cboTemplate" runat="server" AutoPostBack="True" />
            </div>
            <div class="dnnFormItem">
                <dnn:Label ID="plControl" ControlName="txtControl" runat="server" />
                <asp:TextBox ID="txtControl" runat="server" />
            </div>
        </fieldset>
        <ul class="dnnActions dnnClear">
            <li><asp:LinkButton ID="cmdCreate" resourcekey="cmdCreate" runat="server" CssClass="dnnPrimaryAction" /></li>
        </ul>
        <div class="dnnFormItem">
            <asp:Label ID="lblDescription" runat="server" />
        </div>
    </asp:PlaceHolder>
</div>

