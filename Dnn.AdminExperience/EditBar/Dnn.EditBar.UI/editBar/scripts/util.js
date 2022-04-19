'use strict';
define(['jquery'], function ($) {
    return {
        init: function (config) {
            return {
                loadResx: function(cb) {
                    var self = this;
                    
					self.sf.moduleRoot = 'editBar/common';
                    self.sf.controller = 'localization';
                    self.sf.getsilence('gettable', { culture: config.culture }, function (d) {
                        self.resx = d;
                        if (typeof cb === 'function') cb();
                    });
                },

                getResx: function (moduleName, key) {
                    if (this.resx[moduleName] && this.resx[moduleName].hasOwnProperty(key)) {
                        return this.resx[moduleName][key];
                    }

                    return key;
                },

                getModuleNameByParams: function(params) {
                    return params ? (params.moduleName || '') : '';
                },

                getIdentifierByParams: function (params) {
                    return params ? (params.identifier || '') : '';
                },

                asyncParallel: function(deferreds, callback) {
                    var i = deferreds.length;
                    if (i === 0) callback();
                    var call = function() {
                        i--;
                        if (i === 0) {
                            callback();
                        }
                    };

                    $.each(deferreds, function(ii, d) {
                        d(call);
                    });
                },

                asyncWaterfall: function(deferreds, callback) {
                    var call = function() {
                        var deferred = deferreds.shift();
                        if (!deferred) {
                            callback();
                            return;
                        }
                        deferred(call);
                    };
                    call();
                },

                confirm: function(text, confirmBtn, cancelBtn, confirmHandler, cancelHandler) {
                    $('#confirmation-dialog > p').html(text);
                    $('#confirmation-dialog a#confirmbtn').html(confirmBtn).unbind('click').bind('click', function() {
                        if (typeof confirmHandler === 'function') confirmHandler.apply();
                        $('#confirmation-dialog').fadeOut(200, 'linear', function() { $('#mask').hide(); });
                    });
                    $('#confirmation-dialog a#cancelbtn').html(cancelBtn).unbind('click').bind('click', function() {
                        if (typeof cancelHandler === 'function') cancelHandler.apply();
                        $('#confirmation-dialog').fadeOut(200, 'linear', function() { $('#mask').hide(); });
                    });
                    $('#mask').show();
                    $('#confirmation-dialog').fadeIn(200, 'linear');

                    $(window).off('keydown.confirmDialog').on('keydown.confirmDialog', function(evt) {

                        if (evt.keyCode === 27) {
                            $(window).off('keydown.confirmDialog');
                            $('#confirmation-dialog a#cancelbtn').trigger('click');
                        }
                    });
                },

                notify: function(text) {
                    $('#notification-dialog > p').removeClass().html(text);
                    $('#notification-dialog').fadeIn(200, 'linear', function() {
                        setTimeout(function() {
                            $('#notification-dialog').fadeOut(200, 'linear');
                        }, 2000);
                    });
                },

                notifyError: function(text) {
                    $('#notification-dialog > p').removeClass().addClass('errorMessage').html(text);
                    $('#notification-dialog').fadeIn(200, 'linear', function () {
                        setTimeout(function() {
                            $('#notification-dialog').fadeOut(200, 'linear');
                        }, 2000);
                    });
                },

                trimContentToFit: function(content, width) {
                    if (!content || !width) return '';
                    var charWidth = 8.5;
                    var max = Math.floor(width / charWidth);

                    var arr = content.split(' ');
                    var trimmed = '', count = 0;
                    $.each(arr, function(i, v) {
                        count += v.length;
                        if (count < max) {
                            if (trimmed) trimmed += ' ';
                            trimmed += v;
                            count++;
                        } else {
                            trimmed += '...';
                            return false;
                        }
                    });
                    return trimmed;
                },

                getApplicationRootPath: function getApplicationRootPath() {
                    var rootPath = location.protocol + '//' + location.host + (location.port ? (':' + location.port) : '');
                    if (rootPath.substr(rootPath.length - 1, 1) === '/') {
                        rootPath = rootPath.substr(0, rootPath.length - 1);
                    }
                    return rootPath;
                }
            };
        }
    };
});

define('css',{
    load: function (name, require, load, config) {
		function inject(filename)
		{
			var head = document.getElementsByTagName('head')[0];
			var link = document.createElement('link');
			link.href = filename;
			link.rel = 'stylesheet';
			link.type = 'text/css';
			head.appendChild(link);
		}

		var path = name;
        for (var i in config.paths) {
            if (path.indexOf(i) === 0) {
                path = path.replace(i, config.paths[i]);
                break;
            }
        }

        if (path.indexOf('://') === -1) {
            path = config.baseUrl + path;
        }

        if (typeof config.urlArgs === 'string') {
            path = path + '?' + config.urlArgs;
        }
        inject(path);

		load(true);
	},
	pluginBuilder: './css-build'
});;
