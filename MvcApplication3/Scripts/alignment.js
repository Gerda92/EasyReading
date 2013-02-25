var id1 = null;
alert();
function addChapterBinding(id1, id2) {
    $.ajax({
        type: "POST",
        url: "../../Alignment/CreateChapterBinding/",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: {
            id1: id1,
            id2: id2
        },
        success: function (data) {
            alert(data);
        },
        error: function () {
            alert('Some kind of error');
        },
        complete: function () {

        }
    });
}

$(document).ready(function () {
    $(".chapter").dblclick(function (e) {
        e.preventDefault();
        if (id1 == null) id1 = $(this).attr('data-id');
        else {
            addChapterBinding(id1, $(this).attr('data-id'));
            id1 = null
        }
    });
});