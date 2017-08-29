$(document).ready(function () {

    var destinationCountry = false;
    var paymentCurrency = false;
    var receptionMethod = false;
    var firstName = false;
    var lastName = false;
    var middleName = false;
    var city = false;
    var province = false;
    var zipcode = false;
    var dateOfBirth = false;
    var phoneNo = false;
    var email = false;

    $('#destinationCountry').focusout(function () {
        if ($('#destinationCountry').val() == "") {
            $('#destinationCountry').css({ "border-color": "#b94a48", "box-shadow": "none" });
            destinationCountry = false;
        }
        else {
            $('#destinationCountry').css("border-color", "#ccc");
            destinationCountry = true;
        }
    });

    $('#paymentCurrency').focusout(function () {
        if ($('#paymentCurrency').val() == "") {
            $('#paymentCurrency').css({ "border-color": "#b94a48", "box-shadow": "none" });
            paymentCurrency = false;
        }
        else {
            $('#paymentCurrency').css("border-color", "#ccc");
            paymentCurrency = true;
        }
    });

    $('#receptionMethod').focusout(function () {
        if ($('#receptionMethod').val() == "") {
            $('#receptionMethod').css({ "border-color": "#b94a48", "box-shadow": "none" });
            receptionMethod = false;
        }
        else {
            $('#receptionMethod').css("border-color", "#ccc");
            receptionMethod = true;
        }
    });

    $('#firstName').focusout(function () {
        if ($('#firstName').val() == "") {
            $('#firstName').css({ "border-color": "#b94a48", "box-shadow": "none" });
            firstName = false;
        }
        else {
            $('#firstName').css("border-color", "#ccc");
            firstName = true;
        }
    });

    $('#lastName').focusout(function () {
        if ($('#lastName').val() == "") {
            $('#lastName').css({ "border-color": "#b94a48", "box-shadow": "none" });
            lastName = false;
        }
        else {
            $('#lastName').css({ "border-color": "#ccc" });
            lastName = true;
        }
    });

    $('#city').focusout(function () {
        if ($('#city').val() == "") {
            $('#city').css({ "border-color": "#b94a48", "box-shadow": "none" });
            city = false;
        }
        else {
            $('#city').css({ "border-color": "#ccc" });
            city = true;
        }
    });

    $('#province').focusout(function () {
        if ($('#province').val() == "") {
            $('#province').css({ "border-color": "#b94a48", "box-shadow": "none" });
            province = false;
        }
        else {
            $('#province').css({ "border-color": "#ccc" });
            province = true;
        }
    });

    $('#continueButton').click(function () {
        if ($('#addBeneForm').valid()) {
            $("p").click(function () {
                alert("The paragraph was clicked.");
            });

            $('#add_newbeneficiarymain').hide();
            $('#BeneInfo').show();
        }
        //$('#paymentmethod').val() == $('#receptionMethod').val();
    });

    $('#edit').click(function () {
        $('#BeneInfo').hide();
        $('#add_newbeneficiarymain').show();
    });

    $('#finish').click(function () {
        $('#BeneInfo').hide();
        $('#BeneList').show();
    });

    $('#addBeneInBeneList').click(function () {
        $('#BeneList').hide();
        $('#add_newbeneficiarymain').show();
    });

    $('.editBene').click(function () {
        $('#BeneList').hide();
        $('#add_newbeneficiarymain').show();
    });

    $('input[name^="checkboxForEmail"]').click(function (e) {
        if ($(this).is(":checked")) {
            $("#emailcontainer").show();
            $("#emailAddress").focus();
        }
        else {
            $("#emailcontainer").hide();
        }
    });

    $('input[name^="checkboxForSms"]').click(function (e) {
        if ($(this).is(":checked")) {
            if ($('#phoneNo').val() == "") {
                $("#smsContainer").show();
                $('#phoneNo').css({ "border-color": "#b94a48", "box-shadow": "none" });
            }
        } else {
            $('#phoneNo').css({ "border-color": "#ccc" });
            $("#smsContainer").hide();
        }
    });

    $('#continue').click(function () {
        if ($('#BeneForm').valid()
            && dateOfBirth
            && ($('#idIsEmail').prop('checked') == email)) {

            //Display Data
            $('#Bfname').text($('#firstName').val());
            $('#Bmname').text($('#middleName').val());
            $('#Blname').text($('#lastName').val());
            $('#Bcity').text($('#city').val());
            $('#Bprovince').text($('#province').val());
            $('#Bcountry').text($('#destinationCountry').val());
            $('#BdateOfBirth').text($('#dateOfBirth').val());
            $('#Bgender').text($('#rcvrgender').val());
            $('#Bzipcode').text($('#zipcode').val());
            $('#Brelation').text($('#relation').val());
            $('#Bphone').text('+63' + $('#phoneNo').val());
            $('#BpaymentCurrency').text($('#paymentCurrency').val());
            $('#BreceptionMethod').text($('#receptionMethod').val());

            //Model Data
            $('#Bfname1').val($('#firstName').val());
            $('#Bmname1').val($('#middleName').val());
            $('#Blname1').val($('#lastName').val());
            $('#Bcity1').val($('#city').val());
            $('#Bprovince1').val($('#province').val());
            $('#Bcountry1').val($('#destinationCountry').val());
            $('#BdateOfBirth1').val($('#dateOfBirth').val());
            $('#Bgender1').val($('#rcvrgender').val());
            $('#Bzipcode1').val($('#zipcode').val());
            $('#Brelation1').val($('#relation').val());
            $('#Bphone1').val($('#phoneNo').val());
            $('#BpaymentCurrency1').val($('#paymentCurrency').val());
            $('#BreceptionMethod1').val($('#receptionMethod').val());
            $('#Bcountry2').val($('#destinationCountry').val());
            $('#rcvrEmail1').val($('#rcvrEmail').val());

            $('#sms').attr('checked', $('#idIsSMS').prop('checked'));
            $('#email').attr('checked', $('#idIsEmail').prop('checked'));

            $('#add_newbeneficiarymain').hide();
            $('#BeneInfo').show();
        }
        //$('#paymentmethod').val($('#receptionMethod').val());
    });

    //Trappings - Khevin
    $('input#dateOfBirth')
    .keydown(function (e) {
        inputValid('dateOfBirth', 'msgBDateError')
        var currentInput = $(this).val();
        if ((!isNumber(e) && !isBackSpace(e) && !isArrowKeys(e) && !isTab(e) && !isDelete(e) && !isSelectAll(e))
            || ((e.keyCode == 48 || e.keyCode == 96) && currentInput.length == 1 && currentInput[0] == '0')
            || currentInput.length > 10)
            e.preventDefault();
        else if ((currentInput.length == 2 || currentInput.length == 5) && !isBackSpace(e))
            $('input#dateOfBirth').val(currentInput + '/');
    })
    .focusout(function (e) {
        var bDateVal = $(this).val();
        dateOfBirth = false;

        if (bDateVal != "") {
            if (bDateVal == '') {
                inputInvalid('dateOfBirth', 'msgBDateError', '');//'Birth Date Required!');
            }
            else if (bDateVal[2] != '/' || bDateVal[5] != '/') {
                $(this).val('');
                inputInvalid('dateOfBirth', 'msgBDateError', 'Invalid date format');
            }
            else if (bDateVal.length < 10
                    || parseInt(bDateVal.slice(0, 2)) > 12 //if month > 12
                    || parseInt(bDateVal.slice(3, 5)) < 01 //if days < 01
                    || parseInt(bDateVal.slice(3, 5)) > 31 //if days > 31
                    ) {
                inputInvalid('dateOfBirth', 'msgBDateError', 'Invalid date format');
            }
            else if (parseInt((new Date().getFullYear()) -
                        parseInt(bDateVal.slice(6))) < 18)
            { inputInvalid('dateOfBirth', 'msgBDateError', 'You must be over 17 years old'); }
            else if (parseInt((new Date().getFullYear()) -
                        parseInt(bDateVal.slice(6))) > 200)
            { inputInvalid('dateOfBirth', 'msgBDateError', 'Is this correct?'); }
            else {
                inputValid('dateOfBirth', 'msgBDateError');
                dateOfBirth = true;
            }
        }
    });

    $('input#phoneNo')
    .keydown(function (e) {
        inputValid('phoneNo', 'msgPhoneError');
        $("#smsContainer").hide();
        var currentInput = $(this).val();
        if ((!isNumber(e) && !isBackSpace(e) && !isArrowKeys(e) && !isTab(e) && !isDelete(e) && !isSelectAll(e))
            || currentInput.length > 10)
            e.preventDefault();
    })
    .focusout(function (e) {
        if ($(this).val() == '') {
            inputInvalid('phoneNo', 'msgPhoneError', '')
            phoneNo = false;
        }
        else if ($(this).val().length < 10) {
            phoneNo = false;
        }
        else {
            inputValid('phoneNo', 'msgPhoneError')
            phoneNo = true;
        }
    });

    $('input#rcvrEmail')
    .keydown(function (e) {
        inputValid('rcvrEmail', 'msgEmailError');
        if ((e.shiftKey && (e.keyCode == 50 || e.keyCode == 189))   // Allows '@' for 50 and '_' for 189
           || !e.shiftKey && (e.keyCode == 110 || e.keyCode == 190)) // Allow '.' for 110 & 190
            return;
        else if (isPaste(e) || (!isLetter(e) && !isNumber(e) && !isArrowKeys(e) && !isBackSpace(e) && !isDelete(e) && !isTab(e)))
            e.preventDefault();
    })
    .focusout(function (e) {
        var currInput = $(this).val(),
              atIndex = currInput.indexOf('@'),
          checkDomain = currInput.slice(atIndex),
          checkDomDot = checkDomain.indexOf('.');

        if (currInput == '') {
            email = false;
            inputInvalid('rcvrEmail', 'msgEmailError', '');
        }
        else if (atIndex == -1 || atIndex == 0 || checkDomDot < 2           // if @ isn't found or if email starts w/ @ or if the dot after @ isn't found by atleast the next char
               || currInput.split('').reverse().join('').indexOf('.') < 2) { // or if domain is less than 2 characters
            inputInvalid('rcvrEmail', 'msgEmailError', 'E-mail Address is not valid');
            email = false;
        }
        else {
            inputValid('rcvrEmail', 'msgEmailError');
            email = true;
        }
    });

    $('input#zipcode')
    .keydown(function (e) {
        if ((!isNumber(e) && !isBackSpace(e) && !isArrowKeys(e) && !isTab(e) && !isDelete(e) && !isSelectAll(e))
            || ($(this).val().length > 9 && !isBackSpace(e)))
            e.preventDefault();
    })
    .focusout(function () {
        if ($(this).val() == "") {
            $(this).css({ "border-color": "#b94a48", "box-shadow": "none" });
            zipcode = false;
        }
        else {
            $(this).css({ "border-color": "#ccc" });
            zipcode = true;
        }
    });
});



function isNumber(e) {
    return !e.shiftKey &&
           ((e.keyCode >= 48 && e.keyCode <= 57) ||
           (e.keyCode >= 96 && e.keyCode <= 105))
           ? true : false;
}

function isBackSpace(e) {
    return e.keyCode == 8 ? true : false;
}

function isArrowKeys(e) {
    return e.keyCode >= 33 && e.keyCode <= 40 ? true : false;
}

function isTab(e) {
    return e.keyCode == 9 ? true : false;
}

function isDelete(e) {
    return e.keyCode == 46 ? true : false;
}

function isSelectAll(e) {
    return e.ctrlKey && e.keyCode == 65 ? true : false;
}

function isPaste(e) {
    return e.ctrlKey && e.keyCode == 86 ? true : false;
}

function isLetter(e) {
    return e.keyCode >= 65 && e.keyCode <= 90 ? true : false;
}

function inputInvalid(elemId, cLass, msg) {
    if (cLass != '') {
        $('.' + cLass).removeClass('hidden');
        $('span.' + cLass).addClass('glyphicon-remove');
    }
    if (elemId != '') {
        $('input#' + elemId).css("border-color", "#b94a48");
        document.getElementById('msg' + elemId).innerHTML = msg;
    }
}

function inputValid(elemId, cLass) {
    if (cLass != '') {
        $('div.' + cLass).addClass('hidden');
        $('span.' + cLass).removeClass('glyphicon-remove');
        $('span.' + cLass).addClass('glyphicon-ok');
    }
    if (elemId != '') {
        $('input#' + elemId).css("border-color", "#ccc");
        document.getElementById('msg' + elemId).innerHTML = '';
    }
}