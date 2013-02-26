var id1 = null;
function addChapterBinding(id1, id2) {

    alert("../../Alignment/CreateChapter/" + id1 + "/" + id2)
    $.ajax({
        //type: "POST",
        url: "../../Alignment/CreateChapter/" + id1 + "/" + id2,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        cache: false,
        /*data: {
            id1: id1,
            id2: id2
        },*/
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
    $(".chapter").click(function (e) {
        e.preventDefault();
        alert($(this).attr('id'))
        if (id1 == null) {
            id1 = $(this).attr('href');
            id1 = id1.substring(1, id1.length);
        } else {
            id2 = $(this).attr('href');
            id2 = id2.substring(1, id2.length);
            addChapterBinding(id1, id2);
            id1 = null
        }
    });
    $(".chapter").dblclick(function (e) {
    });
});