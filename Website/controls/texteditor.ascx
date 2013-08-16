<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.UserControls.TextEditor" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>
<div class="dnnForm dnnTextEditor dnnClear">
    
  
    <asp:Panel id="PanelTextEditor" runat="server" class="dnnTextPanel">
        <asp:Panel id="PanelView" runat="server" class="dnnTextPanelView" >
            <asp:RadioButtonList id="OptView" Runat="server" AutoPostBack="True" RepeatDirection="Horizontal" RepeatLayout="Flow" />
            <dnn:label id="plView" runat="server" controlname="optView" CssClass="dnnFormLabelNoFloat" />                
        </asp:Panel>    
        <div id="DivBasicTextBox" runat="server" class="dnnFormItem" visible="false">
            <asp:TextBox id="TxtDesktopHTML" runat="server" textmode="multiline" rows="12" width="100%" columns="75" />
            <div id="DivBasicRender" runat="server">
                <dnn:label id="plRender" runat="server" controlname="optRender" />
                <asp:RadioButtonList id="OptRender" Runat="server" AutoPostBack="True" RepeatDirection="Horizontal" />
            </div>
        </div>
        <div id="DivRichTextBox" runat="server" visible="false" class="dnnFormItem">
            <asp:PlaceHolder id="PlcEditor" runat="server" />
        </div>
    </asp:Panel>
</div>