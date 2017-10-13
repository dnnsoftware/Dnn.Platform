'use strict';
define(['jquery', 'knockout', 'jquery.hoverIntent.min'], function ($, ko) {
    var mainFrame = window.parent.document.getElementById("personaBar-iframe");
    var personaBarWidth = 80;
    var util = null;

    var getSummaryContainer = function () {
        var $summary = $('.server-summary');
        if (!$summary.length) {
            $summary = $('' +
                '<div class="server-summary hoverSummaryMenu">' +
                    '<ul>' +
                        '<li class="title" data-bind="html: ProductName"></li>' +
                        '<li class="version-info" data-bind="html: ProductVersion"></li>' +
                        '<li class="framework" data-bind="visible: FrameworkVersion.length > 0"><span data-bind="html: FrameworkVersion"></span><label data-bind="html: resx.Framework"></label></li>' +
                        '<li class="server-name" data-bind="visible: ServerName.length > 0"><span data-bind="html: ServerName"></span><label data-bind="html: resx.ServerName"></label></li>' +
                        '<li class="license-info" data-bind="visible: visibleCheck(\'LicenseVisible\')"><a href="http://www.dnnsoftware.com/products/" data-bind="html: resx.UpgradeLicense" target="_blank"></a><label data-bind="html: resx.LicenseKey"></label></li>' +
                        '<li class="separator"></li>' +
                        '<li class="doc-center"><a href="http://www.dnnsoftware.com/docs/" data-bind="html: resx.Documentation, visible: visibleCheck(\'DocCenterVisible\')" target="_blank"></a></li>' +
                        '<li id="Logout" class="logout" data-bind="html: resx.nav_Logout"></li>' +
                    '</ul>' +
                '</div>');

            $('#personabar').find('.personabarLogo').append($summary);
        }

        return $summary;
    }
    var showServerSummary = function () {
        mainFrame.style.width = '100%';
        $('.hoverSummaryMenu, .hovermenu').hide();
        getSummaryContainer().addClass('shown');
    }

    var hideServerSummary = function () {
        if (!$('.hovermenu:visible').length && !$('.socialpanel:visible').length) {
            mainFrame.style.width = personaBarWidth + "px";
        }
        getSummaryContainer().removeClass('shown');
    }

    var getServerInfo = function(callback) {
        util.sf.moduleRoot = "personaBar";
        util.sf.controller = "ServerSummary";
        util.sf.getsilence("GetServerInfo", {}, function (data) {
            if (typeof callback === "function") {
                callback(data);
            }
        });
    }

    var initialize = function () {
        if (!util.resx) {
            setTimeout(initialize, 500);
            return;
        }
        var $logo = $('#personabar').find('.personabarLogo');
        var $summaryContainer = getSummaryContainer();

        getServerInfo(function (info) {
            var viewModel = $.extend({}, info, { resx: util.resx.PersonaBar, visibleCheck: function(name) {
                return info[name];
            } });
            try {
                ko.applyBindings(viewModel, $summaryContainer[0]);
            } catch (ex) {
                
            }
        });
        
        $logo.hoverIntent({
            over: function() {
                showServerSummary();
            },
            out: function () {
                hideServerSummary();
            },
            timeout: 200
        });
    }

    return {
        init: function (utility) {
            util = utility;

            initialize();
        },
        load: function () {
        },
        leave: function() {
        }
    }
});