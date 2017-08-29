$(document).ready(function ()
{
    getIPAddress();
 
    var passIndicator = false;
    var emailVal = false;
    var bdate = false;
    var password = false;
    var confirmpass = false;
    var fname = false;
    var address = false;
    var mobile = false;
    var lastname = false;
    var city = false;    
    var zipcode = false;

    $(":input[required][type=text]:visible,select[required]:visible").focusout(function () {
        if ($(this).val() == "")
            $(this).css({"border-color" : "#ff000a", "box-shadow" : "none"});
        else
            $(this).css("border-color", "#ccc");
    });

    $('input#data_firstName').focusout(function (e) {
        if ($(this).val() == "") {
            $(this).css({ "border-color": "#ff000a", "box-shadow": "none" });
            fname = false;
        }
        else {
            $(this).css("border-color", "#ccc");
            fname = true;
        }
    });

    $('input#data_lastName').focusout(function (e) {
        if ($(this).val().length < 2) {
            $(this).css("border-color", "#ff000a");
            lastname = false;
        }
        else {
            $(this).css("border-color", "#ccc");
            lastname = true;
        }
    });

    $("input#data_Street").focusout(function (e) {        
        if ($(this).val().length < 2){
            $(this).css("border-color", "#ff000a");
            address = false;
        }
        else
        {
            $(this).css("border-color", "#ccc");
            address = true;
        }
    });

    $("#data_City").focusout(function (e) {
        if ($(this).val().length < 2) {
            city = false;
            $(this).css("border-color", "#ff000a");
        }
        else {
            $(this).css("border-color", "#ccc");
            city = true;
        }
    });

    $('input#data_UserID').keydown(function (e) {
        if ((e.shiftKey && (e.keyCode == 50 || e.keyCode == 189))   // Allows '@' for 50 and '_' for 189
           ||!e.shiftKey && (e.keyCode == 110 || e.keyCode == 190)) // Allow '.' for 110 & 190
            return;
        else if (isPaste(e) || (!isLetter(e) && !isNumber(e) && !isArrowKeys(e) && !isBackSpace(e) && !isDelete(e) && !isTab(e)))
            e.preventDefault();
    });

    $('input#data_UserID').focusout(function (e) {
        var currInput = $(this).val(),
              atIndex = currInput.indexOf('@'),
          checkDomain = currInput.slice(atIndex),
          checkDomDot = checkDomain.indexOf('.');

        if (currInput == '')
        {
            emailVal = false;
            inputInvalid('data_UserID', 'msgEmailError', 'E-mail Address field is required');
        }
        else if (atIndex == -1 || atIndex == 0 || checkDomDot < 2 //if @ isn't found or if email starts w/ @ or if the dot after @ isn't found by atleast the next char
                || currInput.split('').reverse().join('').indexOf('.') < 2){ // or if domain is less than 2 characters
            emailVal = false;
            inputInvalid('data_UserID', 'msgEmailError', 'E-mail Address is not valid');
        }
        else {
            emailVal = true;
                inputValid('data_UserID', 'msgEmailError');
        }
    });

    $('input#password').focusout(function (e) {
        var currInput = $(this).val()        

        if (currInput == "")
            inputInvalid('password', 'msgPassError', 'Password field is required');
        else if (currInput.length < 8)
            inputInvalid('password', 'msgPassError', 'Password should be atleast 8 characters long');
        else
        {   for (var count = 0, hasSmallLetter = false, hasNumber = false; 
                    (count < currInput.length && (!hasSmallLetter || !hasNumber));
                    count++)
            {
                if (currInput[count] >= 'a' || currInput[count] >= 'z')
                { hasSmallLetter = true; }
                else if (!isNaN(currInput[count]))
                { hasNumber = true; }
            }

            if (hasSmallLetter && hasNumber) {
                inputValid('password', 'msgPassError');
                document.getElementById("confirm_password").focus();
                passIndicator = true;
                password = true;
            }
            else {
                passIndicator = false;
                password = false;
                inputInvalid('password', 'msgPassError', 'Password must contain atleast 1 letter & 1 digit number');
            }
        }

        if (passIndicator) {
            if ($('input#confirm_password').val() != $('input#password').val()) {
                inputInvalid('confirm_password', 'msgConfPassError', 'Password mismatch');
            }
            else {
                inputValid('confirm_password', 'msgConfPassError');
            }
        }
    });

    $('input#confirm_password').focusout(function (e) {
        if (passIndicator) {
            if (($(this).val() != $('input#password').val()) || $(this).val() == "" || $('input#password').val() == "") {
                confirmpass = false;
                inputInvalid('confirm_password', 'msgConfPassError', 'Password mismatch');
            }
            else {
                confirmpass = true;
                inputValid('confirm_password', 'msgConfPassError');
                $('span.msgPassError').removeClass('glyphicon-remove');
                $('span.msgPassError').removeClass('glyphicon-ok');
            }
        }
    });

    $('input#birthdate').keydown(function (e) {
        var currentInput = $(this).val();        
        if ((!isNumber(e) && !isBackSpace(e) && !isArrowKeys(e) && !isTab(e) && !isDelete(e) && !isSelectAll(e))
            || ((e.keyCode == 48 || e.keyCode == 96) && currentInput.length == 1 && currentInput[0] == '0')
            || currentInput.length > 10)
            e.preventDefault();
        else if ((currentInput.length == 2 || currentInput.length == 5) && !isBackSpace(e))
            $('input#birthdate').val(currentInput + '/');
    });

    $('input#ExpiryDate').keydown(function (e) {
        var currentInput = $(this).val();
        if ((!isNumber(e) && !isBackSpace(e) && !isArrowKeys(e) && !isTab(e) && !isDelete(e) && !isSelectAll(e))
            || ((e.keyCode == 48 || e.keyCode == 96) && currentInput.length == 1 && currentInput[0] == '0')
            || currentInput.length > 10)
            e.preventDefault();
        else if ((currentInput.length == 2 || currentInput.length == 5) && !isBackSpace(e))
            $('input#ExpiryDate').val(currentInput + '/');
    });

    $('input#birthdate').focusout(function (e) {
        var bDateVal = $(this).val();
            bdate = false;

        if (bDateVal[2] != '/' || bDateVal[5] != '/') {
            $(this).val('');
            $(this).focus();
            inputInvalid('birthdate', 'msgBDateError', 'Invalid date format');
        }
        else if (bDateVal.length < 10 ||
            parseInt(bDateVal.slice(0, 2)) > 12 || //if month > 12
            parseInt(bDateVal.slice(3, 5)) < 01 || //if days < 01
            parseInt(bDateVal.slice(3, 5)) > 31    //if days > 31
           ) {
            inputInvalid('birthdate', 'msgBDateError', 'Invalid date format');
        }
        else if (parseInt((new Date().getFullYear()) -
                 parseInt(bDateVal.slice(6))) < 18)
        { inputInvalid('birthdate', 'msgBDateError', 'You must be over 17 years old'); }
        else if (parseInt((new Date().getFullYear()) -
                 parseInt(bDateVal.slice(6))) > 200)
        { inputInvalid('birthdate', 'msgBDateError', 'Is this correct?'); }
        else {
            inputValid('birthdate', 'msgBDateError');
            bdate = true;
        }
    });

    $('input#cellPhone').keydown(function (e) {
        var currentInput = $(this).val();
        if ((!isNumber(e) && !isBackSpace(e) && !isArrowKeys(e) && !isTab(e) && !isDelete(e) && !isSelectAll(e))
            || currentInput.length > 10)
            e.preventDefault();
    });

    $('input#cellPhone').focusout(function (e) {
        if ($(this).val().length < 10) {
            $('.msgMobile').css("border-color", "#ff000a");
            $('div#isdCodeId').css("color", "#ff000a");
            $('div#isdCodeId').css("background-color", "#fee");
            mobile = false;
        }
        else {
            $('.msgMobile').css("border-color", "green");
            $('div#isdCodeId').css("color", "black");
            $('div#isdCodeId').css("background-color", "#e5ffe5");
            mobile = true;
        }
    });

    $('input#zipCode').keydown(function (e) {        
        if ((!isNumber(e) && !isBackSpace(e) && !isArrowKeys(e) && !isTab(e) && !isDelete(e) && !isSelectAll(e))
            || e.length > 9)
            e.preventDefault();
    });

    $('input#ZipCode').focusout(function (e) {
       
        if ($(this).val().length < 5) {
            $(this).css("border-color", "#ff000a");
            $('#msgdata_zipcode').text('Must be 5 digits');
            zipcode = false;
        }
        else {
            ajxloadtoggle();
            $.ajax({
                url: '../api/WebService/json/getState/?zipCode=' + $(this).val() + '',
                type: "GET" ,
                success: function (data) {
                    if (data.respcode == 1) {
                        $('#data_State').val(data.zCodeResp.state);
                        $('#data_City').val(data.zCodeResp.city);
                        $('input#zipCode').css("border-color", "#ccc");
                        $('#msgdata_zipcode').addClass('hidden');
                        zipcode = true;
                    }
                    else {
                        $('#msgdata_zipcode').text(data.message);
                        $('#msgdata_zipcode').removeClass('hidden');
                        $('input#zipCode').css("border-color", "#ff000a");
                        $('input#zipCode').val('');
                        $('#data_address_stateName').val('');
                        $('#data_address_city').val('');
                        zipcode = false;
                    }
                    ajxloadtoggle();
                },
                error: function (error) {
                    alert("Something went wrong , Please try again!");
                    $('input#zipCode').css("border-color", "#ff000a");
                    zipcode = false;
                    ajxloadtoggle();
                }
            });
        }
    });

    $('input.naming').keypress(function (e) {
        //For keypress event listener
        //isLetterPress = lowercase, isLetter = uppercase
        if(!isLetterPress(e) && !isLetter(e) && !isSpace(e))
            e.preventDefault();
    });

    $('div#displayPolicyTrigger').click(function (e) {
        if (document.getElementById('privacyPolicyAgreement').checked == false) {
            $('span#displayPolicyChecker').removeClass('hidden');
            document.getElementById('privacyPolicyAgreement').checked = true;
        }
        else {
            $('span#displayPolicyChecker').addClass('hidden');
            document.getElementById('privacyPolicyAgreement').checked = false;
        }
    });

    $("#btnSignUp").click(function () {        
        if (fieldCheck()) {
            ajxloadtoggle();
            $.ajax({
                url: $(this).attr("data-register"),
                data: $("#register-form").serialize(),
                success: function (captchaValidity) {
                    if (captchaValidity) {
                        
                        $("#signup-data").hide();
                        $("#confirm-data").show("slide");
                        $("#lbl-emailAddress").text($("#data_UserID").val());
                        $("#lbl-bDate").text($("#birthdate").val());
                        $("#lbl-mobile").text($("#cellPhone").val());
                        $("#lbl-Name").text($("#data_firstName").val() + " " + $("#data_middleName").val() + " " +
                                            $("#data_lastName").val());
                    }
                    else {
                        
                        grecaptcha.reset();
                    }
                    ajxloadtoggle();
                },
                error: function (captchaValidity) {
                    alert('An error has occured!');
                    ajxloadtoggle();
                    return false;
                }
            });
        }
    });

    $("#btnEdit-data").click(function ()
    {
        $("#signup-data").show("slide");
        $("#confirm-data").hide("slide");
        grecaptcha.reset();
    });

    $("#closeModal").click(function () {
        $("#myModal").hide();
    });

    $("#auth-finish").click(function () {

        var url = $(this).attr("data-authenticate");
        var model = $("#authenticate-form").serialize();
        var home = $(this).attr("data-home");
        var auth = $(this).attr("data-auth");

        if ($("#userId").val() != "" && $("#authenticationCode").val() != "") {
            ajxloadtoggle();
            $.ajax(
            {
                url: url,
                data: model,
                type: 'POST',
                success: function (res) {
                    if (res.respcode == 1) {
                        window.location.href = home;
                    }
                    else if (res.respcode == 2) {
                        window.location.href = auth;
                    }
                    else
                    {
                        errModal(res.message);
                    }
                    ajxloadtoggle();
                },
                error: function (res) {
                    ajxloadtoggle();
                    errModal(res);
                }
            });
        }
        else
        {
            errModal("Please input all required fields!");
        }
    });

    $("#btn-resend").click(function () {

        var url = $(this).attr("data-resend");
        var model = $("#authenticate-form").serialize();
     

        if ($("#userId").val() != "") {
            ajxloadtoggle();
            $.ajax(
             {
                 url: url,
                 data: model,
                 type: 'POST',
                 success: function (res) {
                     errModal(res);
                     ajxloadtoggle();
                 },
                 error: function (res) {
                     errModal(res);
                     ajxloadtoggle();
                 }
             });
        }
        else
        {
            errModal("Please Input Email Address!");
        }
    });

    //image verify & upload feature

    $("#btnUpImg").click(function () {
        if ($('#mainUIDisabler').hasClass('imgUpHide')) {
            $('#mainUIDisabler').addClass('imgUpShow');
            $('#mainUIDisabler').removeClass('imgUpHide');

            $('#UpImgTray').addClass('modalUpImgShow');
            $('#UpImgTray').removeClass('modalUpImgHide');
        }
        else {
            $('#mainUIDisabler').addClass('imgUpHide');
            $('#mainUIDisabler').removeClass('imgUpShow');

            $('#UpImgTray').addClass('modalUpImgHide');
            $('#UpImgTray').removeClass('modalUpImgShow');
        }
    });

    $("#btnXImgUp").click(function () {

        $('#selfPortrait').attr('src', '../Images/glyphicon-user.PNG');
        $("#slfprt").val('');

        $('#mainUIDisabler').addClass('imgUpHide');
        $('#mainUIDisabler').removeClass('imgUpShow');

        $('#UpImgTray').addClass('modalUpImgHide');
        $('#UpImgTray').removeClass('modalUpImgShow');
    });

    $("#slfprt").change(function () {
        readURL(this);
    });

    $("#btnvrfyImgs").click(function () {
        //alert('Verifying Images...');
        $('#mainUIDisabler').addClass('imgUpHide');
        $('#mainUIDisabler').removeClass('imgUpShow');

        $('#UpImgTray').addClass('modalUpImgHide');
        $('#UpImgTray').removeClass('modalUpImgShow');
    });
    //-----------------------------------------------
    

    function errModal(message)
    {
        $("#modContent").html(message);
        $("#myModal").show();
    }

    function fieldCheck() {
        var fieldList = [$("#data_UserID"), $("#password"), $("#confirm_password"), $("#data_firstName"), $("#data_lastName"),
                         $("#birthdate"), $("input#cellPhone"), $("#data_Street"), $("#zipCode"), $("#data_City"),
                         $("#data_State")];
                         //$("#privacyPolicyAgreement"), $("#recaptcha-anchor")];
     
        var isfieldInputCorrect = [emailVal, password, confirmpass, fname, lastname, bdate, mobile, address, zipcode, true, true];
        
        for (var count = 0; count < fieldList.length; count++) {
            if (fieldList[count].val() == "" || !isfieldInputCorrect[count]) {
                fieldList[count].focus();
                $("#errorMsgDiv").text("Please input required fields!");
                return false;
            }
        }

        if (!$("#privacyPolicyAgreement").is(":checked")) {
            $('div#MLpolicyAgreement').css('border', '1px solid red');
            //privacyPolicyAgreement.focus();
            $('div#displayPolicyTrigger').focus();
            $("#errorMsgDiv").text("Please input required fields!");
            return false;
        }
        else
            $('div#MLpolicyAgreement').css('border', 'none');

        if ($('#g-recaptcha-response').val() == "") {
            $('div#google-recaptcha').css('border', '2px solid red');
            return false;
        }
        else
            $('div#google-recaptcha').css('border', 'none');

        $("#errorMsgDiv").text("");
        return true;
    }
});

function inputInvalid(elemId, cLass, msg)
{
    if (cLass != '') {
        $('.' + cLass).removeClass('hidden');
        $('span.' + cLass).addClass('glyphicon-remove');
    }
    if (elemId != '') {
        $('input#' + elemId).css("border-color", "#ff000a");
        document.getElementById('msg' + elemId).innerHTML = msg;
    }
}
function inputValid(elemId, cLass)
{
    if (cLass != '') {
        $('div.' + cLass).addClass('hidden');
        $('span.' + cLass).removeClass('glyphicon-remove');
        $('span.' + cLass).addClass('glyphicon-ok');
    }
    if (elemId!= '') {
        $('input#' + elemId).css("border-color", "#ccc");
        document.getElementById('msg' + elemId).innerHTML = '';
    }
}

//--for Keyup/down focusin/out--------------------\/
function isArrowKeys(e) {
    return e.keyCode >= 33 && e.keyCode <= 40 ? true : false;
}
function isSpace(e) {
    return e.keyCode == 32 ? true : false;
}
function isBackSpace(e) {
    return e.keyCode == 8 ? true : false;
}
function isEnter(e) {
    return e.keyCode == 13 ? true : false;
}
function isEscape(e) {
    return e.keyCode == 27 ? true : false;
}
function isTab(e) {
    return e.keyCode == 9 ? true : false;
}
function isDelete(e) {
    return e.keyCode == 46 ? true : false;
}
function isNumber(e) {
    return !e.shiftKey &&
           ((e.keyCode >= 48 && e.keyCode <= 57) ||
           (e.keyCode >= 96 && e.keyCode <= 105))
           ? true : false;
}
function isLetter(e) {
    return e.keyCode >= 65 && e.keyCode <= 90 ? true : false;
}
function isSelectAll(e) {
    return e.ctrlKey && e.keyCode == 65 ? true : false;
}
function isCopy(e) {
    return e.ctrlKey && e.keyCode == 67 ? true : false;
}
function isPaste(e) {
    return e.ctrlKey && e.keyCode == 86 ? true : false;
}
function isCut(e) {
    return e.ctrlKey && e.keyCode == 88 ? true : false;
}
//------------------------------------------------/\


//--for keypress----------------------------------\/
function isLetterPress(e) { //for lowercase letters, isLetter is uppercase
    return e.keyCode >= 97 && e.keyCode <= 122 ? true : false;
}


function openUrlSecurityNewTab(varI,url)
{
    var uri = $(varI).attr("data-useragree");
    var server;
    
    ajxloadtoggle();
    $.ajax(
            {   
                url: uri,
                type: 'GET',
                success: function (res) {
                    server = res+url;
                    ajxloadtoggle();
                    window.open(server, '_blank');
                },
                error: function (res) {
                    alert(res);
                    ajxloadtoggle();
                }
            });
}

function getIPAddress() {
    $.getJSON('//api.ipify.org?format=jsonp&callback=?', function (data) {
        $("#userIPAddress").val(data.ip);
    });
}

// 5mins popup
function noBack() {
    window.history.forward();
}


function readURL(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#selfPortrait').attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
    else
        $('#selfPortrait').attr('src', '../Images/glyphicon-user.PNG');
}

