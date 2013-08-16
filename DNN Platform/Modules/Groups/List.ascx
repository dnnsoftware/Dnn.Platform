<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="List.ascx.cs" Inherits="DotNetNuke.Modules.Groups.List" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Modules.Groups" Namespace="DotNetNuke.Modules.Groups.Controls" %>
<div class="dnnForm dnnClear dnnGroupDirectory">
    <div class="dgdMainContent">

        <asp:Panel ID="panelSearch" runat="server" CssClass="dnnFormItem dnnClear" DefaultButton="btnSearch" Width="450px">
            <asp:TextBox ID="txtFilter" runat="server" CssClass="dnnFixedSizeComboBox" />
            <asp:LinkButton ID="btnSearch" runat="server" resourcekey="SearchButton"
                CssClass="dnnPrimaryAction" OnClick="btnSearch_Click" ValidationGroup="searchGroups" />
            <a href="<%= GetClearFilterUrl() %>">clear</a>
            <asp:RequiredFieldValidator runat="server" ID="reqSearch" ControlToValidate="txtFilter"
                ValidationGroup="searchGroups" CssClass="dnnFormMessage dnnFormError" ResourceKey="GroupSearch.Required" />
        </asp:Panel>

        <% if (CanCreate) { %>
        <a href="<%=GetCreateUrl() %>" class="dnnPrimaryAction createGroup"><%=LocalizeString("CreateNewGroup")%></a>
        <%} %>


        <dnn:GroupListControl ID="ctlGroupList" runat="server" ItemsPerRow="1">
            <itemtemplate>
                <div class="dgdGroupQuickInfoWrap dnnClear">
                    <div class="dgdAvatar dnnLeft"><a href="[groupviewurl]"><img src="[groupitem:PhotoURL]" alt="[groupitem:GroupName]" /></a></div>
                    <div class="dgdGroupQuickInfo dnnRight">
                        <h3><a href="[groupviewurl]" title="[groupitem:GroupName]">[groupitem:GroupName]</a></h3>
                        <p>[groupitem:GroupDescription]</p>
                        <ul>
                            <li class="posts-icn">[groupitem:stat_status] {resx:posts}</li>
                            <li class="member-icn">[groupitem:UserCount] {resx:members}</li>
                            <li class="photo-icn">[groupitem:stat_photo] {resx:photos}</li>
                            <li class="docs-icn">[groupitem:stat_file] {resx:documents}</li>
                            <li class="join-group-icn"><a href="#" title="" id="groupJoin-[groupitem:RoleId]" class="dnnTertiaryAction" groupid="[groupitem:RoleId]" groupViewTabId ="[GroupViewTabId]">{resx:Join}</a></li>
                        </ul>
                    </div>
                </div>
            </itemtemplate>
        </dnn:GroupListControl>

    </div>
</div>
<script type="text/javascript">

    jQuery(document).ready(function ($) {
        
        $('.join-group-icn a').click(function (event) {
            event.preventDefault();
            var groupId = $(this).attr('groupId');
            var groupViewTabId = $(this).attr('groupViewTabId');
            groupJoin(groupId, groupViewTabId);
        });


        function groupJoin(id, groupViewTabId) {
            var data = {};
            data.roleId = id;
            data.groupViewTabId = groupViewTabId;
            groupPost('JoinGroup', data, groupJoinComplete, id);
        }

        function groupJoinComplete(result, id) {
            if (result.URL != '' && typeof (result.URL) != 'undefined') {
                window.location.href = result.URL;
            } else {
                $('#groupJoin-' + id).text('<%=LocalizeString("Pending")%>')
                    .removeClass('dnnTertiaryAction')
                    .addClass('dnnDisabled')
                    .unbind('click')
                    .click(function (event) { event.preventDefault });
            }
        }

        function groupPost(method, data, callback, groupId) {
            var sf = $.ServicesFramework(<%=ModuleId %>);

            $.ajax({
                type: "POST",
                url: sf.getServiceRoot('SocialGroups') + "ModerationService/" + method,
                beforeSend: sf.setModuleHeaders,
                data: data,
                success: function (data) {
                    if (typeof (callback) != "undefined") {
                        callback(data, groupId);
                    }
                },
                error: function (xhr, status, error) {
                    alert(error);
                }
            });
        }
        
    });

</script>
