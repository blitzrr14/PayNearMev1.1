$(document).ready(function () {

    var fpPassIndicator = false,
        fpConfirmPass = false,
        fpNewPassword = false;
    $('input#fpNewPassword').focusout(function (e) {
        var currInput = $(this).val(),
            upperLetter = false,
            smallLetter = false,
            hasNumber = false;
        fpPassIndicator = false;
        fpConfirmPass = false;
        fpNewPassword = false;

        if (currInput == "")
            inputInvalid('fpNewPassword', 'msgPassError', 'Password field is required');
        else if (currInput.length < 8)
            inputInvalid('fpNewPassword', 'msgPassError', 'Password should be atleast 8 characters long');
        else {
            for (var count = 0; (count < currInput.length && (!smallLetter || !hasNumber || !upperLetter)) ; count++) {
                if (!isNaN(currInput[count]))
                { hasNumber = true; }
                else if (currInput[count] == currInput[count].toUpperCase())
                { smallLetter = true; }
                else if (currInput[count] == currInput[count].toLowerCase())
                { upperLetter = true; }
            }

            if (smallLetter && hasNumber && upperLetter) {
                inputValid('fpNewPassword', 'msgPassError');
                fpPassIndicator = true;
                fpNewPassword = true;
            }
            else {
                fpPassIndicator = false;
                fpNewPassword = false;
                inputInvalid('fpNewPassword', 'msgPassError', 'Password must contain atleast 1 lowercase letter, 1 uppercase letter & 1 digit number');
            }
        }

        if (fpPassIndicator) {
            if ($('input#fpConfirmPassword').val() != $('input#fpNewPassword').val()) {
                inputInvalid('fpConfirmPassword', 'msgConfPassError', 'Password mismatch');
            }
            else {
                inputValid('fpConfirmPassword', 'msgConfPassError');
            }
        }
        else {
            $('.msgConfPassError').addClass('hidden');
        }
    });

    $('input#fpConfirmPassword').keyup(function (e) {
        if (fpPassIndicator) {
            if (($(this).val() != $('input#fpNewPassword').val()) || $(this).val() == "" || $('input#fpNewPassword').val() == "") {
                fpConfirmPass = false;
                inputInvalid('fpConfirmPassword', 'msgConfPassError', 'Password mismatch');
            }
            else {
                fpConfirmPass = true;
                inputValid('fpConfirmPassword', 'msgConfPassError');
                $('span.msgPassError').removeClass('glyphicon-remove');
                $('span.msgPassError').removeClass('glyphicon-ok');
            }
        }
    });

    $('form#ForgotPassword-Form').submit(function (e) {
        e.preventDefault();

        if (fpPassIndicator && fpConfirmPass && fpNewPassword) {
            ajxloadtoggle("Confirming new password...");
            $.ajax({
                url: '../ForgotPassword/fpChangePassword',
                type: "POST",
                data: $('form#ForgotPassword-Form').serialize(),
                success: function (data) {
                    if (data.code == 0)
                        fpMsgBox(data.message);
                    else if (data.code == 1) {
                        fpMsgBox(data.message);
                        $('#fpmsgbtn').click(function () {
                            var uri = $("#fpmsgbtn").attr("data-login");
                            window.location.href = uri;
                        });
                    }
                    else {
                        alert(data.message);
                        ajxloadtoggle();
                    }
                },
                error: function (error) {
                    alert("Something went wrong, Please try again!");
                    ajxloadtoggle();
                }
            });
        }
    });
});

var globalEncCID = "", globalEncFN = "";

function fpMsgBox(msg) {
    $('#fpMsgModal').removeClass('hidden');
    document.getElementById('msgContainer').innerHTML = msg;
}

function hideModals() {
    ajxloadtoggle('Loading...');
    ajxloadtoggle();
    $('#fpMsgModal').addClass('hidden')
}

function checkEmail(email) {
    ajxloadtoggle("Verifying...");
    $.ajax({
        url: '../ForgotPassword/CheckEmail',
        type: "POST",
        data: email.serialize(),
        success: function (data) {
            if (data.code == 0)
                fpMsgBox(data.message);
            else if (data.code == 1) {
                requestCode(email, data.encCID, data.encFN, data.isForgot);
                globalEncCID = data.encCID;
                globalEncFN = data.encFN;
            }
            else {
                alert(data.message);
                ajxloadtoggle();
            }
        },
        error: function (error) {
            alert("Something went wrong, Please try again!");
            ajxloadtoggle();
        }
    });
}

function requestCode(email, CID, FN, isFP) {
    ajxloadtoggle("Requesting security code...");
    var actionName = (isFP == false) ? 'RequestCode' : 'ResendCode';
    $.ajax({
        url: '../ForgotPassword/' + actionName,
        type: "POST",
        data: email.serialize() + "&encCID=" + CID + "&encFN=" + FN,
        success: function (data) {
            if (data.code == 0)
                fpMsgBox(data.message);
            else if (data.code == 1) {
                fpMsgBox(data.message);
                fpStep(0);
            }
            else
                ajxloadtoggle();
        },
        error: function (error) {
            alert("Something went wrong, Please try again!");
            ajxloadtoggle();
        }
    });
}

function checkCode(email, securityCode) {
    ajxloadtoggle("Verifying security code...");
    if (securityCode.val().length == 4) {
        $.ajax({
            url: '../ForgotPassword/CheckCode',
            type: "POST",
            data: email.serialize() + '&' + securityCode.serialize() + "&encCID=" + globalEncCID + "&encFN=" + globalEncFN,
            success: function (data) {
                if (data.code == 0)
                    fpMsgBox(data.message);
                else if (data.code == 1) {
                    ajxloadtoggle(data.message);
                    setTimeout(function () { ajxloadtoggle(); }, 1300);
                    fpStep(2);
                }
                else
                    ajxloadtoggle();
            },
            error: function (error) {
                alert("Something went wrong, Please try again!");
                ajxloadtoggle();
            }
        });
    }
    else
        fpMsgBox("Please key in securty code <br /> (4 alphanumeric)");
}

function fpStep(num) {
    var elementID = ['#emailView', '#emailInnerView', '#codeView', '#codeInnerView', '#newpassView', '#newpassInnerView'];

    $(elementID[num]).attr('style', 'border-left: 1px solid gray; border-right: 1px solid gray; border-radius: 10px;');
    $(elementID[num]).removeClass('fpShow');
    $(elementID[num]).addClass('fpHide');
    $(elementID[num + 1]).removeClass('fpInnerShow');
    $(elementID[num + 1]).addClass('fpInnerHide');

    setTimeout(
        function () {
            $(elementID[num]).removeAttr('style');
            $(elementID[num + 2]).attr('style', 'border-left: 1px solid gray; border-right: 1px solid gray; border-radius: 10px;');
            $(elementID[num + 2]).removeClass('fpHide');
            $(elementID[num + 2]).addClass('fpShow');
            $(elementID[num + 3]).removeClass('fpInnerHide');
            $(elementID[num + 3]).addClass('fpInnerShow');
        }
    , 1000);

    setTimeout(
        function () {
            $(elementID[num + 2]).removeAttr('style')
        }
    , 2000);
}