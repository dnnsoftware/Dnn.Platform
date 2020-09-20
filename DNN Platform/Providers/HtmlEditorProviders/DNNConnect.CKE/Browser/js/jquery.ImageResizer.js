function ResizeMe(element, maxWidth, maxHeight) {
    $(element).each(function () {
        $(this).load(function () {
            $(this).css("width", "auto").css("height", "auto");
            $(this).removeAttr("width").removeAttr("height");
            var width = $(this).width();
            var height = $(this).height();
            if (width > maxWidth) {
                var ratio = maxWidth / width;
                $(this).css("width", maxWidth);
                $(this).css("height", height * ratio);
                height = height * ratio
            }
            if (height > maxHeight) {
                var ratio = maxHeight / height;
                $(this).css("height", maxHeight);
                $(this).css("width", width * ratio);
                width = width * ratio
            }
        })
    })
}