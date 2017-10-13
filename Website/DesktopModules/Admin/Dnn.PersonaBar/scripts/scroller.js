'use strict';
define(['jquery'], function ($) {
	return {		
	    init: function (holder, cards, left, right, initialOffset, scrollOffset, visibleCount) {
	        var idx = 0;
	        holder.css({ left: initialOffset });
	        var count = cards.length;
	        if (visibleCount >= count) {
	            right.unbind('click').css('visibility', 'hidden');
	            left.unbind('click').css('visibility', 'hidden');
	            return;
	        }

	        right.unbind('click').bind('click', function (e) {
	            e.preventDefault();
	            if (idx < count - visibleCount) {
	                idx++;
	                holder.animate({ left: '-=' + scrollOffset }, 300);
	            }
	        }).css('visibility', 'visible');

	        left.unbind('click').bind('click', function (e) {
	            e.preventDefault();
	            if (idx > 0) {
	                idx--;
	                holder.animate({ left: '+=' + scrollOffset }, 300);
	            }
	        }).css('visibility', 'visible');
		}
	};	
});