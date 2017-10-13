'use strict';
define(['jquery'], function ($) {
	return {		
		init: function(viewModel, callback){
			viewModel.panel = {
				activate: function(vm, e){
					var li = $(e.target);
					if(li.hasClass('selected')) return;
					var ul = li.parent();
					$('li', ul).removeClass('selected');
					li.addClass('selected');
					var panel = li.data('panel-id');
					ul.parent().find('.tabPanel').hide();
					ul.parent().find('#' + panel).fadeIn(400, 'linear');	
					if(typeof callback === 'function') callback(panel);					
				}
			};
		}
	};	
});