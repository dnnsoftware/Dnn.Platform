<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Languages.LanguageEditor" CodeFile="LanguageEditor.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnLanguageEditor dnnClear" id="dnnLanguageEditor">
    <div class="dnnLangEditorTree dnnLeft">
        <dnn:Label ID="plResources" runat="server" ControlName="DNNTree" />
        <dnn:DnnTreeView ID="resourceFiles" runat="server"></dnn:DnnTreeView>
    </div>
    <div class="dnnLangEditorContent dnnRight">
        <fieldset>
            <div id="rowMode" runat="server" class="dnnFormItem">
                <dnn:Label ID="plMode" runat="server" Text="Available Locales" ControlName="cboLocales" />
                <asp:RadioButtonList ID="rbMode" runat="server" AutoPostBack="True" RepeatLayout="Flow" RepeatColumns="3" RepeatDirection="Horizontal" CssClass="dnnLEMode dnnFormRadioButtons">
                <asp:ListItem resourcekey="ModeSystem" Value="System" Selected="True" />
                    <asp:ListItem resourcekey="ModeHost" Value="Host" />
                    <asp:ListItem resourcekey="ModePortal" Value="Portal" />
                </asp:RadioButtonList>
            </div>
            <div class="dnnFormItem">
                <dnn:Label ID="highlightLabel" runat="server" ControlName="lblEditingLanguage" />
                <asp:CheckBox ID="chkHighlight" runat="server" AutoPostBack="True" />
             </div>
            <div class="dnnFormItem">
                <dnn:Label ID="plEditingLanguage" runat="server" ControlName="lblEditingLanguage" />
                <dnn:DnnLanguageLabel ID="languageLabel" runat="server" />
            </div>
            <div class="dnnFormItem">
                <dnn:Label ID="plFolder" runat="server" ControlName="lblFolder" />
                <asp:Label ID="lblFolder" runat="server" />
            </div>
            <div class="dnnFormItem">
                <dnn:Label ID="plSelected" runat="server" ControlName="lblResourceFile" />
                <asp:Label ID="lblResourceFile" runat="server" />
            </div>
            <div class="dnnFormItem">
                <dnn:DnnGrid ID="resourcesGrid" runat="server" AutoGenerateColumns="false" Width="100%">
                    <MasterTableView>
                        <ItemStyle VerticalAlign="Top" HorizontalAlign="Center" />
                        <AlternatingItemStyle  VerticalAlign="Top" HorizontalAlign="Center" />
                        <HeaderStyle VerticalAlign="Bottom" HorizontalAlign="Left" Wrap="false" />
                        <Columns>
                            <dnn:DnnGridTemplateColumn>
                                <HeaderTemplate>
                                    <table cellpadding="0" cellspacing="0" class="dnnGrid">
                                        <tr>
                                            <td style="width:45%;"><asp:Label ID="Label5" runat="server" resourcekey="DefaultValue" /></td>
                                            <td style="width:45%;"><asp:Label ID="Label4" runat="server" resourcekey="Value" /></td>
                                            <td></td>
                                        </tr>
                                    </table>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <table cellpadding="0" cellspacing="0" class="dnnGrid">
                                        <tr>
                                            <td colspan="2" style="text-align:left;">
                                                <asp:Label ID="resourceKeyLabel" runat="server" resourcekey="ResourceName" />
                                                <asp:Label ID="resourceKey" runat="server" Text='<%# Eval("key") %>' />
                                            </td>
                                            <td></td>
                                        </tr>
                                        <tr style="vertical-align: top;">
                                            <td style="width:45%;">
                                                <asp:TextBox ID="txtDefault" runat="server" Enabled="false"/>
                                            </td>
                                            <td style="width:45%;">
                                                <asp:TextBox ID="txtValue" runat="server" />                                                
                                            </td>
                                            <td valign="top">
                                                <asp:HyperLink ID="lnkEdit" runat="server" NavigateUrl='<%# OpenFullEditor(Eval("key").ToString()) %>'>
                                                    <dnn:DnnImage runat="server" AlternateText="Edit" ID="imgEdit" IconKey="Edit" resourcekey="cmdEdit" Style="vertical-align:top"></dnn:DnnImage>
                                                </asp:HyperLink>
                                            </td>
                                        </tr>
                                    </table>
                                </ItemTemplate>
                            </dnn:DnnGridTemplateColumn>
                        </Columns>
                    </MasterTableView>
                    <PagerStyle Mode="NextPrevAndNumeric" />
                </dnn:DnnGrid>
            </div>
        </fieldset>
        <ul class="dnnActions dnnClear">
    	    <li><asp:LinkButton id="cmdUpdate" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdUpdate" /></li>
    	    <li><asp:LinkButton id="cmdDelete" runat="server" CssClass="dnnSecondaryAction" ResourceKey="cmdDelete" CausesValidation="false" /></li>
    	    <li><asp:LinkButton id="cmdCancel" runat="server" CssClass="dnnSecondaryAction" ResourceKey="cmdCancel" CausesValidation="false" /></li>
        </ul>    
    </div>
</div>
