<%@ Control language="C#" Inherits="Cantarus.Modules.PolyDeploy.Manage" AutoEventWireup="false"  Codebehind="Manage.ascx.cs" %>

<!-- App Wrapper -->
<div class="container-fluid">
    <div id="cantarus-poly-deploy"
        ng-app="cantarus.poly-deploy.manage"
        data-module-id="<%=ModuleId%>">

        <!-- Content -->
        <div class="row">
            <div class="col-xs-12">
                <div ui-view></div>
            </div>
        </div>
        <!-- /Content -->

    </div>
</div>
<!-- /App Wrapper -->