<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%-- CSS files --%>
<dnn:DnnCssInclude ID="BootstrapCSS" runat="server" FilePath="bootstrap/css/bootstrap.min.css" PathNameAlias="SkinPath" Priority="12" />
<dnn:DnnCssInclude ID="SmartMenuBootstrapCSS" runat="server" FilePath="css/jquery.smartmenus.bootstrap.css" PathNameAlias="SkinPath" Priority="13" />
<dnn:DnnCssInclude ID="MainMenuCSS" runat="server" FilePath="Menus/MainMenu/MainMenu.css" PathNameAlias="SkinPath" Priority="14" />
<dnn:DnnCssInclude ID="SkinCSS" runat="server" FilePath="skin.css" PathNameAlias="SkinPath" />

<%-- JS files --%>
<dnn:DnnJsInclude ID="BootstrapJS" runat="server" FilePath="bootstrap/js/bootstrap.min.js" PathNameAlias="SkinPath" />
<dnn:DnnJsInclude ID="SmartMenusJquery" runat="server" FilePath="js/jquery.smartmenus.js" PathNameAlias="SkinPath" />
<dnn:DnnJsInclude ID="SmartMenusJqueryBootstrap" runat="server" FilePath="js/jquery.smartmenus.bootstrap.js" PathNameAlias="SkinPath" />
<dnn:DnnJsInclude ID="scriptJS" runat="server" FilePath="js/scripts.js" PathNameAlias="SkinPath" />
