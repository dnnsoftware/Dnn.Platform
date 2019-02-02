(function(txt) {
    if (!$.isReady) {
        $(document).ready(function() {
            writeFooterStuff();
        });
    }
    else {
        writeFooterStuff();
    }
})();
function writeFooterStuff() {
    var letters = "Hello footer";

    var counter = 0;
    $(".footer").html($(".footer").html() + "<br/>");
    var timer = setInterval(function() {
        $(".footer").html($(".footer").html() + letters[counter]);
        counter++;
        if (counter == letters.length) {
            clearInterval(timer);
        }
    }, 50);
    
    for (var c in letters) {
        
    }
}