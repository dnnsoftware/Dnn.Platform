<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="Subscriptions.ascx.cs" Inherits="DotNetNuke.Modules.CoreMessaging.Subscriptions" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnSubscriptions" id="dnnSubscriptions">
<asp:Panel runat="server" ID="ScopeWrapper" CssClass="dnnClear">
    <div class="activities-list-container dnnForm">        
        <h2 id="dnnSitePanel-ContentItem" class="dnnFormSectionHead"><a href=""><%=LocalizeString("ManageSubscriptions")%></a></h2>
        <fieldset data-bind=" with: $root.searchController">
            <table class="dnnTableDisplay fixed" id="subscription-table">
                <colgroup>
                    <col class="activities-col-action-name"/>
                    <col class="activities-col-subscribed-description"/>
                    <col class="activities-col-points"/>
                </colgroup>
                <thead>
                    <tr>
                        <th class="activities-col-action-name">
                            <span class="sortable" data-bind="click: sortBySubscriptionType"><%= LocalizeString("SubscriptionType") %></span>
                            <span class="sortArrow" data-bind="click: sortBySubscriptionType, css: sortCssSubscriptionType"></span>
                        </th>
                        <th class="activities-col-subscribed-description">
                            <span class="sortable" data-bind="click: sortByDescription"><%= LocalizeString("SubscribedDescription") %></span>
                            <span class="sortArrow" data-bind="click: sortByDescription, css: sortCssDescription"></span>
                        </th>
                        <th class="activities-col-points">
                            <span><%= LocalizeString("Action") %></span>
                        </th>
                    </tr>
                </thead>
                <tfoot>
                    <tr>
                        <td colspan="3">
                            
                            <div class="subscriptions-pager" data-bind="if: pages().length > 1">
                                <a href="#" data-bind="click: function () { changePage(0) }, css: { disabled: currentPage() == 0 }"><%=LocalizeString("First")%></a>
                                <a href="#" data-bind="click: function () { changePage(currentPage() - 1) }, css: { disabled: currentPage() == 0 }"><%=LocalizeString("Prev")%></a>
                                <ul data-bind="foreach: pages">
                                    <li><a href="#" data-bind="click: function () { $parent.changePage($data - 1) }, text: $data, css: { currentPage: $data - 1 == $parent.currentPage() }"></a></li>
                                </ul>
                                <a href="#" data-bind="click: function () { changePage(currentPage() + 1) }, css: { disabled: currentPage() == lastPage() - 1 }"><%=LocalizeString("Next")%></a>
                                <a href="#" data-bind="click: function () { changePage(lastPage() - 1) }, css: { disabled: currentPage() == lastPage() - 1 }"><%=LocalizeString("Last")%></a>
                            </div>
                            
                            <div class="subscriptions-count" data-bind="visible: totalCount() > 0, text: totalItemsText"></div>

                        </td>
                    </tr>
                </tfoot>
                <tbody data-bind="foreach: results">
                    <tr>
                        <td> 
                            <span data-bind="text: subscriptionType"></span>
                        </td>
                        <td>
                            <span data-bind="text: description"></span>
                        </td>
                        <td>
                            <a href="#" data-bind="click: $root['delete']">
                                <img src='<%= ResolveUrl("~/DesktopModules/CoreMessaging/Images/reply.png") %>' alt="<%= Localization.GetString("Unsubscribe", LocalResourceFile) %>" title="<%= Localization.GetString("Unsubscribe", LocalResourceFile) %>"/>
                            </a>
                        </td>
                    </tr>
                </tbody>
            </table>
        </fieldset>
        <h2 id="dnnSitePanel-Subscriptions" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("EmailDeliverySchedule")%></a></h2>
        <fieldset id="fsFrequency">
            <div class="dnnFormItem">
                <dnn:label id="lblNotificationFreq" runat="server" controlname="ddlNotify" />
                <select data-bind="options: $root.frequencyOptions, optionsValue: 'value', optionsText: 'text', value: $root.notifyFrequency" id="ddlNotify"></select>
            </div>     
            <div class="dnnFormItem">
                <dnn:label id="lblMessageFreq" runat="server" controlname="ddlMsg" />
                <select data-bind="options: $root.frequencyOptions, optionsValue: 'value', optionsText: 'text', value: $root.msgFrequency" id="ddlMsg"></select>
            </div>
            <div class="dnnClear">
                <ul class="dnnActions dnnLeft">
                    <li><a href="#" class="dnnPrimaryAction" data-bind="click: save">Save</a></li>
                </ul>
            </div>
            <div class="dnnClear" style="display:none;" id="divUpdated">
                <div class="dnnFormMessage dnnFormSuccess"><span><%= Localization.GetString("PrefSaved", LocalResourceFile) %></span></div>
            </div>
        </fieldset>
    </div>
</asp:Panel>
</div>
<script type="text/javascript">
    $(document).ready(function() {
        $('#dnnSubscriptions').dnnPanels();
    });
</script>