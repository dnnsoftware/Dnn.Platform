<%@ Control language="C#" Inherits="Cantarus.Modules.PolyDeploy.View" AutoEventWireup="false"  Codebehind="View.ascx.cs" %>

<!-- App Wrapper -->
<div ng-app="cantarus.poly-deploy">

    <!-- Main Controller -->
    <div ng-controller="MainController">

        <!-- Navigation -->
        <nav>
            App Navigation
        </nav>
        <!-- /Navigation -->
        
        <div ui-view></div>

    </div>
    <!-- /MainController -->

</div>
<!-- /App Wrapper -->