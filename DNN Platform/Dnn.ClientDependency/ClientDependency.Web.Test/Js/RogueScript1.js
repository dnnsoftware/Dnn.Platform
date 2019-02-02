setTimeout(function() {
    writeStuff("I'm a rogue script and my rogue friend has made the header blink");
}
, 2000);

function writeStuff(str) {
    var letters = str;
    var counter = 0;
    $(".myControl div").html("");
    $(".myControl div").css("color", "blue").css("font-size", "20px").css("padding", "10px");
    var timer = setInterval(function() {
        $(".myControl div").html($(".myControl div").html() + letters[counter]);
        counter++;
        if (counter == letters.length) {
            clearInterval(timer);
        }
    }, 50);

}