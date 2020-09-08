<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="Dnn.Modules.ResourceManager.View" %>
<%
    DotNetNuke.Web.Client.ClientResourceManagement.ClientResourceManager.RegisterScript(this.Page, "~/js/dnn.servicesframework.js");
%>

<asp:Panel runat="server" ID="ResourceManagerContainer" CssClass="rm-container" data-init-callback="initResourceLibrary">
</asp:Panel>

<script>
  $(function () {
    function resxCallback200(data) {
      var resx = data.localization;
      var options = {
        containerId: '<%=ResourceManagerContainer.ClientID%>',
        moduleId: <%=ModuleId%>,
        portalId: <%=PortalId%>,
        groupId: <%=GroupId%>,
        moduleName: 'ResourceManager',
        tabId: <%=TabId%>,
        resx: resx,
        homeFolderId: <%=HomeFolderId%>,
        numItems: <%=FolderPanelNumItems%>,
        itemWidth: <%=ItemWidth%>,
        maxUploadSize: <%=MaxUploadSize%>,
        maxFileUploadSizeHumanReadable: '<%=MaxUploadSizeHumanReadable%>',
        sortingOptions: JSON.parse('<%=SortingFields%>'),
        sorting: '<%=DefaultSortingField%>',
        userLogged: '<%=UserLogged%>' === 'True',
        extensionWhitelist: '<%= ExtensionWhitelist %>',
        validationCode: '<%= ValidationCode %>',
        isAdmin: '<%=IsAdmin%>' === 'True',
      }

      var openFolderId = <%=OpenFolderId%>;
      if (openFolderId) {
        options.openFolderId = openFolderId;
        options.openFolderPath = JSON.parse('<%=OpenFolderPath%>');
      }

      window.dnn.ResourceManager.instance.init(options);
    };

    var localizationOptions = {
      service: 'ResourceManager',
      controller: 'Localization',
      resxName: 'ResourceManagerResourcesResx',
      resourceSettings: {
        local: {
          culture: '<%=ResxCulture%>',
            resxTimeStamp: '<%=ResxTimeStamp%>'
        }
      },
      resources: {
        method: 'GetResources',
        paramName: null,
        callback200: resxCallback200,
        callbackError: null
      }
    };

    dnn.utils.Localization(localizationOptions);
  });
</script>
