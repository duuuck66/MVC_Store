$(function () {

    /* Preview selected image */

    function readURL(input) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();

            reader.onload = function (e) {
                $("img#imgpreview")
                    .attr("src", e.target.result)
                    .width(150)
                    .height(150);
            }

            reader.readAsDataURL(input.files[0]);
        }
    }

    $("#imageUpload").change(function () {
        readURL(this);
    });
});