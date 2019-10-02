'use strict';
define(['jquery', 'main/loader'], function ($, loader) {

	return {
		init: function (siteRoot, tabId, antiForgeryToken) {
			return {
				moduleRoot: 'personaBar',
				controller: '',
				antiForgeryToken: antiForgeryToken,
				setHeaders: function (xhr) {
					if (tabId) {
						xhr.setRequestHeader('TabId', tabId);
					}

					if (antiForgeryToken) {
						xhr.setRequestHeader('RequestVerificationToken', antiForgeryToken);
					}
				},
				getServiceRoot: function () {
					var self = this;
					return siteRoot + 'API/' + self.moduleRoot + '/';
				},
				getSiteRoot: function () {
					return siteRoot;
				},
				rawCall: function (httpMethod, url, params, success, failure, loading, beforeSend, sync, silence, postFile) {
					var beforeCallback;
					var self = this;
					if (typeof beforeSend === 'function') {
						beforeCallback = function (xhr) {
							self.setHeaders(xhr);
							return beforeSend(xhr);
						};
					}
					else {
						beforeCallback = self.setHeaders;
					}

					var options = {
						url: url,
						beforeSend: beforeCallback,
						type: httpMethod,
						async: sync === false,
						success: function (d) {
							if (!silence) loader.stopLoading();
							if (typeof loading === 'function') {
								loading(false);
							}

							if (typeof success === 'function') {
								success(d || {});
							}
						},
						error: function (xhr, status, err) {
							if (!silence) loader.stopLoading(!(httpMethod !== "GET")); //only show error if GET
							if (typeof loading === 'function') {
								loading(false);
							}

							if (typeof failure === 'function') {
								if (xhr) {
									failure(xhr, err);
								}
								else {
									failure(null, 'Unknown error');
								}
							}
						}
					};

					if (httpMethod === 'GET') {
						options.data = params;
					} else if (postFile) {
						options.processData = false;
						options.contentType = false;
						options.data = params;
					}
					else {
						options.contentType = 'application/json; charset=UTF-8';
						options.data = JSON.stringify(params);
						options.dataType = 'json';
					}

					if (typeof loading === 'function') {
						loading(true);
					}

					if (!silence) loader.startLoading();
					return $.ajax(options);
				},

				call: function (httpMethod, method, params, success, failure, loading, beforeSend, sync, silence, postFile) {
					var self = this;
					var url = self.getServiceRoot() + self.controller + '/' + method;

					// Reset url default values
					self.moduleRoot = 'personaBar';
					self.controller = '';

					return this.rawCall(httpMethod, url, params, success, failure, loading, beforeSend, sync, silence, postFile);
				},

				post: function (method, params, success, failure, loading, beforeSend) {
					return this.call('POST', method, params, success, failure, loading, beforeSend, false, false);
				},

				postfile: function (method, params, success, failure, loading, beforeSend) {
					return this.call('POST', method, params, success, failure, loading, beforeSend, false, false, true);
				},

				postsilence: function (method, params, success, failure, loading, beforeSend) {
					return this.call('POST', method, params, success, failure, loading, beforeSend, false, true);
				},

				get: function (method, params, success, failure, loading, beforeSend) {
					return this.call('GET', method, params, success, failure, loading, beforeSend, false, false);
				},

				getsilence: function (method, params, success, failure, loading, beforeSend) {
					return this.call('GET', method, params, success, failure, loading, beforeSend, false, true);
				}
			};
		}
	};
});