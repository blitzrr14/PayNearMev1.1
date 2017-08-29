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


    $('#id-this-week').click(function () {
        $('#block').attr("disabled", true);
        $('#id-this-week').attr("checked", true);
        $('#daterange').hide();
        $('#year').val("");
        $('#month').val("");
        //$('#info').hide('slow', function () {
        //    alert("Hello");
        //});
    });

    $('#id-today').click(function () {
        $('#block').attr("disabled", true);
        $('#daterange').hide();
        $('#year').val("");
        $('#month').val("");
        //$('#info').hide('slow', function () {
        //    alert("Hello");
        //});
    });

    $('#id-this-month').click(function () {
        $('#block').attr("disabled", true);
        $('#daterange').hide();
        $('#year').val("");
        $('#month').val("");
        //$('#info').hide('slow', function () {
        //    alert("Hello");
        //});
    });

    $('#id-last-month').click(function () {
        $('#block').attr("disabled", true);
        $('#daterange').hide();
        $('#year').val("");
        $('#month').val("");
        //$('#info').hide('slow', function () {
        //    alert("Hello");
        //});
    });

    $('#id-no-date-range').click(function () {
        $('#block').attr("disabled", true);
        $('#daterange').hide();
        $('#year').val("");
        $('#month').val("");
        //$('#info').hide('slow', function () {
        //    alert("Hello");
        //});
    });

    $('#id-between-dates').click(function () {
        $('#block').attr("disabled", true);
        //$('#info').hide('slow', function () {
        //    alert("Hello");
        //});
    });



    
    $('#block').keyup(function () {
        if ($(this).val() != '') {
            $('#id-this-week').attr("disabled", true);
            $('#id-this-month').attr("disabled", true);
            $('#id-no-date-range').attr("disabled", true);
            $('#id-today').attr("disabled", true);
            $('#id-last-month').attr("disabled", true);
            $('#id-between-dates').attr("disabled", true);
            $('#Tx_Status').attr("disabled", true);

        }
        else {
            $('#id-this-week').attr("disabled", false);
            $('#id-this-month').attr("disabled", false);
            $('#id-no-date-range').attr("disabled", false);
            $('#id-today').attr("disabled", false);
            $('#id-last-month').attr("disabled", false);
            $('#Tx_Status').attr("disabled", false);
            $('#id-between-dates').attr("disabled", false);
        }
    });

    $('#btnReset').click(function () {
        $('#id-this-week').attr("disabled", false);
        $('#id-this-month').attr("disabled", false);
        $('#id-no-date-range').attr("disabled", false);
        $('#id-today').attr("disabled", false);
        $('#id-last-month').attr("disabled", false);
        $('#Tx_Status').attr("disabled", false);
        $('#id-between-dates').attr("disabled", false);
        $('#block').attr("disabled", false);
        $('#input').val('');



        $('#id-this-week').prop("checked", false);
        $('#id-this-month').prop("checked", false);
        $('#id-no-date-range').prop("checked", false);
        $('#id-today').prop("checked", false);
        $('#id-last-month').prop("checked", false);
        $('#Tx_Status').prop("checked", false);
        $('#id-between-dates').prop("checked", false);


    });

    $('#id-between-dates').click(function () {
        $('#daterange').show();
    });


    $('#id-no-date-range').click(function () {
        $('#daterange').hide();
    });






});



function generatePDF() {
    var pdf = new jsPDF('p', 'pt', 'letter');
    // source can be HTML-formatted string, or a reference
    // to an actual DOM element from which the text will be scraped.
    source = $('#generatePDF')[0];

    // we support special element handlers. Register them with jQuery-style 
    // ID selector for either ID or node name. ("#iAmID", "div", "span" etc.)
    // There is no support for any other type of selectors 
    // (class, of compound) at this time.
    specialElementHandlers = {
        // element with id of "bypass" - jQuery style selector
        '#bypassme': function (element, renderer) {
            // true = "handled elsewhere, bypass text extraction"
            return true
        }
    };
    margins = {
        top: 10,
        bottom: 100,
        left: 70,
        width: 1000
    };
    // all coords and widths are in jsPDF instance's declared units
    // 'inches' in this case
    pdf.fromHTML(
    source, // HTML string or DOM elem ref.
    margins.left, // x coord
    margins.top, { // y coord
        'width': margins.width, // max width of content on PDF
        'elementHandlers': specialElementHandlers
    },

    function (dispose) {
        // dispose: object with X, Y of the last line add to the PDF 
        //          this allow the insertion of new lines after html
        pdf.save('Test.pdf');
    }, margins);
}


