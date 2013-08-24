<%@ Control language="C#" Inherits="DotNetNuke.Modules.Html.HtmlModule" CodeBehind="HtmlModule.ascx.cs" AutoEventWireup="false" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke.WebControls" %>
<dnn:DNNLabelEdit id="lblContent" runat="server" cssclass="Normal" enableviewstate="False" MouseOverCssClass="LabelEditOverClassML"
	LabelEditCssClass="LabelEditTextClass" EditEnabled="False" MultiLine="True" RichTextEnabled="True"
	ToolBarId="editorDnnToobar" RenderAsDiv="True" EventName="none" LostFocusSave="False" CallBackType="Simple" ClientAPIScriptPath="" LabelEditScriptPath="" WorkCssClass=""></dnn:DNNLabelEdit>
<DNN:DNNToolBar id="editorDnnToobar" runat="server" CssClass="eipbackimg" ReuseToolbar="true"
	DefaultButtonCssClass="eipbuttonbackimg" DefaultButtonHoverCssClass="eipborderhover">
	<DNN:DNNToolBarButton ControlAction="edit" ID="tbEdit" ToolTip="Edit" CssClass="eipbutton_edit" runat="server"/>
	<DNN:DNNToolBarButton ControlAction="save" ID="tbSave" ToolTip="Save" CssClass="eipbutton_save" runat="server"/>
	<DNN:DNNToolBarButton ControlAction="cancel" ID="tbCancel" ToolTip="Cancel" CssClass="eipbutton_cancel" runat="server"/>
	<DNN:DNNToolBarButton ControlAction="bold" ID="tbBold" ToolTip="Bold" CssClass="eipbutton_bold" runat="server"/>
	<DNN:DNNToolBarButton ControlAction="italic" ID="tbItalic" ToolTip="Italic" CssClass="eipbutton_italic" runat="server"/>
	<DNN:DNNToolBarButton ControlAction="underline" ID="tbUnderline" ToolTip="Underline" CssClass="eipbutton_underline" runat="server"/>
	<DNN:DNNToolBarButton ControlAction="justifyleft" ID="tbJustifyLeft" ToolTip="JustifyLeft" CssClass="eipbutton_justifyleft" runat="server"/>
	<DNN:DNNToolBarButton ControlAction="justifycenter" ID="tbJustifyCenter" ToolTip="JustifyCenter" CssClass="eipbutton_justifycenter" runat="server"/>
	<DNN:DNNToolBarButton ControlAction="justifyright" ID="tbJustifyRight" ToolTip="JustifyRight" CssClass="eipbutton_justifyright" runat="server"/>
	<DNN:DNNToolBarButton ControlAction="insertorderedlist" ID="tbOrderedList" ToolTip="OrderedList" CssClass="eipbutton_orderedlist" runat="server"/>
	<DNN:DNNToolBarButton ControlAction="insertunorderedlist" ID="tbUnorderedList" ToolTip="UnorderedList" CssClass="eipbutton_unorderedlist" runat="server"/>
	<DNN:DNNToolBarButton ControlAction="outdent" ID="tbOutdent" ToolTip="Outdent" CssClass="eipbutton_outdent" runat="server"/>
	<DNN:DNNToolBarButton ControlAction="indent" ID="tbIndent" ToolTip="Indent" CssClass="eipbutton_indent" runat="server"/>
	<DNN:DNNToolBarButton ControlAction="createlink" ID="tbCreateLink" ToolTip="CreateLink" CssClass="eipbutton_createlink" runat="server"/>
</DNN:DNNToolBar>