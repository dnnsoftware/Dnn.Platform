function doSomething() {
    
    if (!$ && !$.isReady) {
        $(document).ready(function() {
            writeStuff();
        });
    }
    else {
        writeStuff();
    }
}
function writeStuff() {
    var letters = "--------LAZY LOADED!----------";

    var counter = 0;
    $(".footer").html($(".footer").html() + "<br/>");
    var timer = setInterval(function() {
        $(".lazyLoaded").html($(".lazyLoaded").html() + letters[counter]);
        counter++;
        if (counter == letters.length) {
            clearInterval(timer);
        }
    }, 50);

}
doSomething();