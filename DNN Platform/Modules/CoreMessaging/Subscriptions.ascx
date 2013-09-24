<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="Subscriptions.ascx.cs" Inherits="DotNetNuke.Modules.SubscriptionsMgmt.Subscriptions" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnSubscriptions" id="dnnSubscriptions">
<asp:Panel runat="server" ID="ScopeWrapper" CssClass="dnnClear">
    <div class="activities-list-container dnnForm">
        <h2 id="dnnSitePanel-Subscriptions" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("NotificationMessaging")%></a></h2>
        <fieldset id="fsFrequency">
            <div class="dnnFormItem">
                <dnn:label id="lblNotificationFreq" runat="server" controlname="ddlNotify" />
                <select data-bind="options: $root.subscription.frequencyOptions, optionsValue: 'value', optionsText: 'text', value: $root.subscription.notifyFrequency" id="ddlNotify"></select>
            </div>     
            <div class="dnnFormItem">
                <dnn:label id="lblMessageFreq" runat="server" controlname="ddlMsg" />
                <select data-bind="options: $root.subscription.frequencyOptions, optionsValue: 'value', optionsText: 'text', value: $root.subscription.msgFrequency" id="ddlMsg"></select>
            </div>
            <div class="dnnClear">
                <ul class="dnnActions dnnLeft">
                    <li><a href="#" class="dnnPrimaryAction" data-bind="click: subscription.save">Save</a></li>
                </ul>
            </div>
            <div class="dnnClear" style="display:none;" id="divUpdated">
                <div class="dnnFormMessage dnnFormSuccess"><span><%= Localization.GetString("PrefSaved", LocalResourceFile) %></span></div>
            </div>
        </fieldset>
        <h2 id="dnnSitePanel-ContentItem" class="dnnFormSectionHead"><a href=""><%=LocalizeString("ContentSubscriptions")%></a></h2>
        <fieldset>
            <!-- ko with: $root.subscription.searchController -->
            <table class="dnnTableDisplay fixed" id="subscription-table">
                <colgroup>
                    <col class="activities-col-action-name"/>
                    <col class="activities-col-points"/>
                </colgroup>
                <thead>
                    <tr>
                        <th class="activities-col-action-name"><span><%= LocalizeString("SubscriptionType") %></span></th>
                        <th class="activities-col-points"><span><%= LocalizeString("Action") %></span></th>
                    </tr>
                </thead>
                <tfoot>
                    <tr>
                        <td colspan="2">
                            <span class="dnnResults">
                                <%= LocalizeString("Showing") %>
                                <span data-bind="text: totalResults"></span><%= "&#160;" + LocalizeString("Results") %>
                            </span>
                            <ul class="dnnPagination" id="activitiesPages" data-bind="html: $root.subscription.pagingControl">
                            </ul>
                        </td>
                    </tr>
                </tfoot>
                <tbody>
                    <!-- ko foreach: results -->
                    <tr>
                        <td>
                            <span data-bind="text: activity"></span>
                        </td>
                        <td>
                            <a href="#" data-bind="click: $root.subscription['delete']">
                                <img src='<%= ResolveUrl("~/DesktopModules/CoreMessaging/Images/reply.png") %>' alt="<%= Localization.GetString("Unsubscribe", LocalResourceFile) %>" title="<%= Localization.GetString("Unsubscribe", LocalResourceFile) %>"/>
                            </a>
                        </td>
                    </tr>
                    <!-- /ko -->
                </tbody>
            </table>
            <!-- /ko -->
        </fieldset>
    </div>
</asp:Panel>
</div>
<script type="text/javascript">
	$(document).ready(function() {
		$('#dnnSubscriptions').dnnPanels();
	});
</script>