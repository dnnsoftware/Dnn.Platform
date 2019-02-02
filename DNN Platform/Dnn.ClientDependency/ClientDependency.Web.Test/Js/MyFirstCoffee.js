(function() {
  var cube, square;

  square = function(x) {
    return x * x;
  };

  cube = function(x) {
    return square(x) * x;
  };

  alert(square(2 * cube(2)));

}).call(this);
