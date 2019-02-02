$(document).ready(function() {
    var fader = {
        fadeOut: function() {
            var _this = this;
            $(".control.header").fadeOut(1000, function() { _this.fadeIn.call(_this) })
                .effect("highlight");
        },
        fadeIn: function() {
            var _this = this;
            $(".control.header").fadeIn(1000, function() { _this.fadeOut.call(_this) })
                .effect("highlight");
        }
    };
    fader.fadeOut();
});