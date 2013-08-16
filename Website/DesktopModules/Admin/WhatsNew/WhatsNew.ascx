<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Host.WhatsNew" CodeFile="WhatsNew.ascx.cs" %>
<p id="header" runat="server"></p>
<div class="dnnForm dnnWhatsNew dnnClear" id="dnnWhatsNew">
    <div class="dnnFormExpandContent"><a href=""><%=Localization.GetString("ExpandAll", Localization.SharedResourceFile)%></a></div>
    <asp:Repeater ID="WhatsNewList" runat="server">
        <ItemTemplate>
            <div class="wnContent dnnClear">
                <h2 id="Panel-<%# Eval("Version") %>" class="dnnFormSectionHead"><a href="" class=""><%# Eval("Version") %></a></h2>
                <fieldset>
                    <legend></legend>
                    <div class="dnnFormItem">
                        <%#Eval("Notes")%>
                    </div>
                </fieldset>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>
<p id="footer" runat="server"></p>
<script language="javascript" type="text/javascript">
/*globals jQuery, window, Sys */
(function ($, Sys) {
    $(document).ready(function () {
        $('#dnnWhatsNew').dnnPanels();
        $('#dnnWhatsNew .dnnFormExpandContent a').dnnExpandAll({
            expandText: '<%=Localization.GetSafeJSString("ExpandAll", Localization.SharedResourceFile)%>',
            collapseText: '<%=Localization.GetSafeJSString("CollapseAll", Localization.SharedResourceFile)%>',
            targetArea: '#dnnWhatsNew'
        });
    });
} (jQuery, window.Sys));
</script>