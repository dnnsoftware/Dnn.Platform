define(["jquery"], function ($) {
  "use strict";
  var menuItem, util;

  var init = function (menu, utility, params, callback) {
    menuItem = menu;
    util = utility;

    if (typeof callback === "function") {
      callback();
    }

    $('#menu-QuickAddModule button').click(function () {
      util.sf.moduleRoot = "internalservices";
      util.sf.controller = "controlBar";
      util.sf.post(
        "AddModule",
        {
          Visibility: 0,
          Position: "BOTTOM",
          Module: $("#menu-QuickAddModule-module").val(),
          Pane: $("#menu-QuickAddModule-pane").val(),
          AddExistingModule: false,
          CopyModule: false,
        },
        function done() {
          window.top.location.reload();
        }
      );
    });
  };

  return {
    init: init,
  };
});
