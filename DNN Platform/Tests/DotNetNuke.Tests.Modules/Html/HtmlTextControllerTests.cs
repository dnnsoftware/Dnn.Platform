// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Modules.Html
{
    using System.Collections.Generic;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Modules.Html;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class HtmlTextControllerTests
    {
        [SetUp]
        public void SetUp()
        {
            ComponentFactory.RegisterComponentInstance<CachingProvider>(new FakeCachingProvider(new Dictionary<string, object>()));
        }

        [TearDown]
        public void TearDown()
        {
            TabController.ClearInstance();
            TabVersionSettings.ClearInstance();
            TabWorkflowSettings.ClearInstance();
            SystemWorkflowManager.ClearInstance();
            WorkflowManager.ClearInstance();
        }

        [Test]
        public void ManageRelativePaths_DoesNotChangePlainHtml()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "<p>Hello</p>",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("<p>Hello</p>"));
        }

        [Test]
        public void ManageRelativePaths_AdjustsRelativeImgSrc()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "<img src=\"image.jpg\"/>",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("<img src=\"/portals/0/image.jpg\"/>"));
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustImgSrcWithCorrectPathCaseInsensitive()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "<img src=\"/Portals/0/image.jpg\"/>",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("<img src=\"/portals/0/image.jpg\"/>"));
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustImgSrcWithAbsoluteUrl()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "<img src=\"https://example.com/image.jpg\"/>",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("<img src=\"https://example.com/image.jpg\"/>"));
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustImgSrcWithAbsoluteUrlInContent()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "src=\"https://example.com/image.jpg\" is how you indicate a URL",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("src=\"https://example.com/image.jpg\" is how you indicate a URL"));
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustContentEndingInImgSrc()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "src=\"image.jpg\"",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("src=\"image.jpg\""));
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustContentEndingInUnclosedImgSrc()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "src=\"image.jpg",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("src=\"image.jpg"));
        }

        [Test]
        public void ManageRelativePaths_DoesAdjustImgSrcWithRelativeUrlInContent()
        {
            // TODO: should we attempt to avoid making this change?
            var actual = HtmlTextController.ManageRelativePaths(
                "src=\"image.jpg\" is how you indicate a URL",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("src=\"/portals/0/image.jpg\" is how you indicate a URL"));
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustImgSrcWithDataUrl()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "<img src=\"data:image/gif;base64,R0lGODlhEAAQAMQAAORHHOVSKudfOulrSOp3WOyDZu6QdvCchPGolfO0o/XBs/fNwfjZ0frl3/zy7////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAkAABAALAAAAAAQABAAAAVVICSOZGlCQAosJ6mu7fiyZeKqNKToQGDsM8hBADgUXoGAiqhSvp5QAnQKGIgUhwFUYLCVDFCrKUE1lBavAViFIDlTImbKC5Gm2hB0SlBCBMQiB0UjIQA7\"/>",
                "/portals/0/",
                "src");
            Assert.That(actual, Is.EqualTo("<img src=\"data:image/gif;base64,R0lGODlhEAAQAMQAAORHHOVSKudfOulrSOp3WOyDZu6QdvCchPGolfO0o/XBs/fNwfjZ0frl3/zy7////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAkAABAALAAAAAAQABAAAAVVICSOZGlCQAosJ6mu7fiyZeKqNKToQGDsM8hBADgUXoGAiqhSvp5QAnQKGIgUhwFUYLCVDFCrKUE1lBavAViFIDlTImbKC5Gm2hB0SlBCBMQiB0UjIQA7\"/>"));
        }

        [Test]
        public void ManageRelativePaths_AdjustsNonRootedRelativePathsAndDoesNotAdjustOtherPaths()
        {
            const string HtmlContent = @"
<img alt=""a data URI"" src=""data:image/gif;base64,R0lGODlhEAAQAMQAAORHHOVSKudfOulrSOp3WOyDZu6QdvCchPGolfO0o/XBs/fNwfjZ0frl3/zy7////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAkAABAALAAAAAAQABAAAAVVICSOZGlCQAosJ6mu7fiyZeKqNKToQGDsM8hBADgUXoGAiqhSvp5QAnQKGIgUhwFUYLCVDFCrKUE1lBavAViFIDlTImbKC5Gm2hB0SlBCBMQiB0UjIQA7""/>
<img alt=""a non-rooted relative path"" src=""images/image.png"" />
<img alt=""a rooted relative path"" src=""/image.png"" />
<img alt=""an absolute path"" src=""https://dnncommunity.org/Portals/0/DNN_White_Logo_lg.png"" />
<img alt=""another data URI"" src=""data:image/gif;base64,R0lGODlhDgAOAOYAAOBxJvGKIfOEIu5+ItlaIt1XD/zu5uNzNvSJItxXD99cFumEIe2/lOGfb/nNoN9/Huy+lNxgIuqHHeOAH/PQsNZ+SfvRpN+NNficPu2DLt1WDfueOvCfV+WldviSLfHJoeGYR9+AH+l3It1hHvbfxu/MqeSLT+OcUedxIuFwIfaQMeate+qTNeWkfuGib/fcwvufPvjn1uuCKNxbFe2QTOOLNvKHGfmdOt95F9uGRuSjdeiHH+KMQ999H/ubOei0jO6LKuFoIvjZuuFuMeSZSt58P+CFNvCZSPfDivjIm9NgIN5rJfOLLttaH/KNIPKvavSMIfeMJdZUDu+MMvvz6uV4KeR2J+qUXtlTDPLXv+l9KvSMG+y7jPrTqPaWNu2EHdlWD/CELPufPOOJWvfl0tlkI91lIvuaMuadcOONSuqTSeOQU95XD+aIM+SBGu2sivCFHfucOdp5F+etc91cF92CRvHLotxyKPzu5+SLQviSL/rQpN9dF+NvMdxNAP///yH5BAAAAAAALAAAAAAOAA4AAAe7gHgHIzN0fIeHCgp9BgdmSi1oJlZaYUwqeh59CWUVf59/FEdePhtnfhpLdTodP1mfDjdicX4FAHdreW01czF/XTA+tQA5ny9PLCBkfxhRtVU8ECWfSDsnf0kCfmwZU0ALRCR/HCEMfwN+CTJfNlsSF39CPSt/NH5gOB9/Fk4P00Yu/lzxI0UOlU9QJnD5k6bBnzd+sLix82cPggVqOKQoMoaAnyFwAgRAIGCACBRBIhBowqiPn5cwYzIKBAA7"">
<img alt=""another non-rooted relative path"" src=""images2/image2.png"" />
<img alt=""another rooted relative path"" src=""/images/image2.png"" />
<img alt=""another absolute path"" src=""https://dnncommunity.org/DesktopModules/ActiveForums/images/feedicon.gif"" />
";
            var actual = HtmlTextController.ManageRelativePaths(
                HtmlContent,
                "/portals/0/",
                "src");

            const string Expected = @"
<img alt=""a data URI"" src=""data:image/gif;base64,R0lGODlhEAAQAMQAAORHHOVSKudfOulrSOp3WOyDZu6QdvCchPGolfO0o/XBs/fNwfjZ0frl3/zy7////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAkAABAALAAAAAAQABAAAAVVICSOZGlCQAosJ6mu7fiyZeKqNKToQGDsM8hBADgUXoGAiqhSvp5QAnQKGIgUhwFUYLCVDFCrKUE1lBavAViFIDlTImbKC5Gm2hB0SlBCBMQiB0UjIQA7""/>
<img alt=""a non-rooted relative path"" src=""/portals/0/images/image.png"" />
<img alt=""a rooted relative path"" src=""/image.png"" />
<img alt=""an absolute path"" src=""https://dnncommunity.org/Portals/0/DNN_White_Logo_lg.png"" />
<img alt=""another data URI"" src=""data:image/gif;base64,R0lGODlhDgAOAOYAAOBxJvGKIfOEIu5+ItlaIt1XD/zu5uNzNvSJItxXD99cFumEIe2/lOGfb/nNoN9/Huy+lNxgIuqHHeOAH/PQsNZ+SfvRpN+NNficPu2DLt1WDfueOvCfV+WldviSLfHJoeGYR9+AH+l3It1hHvbfxu/MqeSLT+OcUedxIuFwIfaQMeate+qTNeWkfuGib/fcwvufPvjn1uuCKNxbFe2QTOOLNvKHGfmdOt95F9uGRuSjdeiHH+KMQ999H/ubOei0jO6LKuFoIvjZuuFuMeSZSt58P+CFNvCZSPfDivjIm9NgIN5rJfOLLttaH/KNIPKvavSMIfeMJdZUDu+MMvvz6uV4KeR2J+qUXtlTDPLXv+l9KvSMG+y7jPrTqPaWNu2EHdlWD/CELPufPOOJWvfl0tlkI91lIvuaMuadcOONSuqTSeOQU95XD+aIM+SBGu2sivCFHfucOdp5F+etc91cF92CRvHLotxyKPzu5+SLQviSL/rQpN9dF+NvMdxNAP///yH5BAAAAAAALAAAAAAOAA4AAAe7gHgHIzN0fIeHCgp9BgdmSi1oJlZaYUwqeh59CWUVf59/FEdePhtnfhpLdTodP1mfDjdicX4FAHdreW01czF/XTA+tQA5ny9PLCBkfxhRtVU8ECWfSDsnf0kCfmwZU0ALRCR/HCEMfwN+CTJfNlsSF39CPSt/NH5gOB9/Fk4P00Yu/lzxI0UOlU9QJnD5k6bBnzd+sLix82cPggVqOKQoMoaAnyFwAgRAIGCACBRBIhBowqiPn5cwYzIKBAA7"">
<img alt=""another non-rooted relative path"" src=""/portals/0/images2/image2.png"" />
<img alt=""another rooted relative path"" src=""/images/image2.png"" />
<img alt=""another absolute path"" src=""https://dnncommunity.org/DesktopModules/ActiveForums/images/feedicon.gif"" />
";
            Assert.That(actual, Is.EqualTo(Expected));
        }

        [Test]
        public void GetWorkflow_UsesDirectPublish_WhenTabHasStateButWorkflowIsDisabled()
        {
            const int PortalId = 1;
            const int TabId = 2;
            const int DirectPublishWorkflowId = 3;
            var tab = new TabInfo { PortalID = PortalId, TabID = TabId, StateID = 6 };
            var workflow = new Workflow { WorkflowID = DirectPublishWorkflowId, WorkflowName = "Direct Publish" };

            var tabController = new Mock<ITabController>();
            tabController.Setup(c => c.GetTab(TabId, PortalId)).Returns(tab);
            TabController.SetTestableInstance(tabController.Object);

            var tabVersionSettings = new Mock<ITabVersionSettings>();
            tabVersionSettings.Setup(s => s.IsVersioningEnabled(PortalId, TabId)).Returns(false);
            TabVersionSettings.SetTestableInstance(tabVersionSettings.Object);

            var systemWorkflowManager = new Mock<ISystemWorkflowManager>();
            systemWorkflowManager.Setup(m => m.GetDirectPublishWorkflow(PortalId)).Returns(workflow);
            SystemWorkflowManager.SetTestableInstance(systemWorkflowManager.Object);

            var workflowManager = new Mock<IWorkflowManager>();
            workflowManager.Setup(m => m.GetWorkflow(DirectPublishWorkflowId)).Returns(workflow);
            WorkflowManager.SetTestableInstance(workflowManager.Object);

            var result = CreateHtmlTextController().GetWorkflow(1, TabId, PortalId);

            Assert.That(result.Value, Is.EqualTo(DirectPublishWorkflowId));
        }

        private static HtmlTextController CreateHtmlTextController()
        {
            var hostSettings = Mock.Of<IHostSettings>();
            var portalController = Mock.Of<IPortalController>();
            return new HtmlTextController(
                Mock.Of<INavigationManager>(),
                Mock.Of<IPortalAliasService>(),
                portalController,
                Mock.Of<IApplicationStatusInfo>(),
                hostSettings,
                new HtmlModuleSettingsRepository(
                    Mock.Of<IModuleController>(),
                    hostSettings,
                    Mock.Of<IHostSettingsService>(),
                    portalController));
        }
    }
}
