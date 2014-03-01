<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Controls.UserAndLogin" CodeFile="UserAndLogin.ascx.cs" ViewStateMode="Disabled" %>
<div class="userProperties">
    <ul>
        <%  if (!IsAuthenticated)
            {
        %> 
                <li class="userRegister"><asp:HyperLink ID="registerLink" runat="server"><% =LocalizeString("Register") %></asp:HyperLink>
                <li class="userLogin"><asp:HyperLink ID="loginLink" runat="server"><% =LocalizeString("Login") %></asp:HyperLink>
        <%  }
            else
            {
        %>
                <li class="userName"><a id="dnn_dnnUser_userNameLink" href="#"><%=DisplayName %></a>
                    <ul class="userMenu">
                        <li class="viewProfile"><asp:HyperLink ID="viewProfileLink" runat="server"><%=LocalizeString("Profile")%></asp:HyperLink></li>
                        <li class="userMessages"><asp:HyperLink ID="messagesLink" runat="server"><asp:Label ID="messageCount" runat="server" Visible="false" /> <%=LocalizeString("Messages") %></asp:HyperLink></li>
                        <li class="userNotifications"><asp:HyperLink ID="notificationsLink" runat="server"><asp:Label ID="notificationCount" runat="server" Visible="False"/> <%=LocalizeString("Notifications") %></asp:HyperLink></li>
                        <li class="userSettings"><asp:HyperLink ID="accountLink" runat="server"><%=LocalizeString("Account") %></asp:HyperLink></li>
                        <li class="userProfilename"><asp:HyperLink ID="editProfileLink" runat="server"><%=LocalizeString("EditProfile") %></asp:HyperLink></li>
                        <li class="userLogout"><asp:HyperLink ID="logoffLink" runat="server"><strong><%=LocalizeString("Logout") %></strong></asp:HyperLink></li>
                    </ul><!--close userMenu-->
                </li>
                <li class="userProfile">
                    <asp:HyperLink ID="viewProfileImageLink" runat="server"><span class="userProfileImg"><asp:Image ID="profilePicture" runat="server"/></span></asp:HyperLink>
                    <asp:Label ID="messages" runat="server" Visible="false" CssClass="userMessages" />
                </li>
        <%       
            }
        %>
    </ul>
</div>
