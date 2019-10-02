'use strict';
define(['knockout', 'main/util', 'main/config'], function (ko, util, config) {
	return {		
		init: function(viewModel, pageSize, load, resx, pagerItemFormatDesc, nopagerItemFormatDesc){
			viewModel.pageSize = pageSize;
			viewModel.pageIndex = ko.observable(0);
		    var cf = config.init();
		    var utility = util.init(cf);
			viewModel.startIndex = ko.computed(function(){ 
				return viewModel.pageIndex() * viewModel.pageSize + 1; 
			});
			
			viewModel.endIndex = ko.computed(function(){ 
				return Math.min((viewModel.pageIndex() + 1) * viewModel.pageSize, viewModel.totalResults());
			});
			
			viewModel.currentPage = ko.computed(function(){ 
				return viewModel.pageIndex() + 1;
			});
			
			viewModel.totalPages = ko.computed(function(){
				if(typeof viewModel.totalResults === 'function' && viewModel.totalResults())
					return Math.ceil(viewModel.totalResults() / viewModel.pageSize);
				return 1;
			});
			
			viewModel.pagerVisible = ko.computed(function(){
				return viewModel.totalPages() > 1;
			});			
			
			viewModel.pagerItemsDescription = ko.computed(function(){
				var pagerFormat = pagerItemFormatDesc || resx.pager_ItemPagedDesc;
				var nopagerFormat = nopagerItemFormatDesc || resx.pager_ItemDesc;				
				if (viewModel.pagerVisible())
				    return pagerFormat.replace('{0}', viewModel.startIndex()).replace('{1}', viewModel.endIndex()).replace('{2}', utility.formatCommaSeparate(viewModel.totalResults()));
				return nopagerFormat.replace('{0}', utility.formatCommaSeparate(viewModel.totalResults()));
			});
			
			viewModel.pagerDescription = ko.computed(function(){
				var pagerFormat = resx.pager_PageDesc;
				if(viewModel.pagerVisible())
				    return pagerFormat.replace('{0}', viewModel.currentPage()).replace('{1}', utility.formatCommaSeparate(viewModel.totalPages()));
				return '';			
			});
			
			viewModel.pagerPrevClass= ko.computed(function(){
				return 'prev' + (viewModel.pageIndex() < 1 ? ' disabled' : '');
			});
			
			viewModel.pagerNextClass = ko.computed(function(){
				return 'next' + (viewModel.pageIndex() >= viewModel.totalPages() - 1 ? ' disabled' : '');
			});
			
			viewModel.prev = function(){	
				if(viewModel.pageIndex() <= 0) return;
				viewModel.pageIndex(viewModel.pageIndex() - 1);
				if(typeof load === 'function') load.apply(viewModel);
			};
			
			viewModel.next = function(){
				if(viewModel.pageIndex() >= viewModel.totalPages() -1) return;
				viewModel.pageIndex(viewModel.pageIndex() + 1);
				if(typeof load === 'function') load.apply(viewModel);
			};	
		}
	};	
});