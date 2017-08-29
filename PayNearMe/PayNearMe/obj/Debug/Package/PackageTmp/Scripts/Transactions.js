//$('#text').keypress(function (e) {
//    var regex = new RegExp("^[a-zA-Z0-9-]+$");
//    var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
//    if (regex.test(str)) {
//        return true;
//    }

//    e.preventDefault();
//    return false;
//});

$(document).ready(function () {
    $("#table1").freezeHeader({ 'height': '250px' });
    $("#table2").freezeHeader();
    $("#tbex1").freezeHeader();
    $("#tbex2").freezeHeader();
    $("#tbex3").freezeHeader();
    $("#tbex4").freezeHeader();


    $('#block').bind('keypress', function (e) {

        if ($('#block').val().length == 0) {
            var k = e.which;
            var ok = k >= 65 && k <= 90 || // A-Z
                k >= 97 && k <= 122 || // a-z
                k >= 48 && k <= 57; // 0-9

            if (!ok) {
                e.preventDefault();
            }
        }
    });
})