<%@ Control language="C#" Inherits="Cantarus.Modules.PolyDeploy.View" AutoEventWireup="false"  Codebehind="View.ascx.cs" %>

<!-- App Wrapper -->
<div class="container-fluid">
    <div id="cantarus-poly-deploy" ng-app="cantarus.poly-deploy">

        <!-- Main Controller -->
        <div ng-controller="MainController">

            <!-- Navigation -->
            <div class="row">
                <div class="col-xs-12">
                    <nav>
                        App Navigation
                    </nav>
                </div>
            </div>
            <!-- /Navigation -->
        
            <!-- Content -->
            <div class="row">
                <div class="col-xs-12">
                    <div ui-view></div>
                </div>
            </div>
            <!-- /Content -->

        </div>
        <!-- /MainController -->

    </div>
</div>
<!-- /App Wrapper -->