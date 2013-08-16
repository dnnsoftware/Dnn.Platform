<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RedirectionManagerView.ascx.cs" Inherits="DotNetNuke.Modules.MobileManagement.RedirectionManagerView" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<div class="dnnForm dnnClear" id="dnnMobileManagement">
	<fieldset>
		<div class="dnnFormItem">
			<asp:RadioButtonList ID="optSimpleAdvanced" CssClass="dnnFormRadioButtons" runat="server" RepeatLayout="Flow">
				<asp:ListItem Value="SimpleSettings" resourcekey="optSimple" Selected="true" />
				<asp:ListItem Value="RedirectionSettings" resourcekey="optAdvanced" />
			</asp:RadioButtonList>
        </div>
      
        <div class="dnnFormItem">
		<ul class="dnnActions dnnClear">
			<li><asp:HyperLink ID="lnkAdd" runat="server" resourcekey="cmdAdd" CssClass="dnnPrimaryAction" /></li>
		</ul>
	    </div>
		
        <div id="dvRedirectionsGrid" runat="server" class="dnnGrid dnnRedirectionGrid">
        	<div><%=LocalizeString("RedirectManagerMessage.Text")%></div>
			<hr />
            <asp:DataGrid ID="RedirectionsGrid" AutoGenerateColumns="false" Width="100%" CellPadding="2" GridLines="None" CssClass="dnnGrid" runat="server">
			    <HeaderStyle CssClass="dnnGridHeader" VerticalAlign="Top" HorizontalAlign="Center" />
			    <ItemStyle CssClass="dnnGridItem" HorizontalAlign="Center" Height="30"  />
			    <AlternatingItemStyle CssClass="dnnGridAltItem" />
			    <EditItemStyle CssClass="dnnFormEditItem" />
			    <SelectedItemStyle CssClass="dnnFormError" />
			    <FooterStyle CssClass="dnnGridFooter" />
			    <PagerStyle CssClass="dnnGridPager" />
			    <Columns>
				    <asp:TemplateColumn HeaderText="" HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder">
					    <ItemTemplate>
						    <img id="imgDragDrop" runat="server" src="~/Icons/Sigma/DragDrop_15x15_Standard.png" alt=""/>
					    </ItemTemplate>
				    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="" HeaderStyle-CssClass="dnnGridHeaderTD-NoBorder">
					    <ItemTemplate>
                            <a href="<%#GetEditUrl(Eval("Id").ToString()) %>"><img src="<%=ResolveUrl("~/icons/sigma/edit_16X16_standard.png") %>" alt="" /></a>
					    </ItemTemplate>
				    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="">
					    <ItemTemplate>
                            <asp:LinkButton ID="btnDel" runat="server" CssClass="delete" CausesValidation="false" CommandArgument='<%#Eval("Id") %>' CommandName="delete" >
                                 <asp:Image ID="image1" runat="server" ImageUrl="~/icons/sigma/delete_16X16_standard.png" />
                            </asp:LinkButton>
					    </ItemTemplate>
				    </asp:TemplateColumn>
				    <asp:TemplateColumn HeaderText="Redirection Rule">
                        <HeaderStyle CssClass="dnnRedirectionRuleHeader" />
                        <ItemStyle  CssClass="dnnRedirectionRule" />
					    <ItemTemplate>
                            <span class="RedirectToolItem"><span class="SortOrder"><%#Eval("SortOrder") %></span>. <%#Eval("Name") %>
								<span class="dnnFormItem dnnTooltip" style="z-index:1; position:absolute; display:none;">
									<span class="dnnFormHelpContent dnnClear" style="width:35em;">
                                        <%#RenderTooltip("source", Eval("Id").ToString()) %><br/>
                                        <%#RenderTooltip("destination", Eval("Id").ToString()) %>
                                    </span>
								</span>							
							</span>
					    </ItemTemplate>
				    </asp:TemplateColumn>
				    <asp:TemplateColumn HeaderText="Device Type">
					    <ItemTemplate>
						    <%#this.LocalizeString(Eval("Type").ToString()) %>
					    </ItemTemplate>
				    </asp:TemplateColumn>
				    <asp:TemplateColumn HeaderText="Enabled">
                        <ItemTemplate>
                	        <div class="dnnRedirectionIsEnabled">
						        <asp:LinkButton id="btnEnable" runat="server" CssClass="enable" CausesValidation="false" CommandArgument='<%#Eval("Id") %>' CommandName="enable">
							        <img src="<%#Convert.ToBoolean(Eval("Enabled")) ? ResolveUrl("~/icons/sigma/checked_16x16_standard.png") : ResolveUrl("~/icons/sigma/cancel_16x16_standard.png") %>" alt="" />
						        </asp:LinkButton>
					        </div>
                        </ItemTemplate>
				    </asp:TemplateColumn>
			    </Columns>
		    </asp:DataGrid>
        </div>

	</fieldset>

</div>
<script type="text/javascript">
    (function ($) {
        $(document).ready(function () {
        	$("#<%=lnkAdd.ClientID %>").click(function (e) {
        		var type = $("input[type=radio][name$=optSimpleAdvanced]:checked").val();
                var href = $(this).attr("href").replace(escape("{0}"), type).replace("{0}", type);
                $(this).attr("href", href);
            });
            $('a.delete').dnnConfirm({
                text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("DeleteItem")) %>',
                yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
                noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
                title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
            });
            $('a.enable').dnnConfirm({
                text: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("EnableItem.Text")) %>',
                yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
                noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
                title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
            });

            // Show/Hide Tooltip
            $('.RedirectToolItem').mouseover(function (e) {
                var container = $("#dnnMobileManagement");
                var posX = eval($(this).offset().left);
                var posY = eval(e.pageY - container.offset().top);
                $(this).find('.dnnFormItem, .dnnToolTip').css({ left: posX, top: posY });
                $(this).find('.dnnFormItem, .dnnToolTip').show();
            });
            $('.RedirectToolItem').mouseout(function () {
                $(this).find('.dnnFormItem, .dnnToolTip').hide();
            });
            
            var listContainer = $("#<%=RedirectionsGrid.ClientID %> tbody");
            var itemFilter = ".dnnGridItem,.dnnGridAltItem,.dnnFormEditItem";
            if (listContainer.find(itemFilter).length > 0) {
                listContainer.sortable({
                    start: function () { $(this).find('.dnnFormItem, .dnnToolTip').hide(); },
                    stop: function (event, ui) {
                        var moveId = ui.item.attr("data");
                        var nextId = "-1";
                        if (ui.item.next().length > 0) {
                            nextId = ui.item.next().attr("data");
                        }
                        var action = "\"action=sort&moveId=" + moveId + "&nextId=" + nextId + "\"";
                        eval(dnn.getVar('ActionCallback').replace("[ACTIONTOKEN]", action));
                    },
                    items: itemFilter,
                    helper: function (event, ui) {
                        var helper = ui.clone(false);
                        helper.find("td").each(function (index) {
                            $(this).width(ui.find("td").eq(index).width());
                        });
                        return helper;
                    },
                    placeholder: "dnnGridItem"
                });
            }            
        });
    })(jQuery);    
    
    (function ($) {
        window.success = function (result, ctx) {
            var listContainer = $("#<%=RedirectionsGrid.ClientID %> tbody");
            var itemFilter = ".dnnGridItem,.dnnGridAltItem,.dnnFormEditItem";
            // Change order numbers on sort
            listContainer.find("span.SortOrder").each(function (index) {
                $(this).html(index + 1);
            });
            // Render row backgrounds on sort
            listContainer.find(itemFilter).each(function (index) {
                $(this).removeClass("dnnGridItem").removeClass("dnnGridAltItem").addClass(index % 2 == 0 ? "dnnGridItem" : "dnnGridAltItem");
            });
        };

        window.error = function (result, ctx) {
            location.reload(true);
        };
    })(jQuery); 
</script>
