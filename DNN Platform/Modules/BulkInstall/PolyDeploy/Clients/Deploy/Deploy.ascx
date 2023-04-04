<%@ Control language="C#" Inherits="DotNetNuke.BulkInstall.Deploy" AutoEventWireup="false"  Codebehind="Deploy.ascx.cs" %>

<!-- App Wrapper -->
<div class="container-fluid">
    <div id="cantarus-poly-deploy"
        ng-app="cantarus.poly-deploy.deploy"
        data-module-id="<%=ModuleId%>">

        <!-- Content -->
        <div ui-view></div>
        <!-- /Content -->

    </div>
</div>
<!-- /App Wrapper -->