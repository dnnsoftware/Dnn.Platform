var pager = {
        init: function (viewModel, pageSize, load, resx, pagerItemFormatDesc, nopagerItemFormatDesc) {
            viewModel.pageSize = pageSize;
            viewModel.pageIndex = ko.observable(0);
            
            viewModel.startIndex = ko.computed(function () {
                return viewModel.pageIndex() * viewModel.pageSize + 1;
            });

            viewModel.endIndex = ko.computed(function () {
                return Math.min((viewModel.pageIndex() + 1) * viewModel.pageSize, viewModel.totalResults());
            });

            viewModel.currentPage = ko.computed(function () {
                return viewModel.pageIndex() + 1;
            });

            viewModel.totalPages = ko.computed(function () {
                if (typeof viewModel.totalResults === 'function' && viewModel.totalResults())
                    return Math.ceil(viewModel.totalResults() / viewModel.pageSize);
                return 1;
            });

            viewModel.pagerVisible = ko.computed(function () {
                return viewModel.totalPages() > 1;
            });

            viewModel.pagerItemsDescription = ko.computed(function () {
                return "Showing " + viewModel.startIndex() + " - " + viewModel.endIndex() + " of " + viewModel.totalResults() + " contacts";
            });

            viewModel.pagerDescription = ko.computed(function () {
                return "Page: " + viewModel.currentPage() + " of " + viewModel.totalPages();
            });

            viewModel.pagerPrevClass = ko.computed(function () {
                return 'prev' + (viewModel.pageIndex() < 1 ? ' disabled' : '');
            });

            viewModel.pagerNextClass = ko.computed(function () {
                return 'next' + (viewModel.pageIndex() >= viewModel.totalPages() - 1 ? ' disabled' : '');
            });

            viewModel.prev = function () {
                if (viewModel.pageIndex() <= 0) return;
                viewModel.pageIndex(viewModel.pageIndex() - 1);
                if (typeof load === 'function') load.apply(viewModel);
            };

            viewModel.next = function () {
                if (viewModel.pageIndex() >= viewModel.totalPages() - 1) return;
                viewModel.pageIndex(viewModel.pageIndex() + 1);
                if (typeof load === 'function') load.apply(viewModel);
            };
        }
    };